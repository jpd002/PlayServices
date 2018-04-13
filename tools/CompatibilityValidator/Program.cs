using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using PlayServices;

namespace PlayCompatValidator
{
    class GameCompatibility
    {
        public string GameId { get; set; }
        public string State { get; set; }
    }

    class Program
    {
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

        static bool ValidateGameCompatibilities()
        {
            bool isValid = true;
            var rawIssues = Github.ReadIssues();

            using (var sw = new StreamWriter("report.txt"))
            {
                var gameIds = new HashSet<string>();
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

                        if (gameIds.Contains(gameId)) throw new System.Exception("Disc ID already exists.");
                        gameIds.Add(gameId);

                        ValidateBody(rawIssue.body);

                        int stateLabelCount = 0;
                        foreach(var label in rawIssue.labels)
                        {
                            if(label.name.StartsWith("state-")) stateLabelCount++;
                        }

                        if(stateLabelCount != 1) throw new System.Exception("Issue doesn't have a state label.");
                    }
                    catch (System.Exception exception)
                    {
                        isValid = false;
                        sw.WriteLine(
                            string.Format("Validation error for '{0}', {1} : {2}",
                            rawIssue.title, rawIssue.html_url, exception.Message)
                        );
                    }
                }
            }
            return isValid;
        }
        
        static void Main(string[] args)
        {
            Github.DownloadIssues();
            if(!ValidateGameCompatibilities())
            {
                System.Console.WriteLine("Compatibility report validation failed. See 'report.txt' for details.");
                return;
            }
            //var values = GetGameCompatibilities();
        }
    }
}
