using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Octokit;

namespace PlayServices
{
    class CompatibilityDataExtractor
    {
        static Tuple<string, string> ExtractGameInfo(string issueTitle)
        {
            var reg = new Regex(@"\[([^]]*)\] (.*)");
            var matches = reg.Matches(issueTitle);
            if (matches.Count != 1) throw new System.Exception();
            if (matches[0].Groups.Count != 3) throw new System.Exception("Bad issue title format.");
            var gameId = matches[0].Groups[1].Value;
            //Validate game Id XXXX-XXXXXX
            var gameName = matches[0].Groups[2].Value;
            return Tuple.Create(gameId, gameName);
        }
        
        static string GetState(IEnumerable<Label> labels)
        {
            var stateLabels = new List<Label>();
            foreach(var label in labels)
            {
                if(label.Name.StartsWith("state-"))
                {
                    stateLabels.Add(label);
                }
            }

            if(stateLabels.Count != 1) throw new System.Exception("Issue doesn't have a state label or has multiple state labels.");

            return stateLabels[0].Name;
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

        static bool ValidateGameCompatibilitiesInternal(IEnumerable<Issue> rawIssues)
        {
            bool isValid = true;

            using (var sw = new StreamWriter("report.txt"))
            {
                var gameIds = new HashSet<string>();
                foreach (var rawIssue in rawIssues)
                {
                    try
                    {
                        var gameInfo = ExtractGameInfo(rawIssue.Title);

                        if (gameIds.Contains(gameInfo.Item1)) throw new System.Exception("Disc ID already exists.");
                        gameIds.Add(gameInfo.Item1);

                        ValidateBody(rawIssue.Body);
                        GetState(rawIssue.Labels);
                    }
                    catch(System.Exception exception)
                    {
                        isValid = false;
                        sw.WriteLine(
                            string.Format("Validation error for '{0}', {1} : {2}",
                            rawIssue.Title, rawIssue.HtmlUrl, exception.Message)
                        );
                    }
                }
            }
            return isValid;
        }

        static List<GameCompatibility> GetGameCompatibilitiesInternal(IEnumerable<Issue> rawIssues)
        {
            var result = new List<GameCompatibility>();
            foreach (var rawIssue in rawIssues)
            {
                try
                {
                    var gameInfo = ExtractGameInfo(rawIssue.Title);
                    var compat = new GameCompatibility();
                    compat.GameId = gameInfo.Item1;
                    compat.State = GetState(rawIssue.Labels);
                    result.Add(compat);
                }
                catch(System.Exception exception)
                {
                    System.Console.WriteLine("Warning: Failed to obtain info about {0}.", rawIssue.Title);
                }
            }
            return result;
        }

        public static IReadOnlyList<Octokit.Issue> GetIssuesFromRepository()
        {
            var client = new GitHubClient(new ProductHeaderValue("PlayServices"));
            var ghToken = Environment.GetEnvironmentVariable("ps_gh_apitoken");
            if(!String.IsNullOrEmpty(ghToken))
            {
                client.Credentials = new Credentials(ghToken);
            }

            var issuesTask = client.Issue.GetAllForRepository("jpd002", "Play-Compatibility");
            issuesTask.Wait();
            return issuesTask.Result;
        }

        public static bool ValidateGameCompatibilities()
        {
            var issues = GetIssuesFromRepository();
            return ValidateGameCompatibilitiesInternal(issues);
        }

        public static List<GameCompatibility> GetGameCompatibilities()
        {
            var issues = GetIssuesFromRepository();
            return GetGameCompatibilitiesInternal(issues);
        }
    }
}
