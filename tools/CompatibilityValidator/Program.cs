using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Octokit;

namespace PlayCompatValidator
{
    class GameCompatibility
    {
        public string GameId { get; set; }
        public string State { get; set; }
    }

    public class CompatibilitySummaryItem
    {
        [JsonProperty("state")]
        public string State { get; set; } = String.Empty;

        [JsonProperty("count")]
        public int Count { get; set; }
    };

    public class CompatibilitySummary
    {
        [JsonProperty("items")]
        public List<CompatibilitySummaryItem> Items { get; set; } = new List<CompatibilitySummaryItem>();
    };

    class Program
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

        static bool ValidateGameCompatibilities(IEnumerable<Issue> rawIssues)
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
                    catch (System.Exception exception)
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

        static List<GameCompatibility> GetGameCompatibilities(IEnumerable<Issue> rawIssues)
        {
            var result = new List<GameCompatibility>();
            foreach (var rawIssue in rawIssues)
            {
                var gameInfo = ExtractGameInfo(rawIssue.Title);
                var compat = new GameCompatibility();
                compat.GameId = gameInfo.Item1;
                compat.State = GetState(rawIssue.Labels);
                result.Add(compat);
            }
            return result;
        }

        static void GenerateCompatibilitySummary(IEnumerable<GameCompatibility> gameCompats)
        {
            var stateCount = new Dictionary<string, int>();
            foreach(var gameCompat in gameCompats)
            {
                int currentCount = 0;
                stateCount.TryGetValue(gameCompat.State, out currentCount);
                stateCount[gameCompat.State] = currentCount + 1;
            }

            var summary = new CompatibilitySummary();
            foreach(var entry in stateCount)
            {
                var summaryItem = new CompatibilitySummaryItem();
                summaryItem.State = entry.Key;
                summaryItem.Count = entry.Value;
                summary.Items.Add(summaryItem);
            }
            var result = JsonConvert.SerializeObject(summary.Items);
            File.WriteAllText("compat_summary.json", result);
        }

        static void Main(string[] args)
        {
            var client = new GitHubClient(new ProductHeaderValue("PlayServices"));
            var ghToken = Environment.GetEnvironmentVariable("ps_gh_apitoken");
            if(!String.IsNullOrEmpty(ghToken))
            {
                client.Credentials = new Credentials(ghToken);
            }

            var issuesTask = client.Issue.GetAllForRepository("jpd002", "Play-Compatibility");
            issuesTask.Wait();
            var issues = issuesTask.Result;

            if(!ValidateGameCompatibilities(issues))
            {
                System.Console.WriteLine("Compatibility report validation failed. See 'report.txt' for details.");
                return;
            }
            var gameCompats = GetGameCompatibilities(issues);
            GenerateCompatibilitySummary(gameCompats);
        }
    }
}
