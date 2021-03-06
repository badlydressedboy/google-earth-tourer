using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace GETourer
{
    class Program
    {
        static void Main(string[] args)
        {
            // read myplaces
            string tourName = "AutoGeneratedCanyonTour2";
            string durationFlying = "3";
            string inputKmlfile = @"C:\Users\user\AppData\LocalLow\Google\GoogleEarth\myplaces.kml";
            string outputKmlFile = @"C:\Temp\GE\AutoGeneratedCanyonTour2.kml";
            string folderName = "CanyonTourPlacemarks";

            int counter = 315000;
            StringBuilder sb = new StringBuilder();
            sb.Append("<Folder>");

            IEnumerable<string> lines = File.ReadLines(inputKmlfile);            
            var linex = lines.Skip(counter);

            bool inSection = false;
            
            // get test between unique opening name and </Folder>? - must be no subfolders obv            
            foreach (string line in linex)
            {
                Console.WriteLine(counter);
                if (line.Contains(folderName))
                {
                    inSection = true;                    
                }
                if (inSection && line.Contains("</Folder>"))
                {
                    sb.Append(line);
                    inSection = false;    
                }

                if (inSection)
                {
                    sb.Append(line);
                    Console.WriteLine(line);
                }
                counter++;
            }



            XmlDocument doc2 = new XmlDocument();
            doc2.LoadXml(sb.ToString().Replace("gx:", ""));
            XmlNode root = doc2.DocumentElement;
            List<XmlNode> placemarks = new List<XmlNode>();

            foreach (XmlNode item in root.ChildNodes)
            {
                if (item.Name == "Placemark")
                {
                    Console.WriteLine("Reading placemark " + item.ChildNodes[0].InnerText);
                    placemarks.Add(item);
                }                
            }


            // start building the xml for the tour
            string xml = string.Format(@"<?xml version=""1.0"" encoding=""UTF-8""?>
<kml xmlns=""http://www.opengis.net/kml/2.2"" xmlns:gx=""http://www.google.com/kml/ext/2.2"" xmlns:kml=""http://www.opengis.net/kml/2.2"" xmlns:atom=""http://www.w3.org/2005/Atom"">
<gx:Tour>
	<name>{0}</name>
	<gx:Playlist>", tourName);

            foreach (var placemark in placemarks)
            {
                try
                {
                    Console.WriteLine("Writing placemark " + placemark.ChildNodes[0].InnerText);

                    XmlNode lookatNode; 
                    // have we used fancy flying?
                    if (placemark.SelectSingleNode("LookAt") == null)
                    {
                        lookatNode = placemark.SelectSingleNode("Camera");
                    }
                    else
                    {
                        lookatNode = placemark.SelectSingleNode("LookAt");
                    }
                                                          
                    string longitude = lookatNode.SelectSingleNode("longitude").InnerText;
                    string latitude = lookatNode.SelectSingleNode("latitude").InnerText;
                    string altitude = lookatNode.SelectSingleNode("altitude").InnerText;
                    string heading = lookatNode.SelectSingleNode("heading").InnerText;
                    string tilt = lookatNode.SelectSingleNode("tilt").InnerText;                    
                    string altitudeMode = lookatNode.SelectSingleNode("altitudeMode").InnerText;

                    // roll and range are mutually exclusive
                    string rollRange = "";
                    if (lookatNode.SelectSingleNode("roll") != null)
                    {
                        rollRange = "<roll>" + lookatNode.SelectSingleNode("roll").InnerText + "</roll>";
                    }
                    else
                    {
                        rollRange = "<range>" + lookatNode.SelectSingleNode("range").InnerText + "</range>";
                    }

                    xml += string.Format(@"<gx:FlyTo>
                    <gx:flyToMode>smooth</gx:flyToMode>
			        <gx:duration>{0}</gx:duration>
			        <LookAt>
				        <longitude>{1}</longitude>
				        <latitude>{2}</latitude>
				        <altitude>{3}</altitude>
				        <heading>{4}</heading>
				        <tilt>{5}</tilt>
				        {6}
				        <gx:altitudeMode>{7}</gx:altitudeMode>
			        </LookAt>
		        </gx:FlyTo>"
                                         , durationFlying
                                         , longitude
                                         , latitude
                                         , altitude
                                         , heading
                                         , tilt
                                         , rollRange
                                         , altitudeMode);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }


            xml += @"</gx:Playlist>
            </gx:Tour>
            </kml>";

            // write out
            File.WriteAllText(outputKmlFile, xml);

            Console.WriteLine("DONE");

            Process.Start(outputKmlFile);
            //Console.Read();
        }
    }
}
