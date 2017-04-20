using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DCP.Geosupport.DotNet.GeoX;

namespace Geocoder
{
    public partial class Form1 : Form
    {
        List<String[]> addresses = new List<String[]>();
        Wa1 mywa1 = new Wa1();
        Wa2F1ex mywa2 = new Wa2F1ex();
        GeoConnCollection myGeoConns = new GeoConnCollection("C:\\Users\\Lior\\Documents\\Visual Studio 2013\\Projects\\Geocoder\\Geocoder\\GeoConns.xml");
        StringBuilder csv = null;

        int[] desiredHeaderIndices = new int[2];
        List<string> header_array = new List<string>();

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            geo myGeo = new geo(myGeoConns);
            string filename = null;

            if (addresses.Count == 0)
            {
                //Load the address input file
                OpenFileDialog dlg = new OpenFileDialog();
                dlg.DefaultExt = ".csv";
                dlg.Filter = "CSV Files (.csv)|*.csv";
                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    filename = dlg.FileName;
                    string file_text = File.ReadAllText(filename);
                    char[] sep = { '\n' };
                    string[] rows = file_text.Split(sep);
                    int i = 0;
                    foreach (string x in rows)  
                    {
                        char[] line_sep = {','};
                        string[] next_row = x.Split(line_sep);
                        if (i != 0) //skip headers
                        {
                            addresses.Add(next_row);
                        }
                        i++;
                    }
                    Console.WriteLine("Number of addresses loaded: " + addresses.Count + "\n");
                }
            }
            else
            {
                MessageBox.Show("Addresses have already been loaded");
            }

            if (addresses.Count > 2)
            {
                mywa1.Clear();
                mywa1.in_func_code = "1E";
                mywa1.in_mode_switch = "X";
                mywa1.in_platform_ind = "C";

                csv = new StringBuilder();
                var headerLine = string.Format("{0},{1},{2},{3},{4},{5},{6}{7}", "Address", "Borocode", "Latitude", "Longitude","BIN", "BBL", "Zipcode", Environment.NewLine);
                csv.Append(headerLine);
                //Console.WriteLine(String.Join(",", header_array));

                int missed = 0;
                int hit = 0;

                foreach (string[] address in addresses)
                {
                    //Console.WriteLine(String.Join(" | ",address));
                    //Console.WriteLine(address.Length);

                    //string borough_string = null;

                   if (address.Length > 1)
                    {
                        mywa1.in_b10sc1.boro = address[1];
                        mywa1.in_hnd = address[0].Split()[0];
                        //mywa1.in_hnd = address[0];
                        List<string> address_string_list = new List<string>(address[0].Split().Skip(1));
                        //List<string> address_string_list = new List<string>(address[0].Split());
                        string address_string = String.Join(" ", address_string_list);
                        mywa1.in_stname1 = address_string;
                        myGeo.GeoCall(ref mywa1, ref mywa2);

                            if (mywa1.out_grc == "00")
                            {
                                hit++;
                                Console.WriteLine(hit);
                                var newLine = string.Format("{0},{1},{2},{3},{4},{5},{6}{7}", address[0], address[1].Trim(), mywa2.latitude, mywa2.longitude, mywa1.out_bin.BINToString(), mywa1.out_bbl.BBLToString(), mywa2.zip_code, Environment.NewLine);
                                csv.Append(newLine);
                                Console.WriteLine("Missed: " + missed + "\tHit:" + hit);

                            }
                            else if (mywa1.out_grc == "01")
                            {
                                hit++;
                                Console.WriteLine(hit);
                                var newLine = string.Format("{0},{1},{2},{3},{4},{5},{6}{7}", address[0], address[1].Trim(), mywa2.latitude, mywa2.longitude, mywa1.out_bin.BINToString(), mywa1.out_bbl.BBLToString(), mywa2.zip_code, Environment.NewLine);
                                csv.Append(newLine);
                                Console.WriteLine("Missed: " + missed + "\tHit:" + hit);
                            }
                            else
                            {
                                //Console.WriteLine(address[0]);
                                missed++;
                                Console.WriteLine("Missed: " + missed + "\tHit:" + hit);
                                //Console.WriteLine(missed);
                                Console.WriteLine(String.Join(" | ", address));
                                Console.WriteLine("\n");
                                //Console.WriteLine(address[5]+" "+address[6]);
                                //Console.WriteLine();
                                /*Console.WriteLine("Unsuccessful");
                                Console.WriteLine(String.Join(" | ", address).Trim());
                                Console.WriteLine(mywa1.out_grc);
                                Console.WriteLine(mywa1.out_error_message);*/
                                //var newLine = String.Join(",", address).Trim()+Environment.NewLine;
                                var newLine = string.Format("{0},{1},{2},{3},{4},{5},{6}{7}", address[0], address[1].Trim(), "NOT FOUND", "NOT FOUND", "NOT FOUND", "NOT FOUND", "NOT FOUND", Environment.NewLine);
                                csv.Append(newLine);
                            }
                        }
                }
                
                MessageBox.Show("Great! Now choose where the output error file must be saved.\nDo not add the file extension, we took care of that.");
                saveFileDialog1.ShowDialog();
            }
        }

        private void saveFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            string name = saveFileDialog1.FileName;
;           File.WriteAllText(name+".csv", csv.ToString());
            MessageBox.Show("Done!");
            Application.Exit();
        }
    }
}