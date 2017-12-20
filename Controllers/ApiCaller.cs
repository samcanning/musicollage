using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net;
using System.IO;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Musicollage
{
    public class ApiCaller
    {
        public static async Task SearchArtistName(string name, Action<object> Callback)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"http://musicbrainz.org/ws/2/artist/?query=artist:{name}&fmt=json");
                request.Headers["User-Agent"] = "Test Application - samcanning@outlook.com";
                HttpWebResponse response = (HttpWebResponse) await request.GetResponseAsync();
                StreamReader reader = new StreamReader(response.GetResponseStream());
                object result = JsonConvert.DeserializeObject(reader.ReadToEnd());
                reader.Dispose();
                response.Dispose();
                Callback(result);
            }
            catch(Exception e)
            {
                System.Console.WriteLine(e.Message);
            }
        }
        public static async Task SearchReleaseName(string title, Action<object> Callback)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"http://musicbrainz.org/ws/2/release-group/?query=release:{title}&fmt=json");
                request.Headers["User-Agent"] = "Test Application - samcanning@outlook.com";
                HttpWebResponse response = (HttpWebResponse) await request.GetResponseAsync();
                StreamReader reader = new StreamReader(response.GetResponseStream());
                object result = JsonConvert.DeserializeObject(reader.ReadToEnd());
                reader.Dispose();
                response.Dispose();
                Callback(result);
            }
            catch(Exception e)
            {
                System.Console.WriteLine(e.Message);
            }
        }
        public static async Task GetArtistData(string id, Action<object> Callback)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"http://musicbrainz.org/ws/2/artist/{id}?inc=release-groups&fmt=json");
                request.Headers["User-Agent"] = "Test Application - samcanning@outlook.com";
                HttpWebResponse response = (HttpWebResponse) await request.GetResponseAsync();
                StreamReader reader = new StreamReader(response.GetResponseStream());
                object result = JsonConvert.DeserializeObject(reader.ReadToEnd());
                reader.Dispose();
                response.Dispose();
                Callback(result);
            }
            catch(Exception e)
            {
                System.Console.WriteLine(e.Message);
            }
        }
        
        public static async Task GetArtistReleases(string id, Action<object> Callback)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"http://musicbrainz.org/ws/2/release/?query=arid:{id}&fmt=json");
                request.Headers["User-Agent"] = "Test Application - samcanning@outlook.com";
                HttpWebResponse response = (HttpWebResponse) await request.GetResponseAsync();
                StreamReader reader = new StreamReader(response.GetResponseStream());
                object result = JsonConvert.DeserializeObject(reader.ReadToEnd());
                reader.Dispose();
                response.Dispose();
                Callback(result);
            }
            catch(Exception e)
            {
                System.Console.WriteLine(e.Message);
            }
        }

        public static async Task GetReleaseData(string id, Action<object> Callback)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"http://musicbrainz.org/ws/2/release-group/{id}?inc=artists&fmt=json");
                request.Headers["User-Agent"] = "Test Application - samcanning@outlook.com";
                HttpWebResponse response = (HttpWebResponse) await request.GetResponseAsync();
                StreamReader reader = new StreamReader(response.GetResponseStream());
                object result = JsonConvert.DeserializeObject(reader.ReadToEnd());
                reader.Dispose();
                response.Dispose();
                Callback(result);
            }
            catch(Exception e)
            {
                System.Console.WriteLine(e.Message);
            }
        }

        public static async Task GetArt(string id, Action<object> Callback)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"https://coverartarchive.org/release-group/{id}");
                request.Headers["User-Agent"] = "Test Application - samcanning@outlook.com";
                HttpWebResponse response = (HttpWebResponse) await request.GetResponseAsync();
                StreamReader reader = new StreamReader(response.GetResponseStream());
                object result = JsonConvert.DeserializeObject(reader.ReadToEnd());
                reader.Dispose();
                response.Dispose();
                Callback(result);
            }
            catch(Exception e)
            {
                System.Console.WriteLine(e.Message);
            }
        }
    }
}