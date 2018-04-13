using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Newtonsoft.Json;

namespace PlayServices
{
    class Github
    {
        public class IssueLabel
        {
            public string id { get; set; }
            public string name { get; set; }
        };

        public class Issue
        {
            public string url { get; set; }
            public string html_url { get; set; }
            public string title { get; set; }
            public string body { get; set; }
            public List<IssueLabel> labels { get; set; }
        };

        const string g_issuesDirectory = "issues";
        const string g_repositoryIssuesUrl = "https://api.github.com/repos/jpd002/Play-Compatibility/issues";

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

        public static void DownloadIssues()
        {
            int issuePageCounter = 0;
            var pageUrl = g_repositoryIssuesUrl;
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

        public static List<Issue> ReadIssues()
        {
            var issues = new List<Issue>();
            foreach(var file in System.IO.Directory.EnumerateFiles(g_issuesDirectory, "issues_*.json"))
            {
                var issueFileContents = ReadFileContents(file);
                var fileIssues = JsonConvert.DeserializeObject<List<Issue>>(issueFileContents);
                issues.AddRange(fileIssues);
            }
            return issues;
        }
    }
}
