using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace PlayCompatValidator
{
    class IssueLabel
    {
        public string id { get; set; }
        public string name { get; set; }
    };

    class RawIssue
    {
        public string url { get; set; }
        public string html_url { get; set; }
        public string title { get; set; }
        public string body { get; set; }
        public List<IssueLabel> labels { get; set; }
    };

    class GameCompatibility
    {
        public string GameId { get; set; }

        public string State { get; set; }
    }

    class Program
    {
        const string g_issuesDirectory = "issues";

        static string ExtractUrlFromLink(string link)
        {
            var urlStart = link.IndexOf('<') + 1;
            var urlEnd = link.IndexOf('>');
            if(urlStart > urlEnd) throw new System.Exception("Invalid link.");
            return link.Substring(urlStart, urlEnd - urlStart);
        }
        
        static string ExtractNextLinkUrl(WebHeaderCollection responseHeaders)
        {
            var linkHeader = responseHeaders.Get("Link");
            if(linkHeader == null) return String.Empty;
            var linkList = linkHeader.Split(", ");
            foreach(var link in linkList)
            {
                if(!link.Contains("rel=\"next\"")) continue;
                var linkUrl = ExtractUrlFromLink(link);
                return linkUrl;
            }
            return String.Empty;
        }

        static string DownloadIssuePage(string url, string outputPath)
        {
            using (var client = new WebClient())
            {
                client.Headers.Add("User-Agent: Other");
                client.DownloadFile(url, outputPath);
                var nextPageUrl = ExtractNextLinkUrl(client.ResponseHeaders);
                return nextPageUrl;
            }
        }

        static void DownloadIssues()
        {
            int issuePageCounter = 0;
            var pageUrl = "https://api.github.com/repos/jpd002/Play-Compatibility/issues";
            if(System.IO.Directory.Exists(g_issuesDirectory))
            {
                System.IO.Directory.Delete(g_issuesDirectory, true);
            }
            System.IO.Directory.CreateDirectory(g_issuesDirectory);
            while(true)
            {
                var outputPath = String.Format("{0}/issues_{1}.json", g_issuesDirectory, issuePageCounter);
                pageUrl = DownloadIssuePage(pageUrl, outputPath);
                issuePageCounter++;
                if(String.IsNullOrEmpty(pageUrl)) break;
            }
        }

        static string ReadFileContents(string path)
        {
            using (StreamReader sr = new StreamReader(path))
            {
                return sr.ReadToEnd();
            }  
        }

        static List<RawIssue> ReadIssues()
        {
            var issues = new List<RawIssue>();
            foreach(var file in System.IO.Directory.EnumerateFiles(g_issuesDirectory, "issues_*.json"))
            {
                var issueFileContents = ReadFileContents(file);
                var rawIssues = JsonConvert.DeserializeObject<List<RawIssue>>(issueFileContents);
                issues.AddRange(rawIssues);
            }
            return issues;
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
                    var validationMessage = String.Format("Header doesn't match reference. Got '{0}', expected '{1}'.", 
                        headers[i], headersReference[i]);
                    throw new System.Exception(validationMessage);
                }
            }
        }

        static IReadOnlyCollection<GameCompatibility> GetGameCompatibilities()
        {
            DownloadIssues();

            var rawIssues = ReadIssues();

            using (var sw = new StreamWriter("report.txt"))
            {
                var gameCompatibilities = new Dictionary<string, GameCompatibility>();
                foreach (var rawIssue in rawIssues)
                {
                    try
                    {
                        //Extract info and validate title
                        var reg = new Regex(@"\[([^]]*)\] (.*)");
                        var matches = reg.Matches(rawIssue.title);
                        if (matches.Count != 1) throw new System.Exception();
                        if (matches[0].Groups.Count != 3) throw new System.Exception("Bad issue title format.");
                        var gameId = matches[0].Groups[1].Value;
                        //Validate game Id XXXX-XXXXXX
                        var gameName = matches[0].Groups[2].Value;

                        if (gameCompatibilities.ContainsKey(gameId)) throw new System.Exception("Disc ID already exists.");

                        ValidateBody(rawIssue.body);

                        if (rawIssue.labels.Count == 0) throw new System.Exception("Issue doesn't have a label.");

                        gameCompatibilities.Add(gameId, new GameCompatibility { GameId = gameId, State = rawIssue.labels[0].name });
                    }
                    catch (System.Exception exception)
                    {
                        sw.WriteLine(
                            string.Format("Validation error for '{0}', {1} : {2}",
                            rawIssue.title, rawIssue.html_url, exception.Message)
                        );
                    }
                }

                return gameCompatibilities.Values;
            }
        }
        
        static void Main(string[] args)
        {
            var values = GetGameCompatibilities();
        }
    }
}
