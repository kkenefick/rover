using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Rover
{
    class Program
    {
        public static string  RoverUrl = "https://api.nasa.gov/mars-photos/api/v1/rovers/curiosity/photos";

        static void Main(string[] args)
        {

            if (args[0] != null)
                throw new Exception("Invalid input file arg");

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            // loop through dates saving images
            using (var reader = new StreamReader(args[0]))
            {
                var line = reader.ReadLine();
                while (line != null)
                {
                    DateTime date = Convert.ToDateTime(line);

                    String imagepath = findImage(date.ToString("yyyy-MM-dd"));

                    if (imagepath!=null)
                        saveImage(imagepath, date.ToString("yyyyMMdd"));

                    line = reader.ReadLine();
                }
            }
            
        }
        static string findImage (string date)
        {
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(RoverUrl + "?earth_date=" + date + "&api_key=DEMO_KEY");
            request.Method = "Get";

            using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
            {
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    throw new Exception();
                }

                using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                {
                    String resp = reader.ReadToEnd();
                    var jObject = JObject.Parse(resp);
                    if (jObject["photos"].Count()>0)
                        return (string)jObject["photos"][0]["img_src"];
                    else
                        return null;
                }
            }
        }
        static bool saveImage (string url, string date)
        {
            string filename="C:\\" + date + "_" + url.Substring(url.LastIndexOf('/')+1);
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
            request.Method = "Get";
            System.IO.FileStream output = new System.IO.FileStream(filename, FileMode.Create);

            using (HttpWebResponse response = request.GetResponse() as HttpWebResponse)
            {
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    throw new Exception();
                }

                using (BinaryReader reader = new BinaryReader(response.GetResponseStream()))
                {
                    byte[] buffer = new byte[8196];
                    int readBytes = 0;
                    while ((readBytes = reader.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        output.Write(buffer, 0, readBytes);
                    }
                }
            }

            return true;
        }

    }
}
