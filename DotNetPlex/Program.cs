using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using CsvHelper;
using DotNetPlex.Model;
using Newtonsoft.Json;

namespace DotNetPlex
{
    class Program
    {
        static void Main(string[] args)
        {
         //   GetLibrarySections();
            SaveVideoDetails();
            Console.ReadLine();
        }

        public static List<LibrarySection> GetLibrarySections()
        {
            var libraries = new List<LibrarySection>();
            using (var client = new WebClient())
            {
                client.BaseAddress = "http://192.168.0.100:32400/";
                var data = client.DownloadString("system/library/sections");

                XDocument doc = XDocument.Parse(data);
                var decendants = doc.Descendants("Directory");
                foreach (XElement decendant in decendants)
                {
                    libraries.Add(new LibrarySection()
                    {
                        Name = decendant.Attribute("name").Value,
                        Path = decendant.Attribute("path").Value,
                        Type = decendant.Attribute("type").Value

                    });
                }
            }
            return libraries;
        }

        public static void SaveVideoDetails()
        {

            var libraries = GetLibrarySections().Where(l=>l.Type=="movie"||l.Type=="show");


            var downloadLibraries = new List<Library>();
            using (var client = new WebClient())
            {
                client.BaseAddress = "http://192.168.0.100:32400/";
                //get library data
                foreach (var library in libraries)
                {
                        //required to readd header value each time
                        client.Headers.Add("Accept", "application/json");

                        var downloadString = client.DownloadString(string.Format("{0}/all", library.Path));
                        var libraryData = JsonConvert.DeserializeObject<Library>(downloadString);
                        downloadLibraries.Add(libraryData);
                }
            }

            WriteDetails(downloadLibraries);
 
        }

        public static void WriteDetails(List<Library> libraries)
        {
            const string filePath = @"C:\\temp\plex.csv";
            using (TextWriter tw = File.CreateText(filePath))
            {
                using (var writer = new CsvWriter(tw))
                {
                    writer.WriteField("Title");
                    writer.WriteField("Library Type");
                    writer.WriteField("Plex Folder");
                    writer.WriteField("Summary");
                    writer.NextRecord();


                    foreach (var library in libraries)
                    {
                        Console.WriteLine(string.Format("Writing Library : {0}", library.title1));
                       
                        foreach (var item in library.LibraryItems.OrderBy(o=>o.title))
                        {
                            //write season details out to csv
                            //if (item.type == "show")
                            //{
                            //    //check if its a directory. e.g. season 1,2,3
                            //    //if (item._elementType.ToLower().Equals("directory"))
                            //    //{
                            //    //    var path = item.key;

                            //    //    var download = DownloadWebString(path);

                            //    //    var libraryItems = JsonConvert.DeserializeObject<LibraryItem>(download);
                            //    //}
                            //    ///library/metadata/420/children
                            //    //var elements = item.ChildElements;
                            //    //foreach (var childElement in elements)
                            //    //{
                            //    //    var x = childElement.container;
                            //    //}
                            //}

                            writer.WriteField(item.title);
                            writer.WriteField(item.type);
                            writer.WriteField(library.title1);
                            writer.WriteField(item.summary);
                            writer.NextRecord();
                        }
                           
                    }
                }
                Console.WriteLine(string.Format("Saved library details to {0}",filePath));
            }
        }

        private static string DownloadWebString(string endPoint)
        {
            using (var client = new WebClient())
            {
                client.BaseAddress = "http://192.168.0.100:32400/";
                client.Headers.Add("Accept", "application/json");
                return client.DownloadString(endPoint);
            }
         
        }
    }
}
