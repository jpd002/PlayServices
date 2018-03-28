using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace PlayCompatValidator
{
    class RawIssue
    {
        public string url { get; set; }
        public string html_url { get; set; }
        public string title { get; set; }
        public string body { get; set; }
    };

    class Program
    {
        static void DownloadIssues()
        {
            using (var client = new WebClient())
            {
                client.Headers.Add("User-Agent: Other");
                client.DownloadFile("https://api.github.com/repos/jpd002/Play-Compatibility/issues", "issues.json");
            }
        }

        static string ReadIssues()
        {
            using (StreamReader sr = new StreamReader("issues.json"))
            {
                return sr.ReadToEnd();
            }
        }

        static string[] headersReference =
        {
            "**Last Tested On**",
            "**Known Issues & Notes**",
            "**Related**",
            "**Screenshots**"
        };

        static void ValidateBody(string body)
        {
            var lines = body.Split("\r\n");
            var headers = new List<String>();
            foreach(var line in lines)
            {
                if(line.StartsWith("**"))
                {
                    headers.Add(line);
                }
            }
            if (headers.Count != headersReference.Length) throw new System.Exception("Body format incorrect.");
            for(int i = 0; i < headers.Count; i++)
            {
                if(headers[i] != headersReference[i])
                {
                    throw new System.Exception();
                }
            }
        }

        static void Main(string[] args)
        {
            DownloadIssues();

            var rawIssuesJson = ReadIssues();
            var rawIssues = JsonConvert.DeserializeObject<List<RawIssue>>(rawIssuesJson);

            using (var sw = new StreamWriter("report.txt"))
            {
                var gameIdSet = new SortedSet<string>();
                foreach (var rawIssue in rawIssues)
                {
                    try
                    {
                        var reg = new Regex(@"\[([^]]*)\] (.*)");
                        var matches = reg.Matches(rawIssue.title);
                        if (matches.Count != 1) throw new System.Exception();
                        if (matches[0].Groups.Count != 3) throw new System.Exception("Bad issue title format.");
                        var gameId = matches[0].Groups[1].Value;
                        //Validate game Id XXXX-XXXXXX
                        var gameName = matches[0].Groups[2].Value;
                        if (gameIdSet.Contains(gameId)) throw new System.Exception("Game ID already exists.");
                        gameIdSet.Add(gameId);
                        ValidateBody(rawIssue.body);
                    }
                    catch (System.Exception exception)
                    {
                        sw.WriteLine(
                            string.Format("Validation error for '{0}', {1} : {2}",
                            rawIssue.title, rawIssue.html_url, exception.Message)
                        );
                    }
                }
            }
        }
    }
}
