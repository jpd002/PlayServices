using System;

namespace PlayServices
{
    public class Build
    {
        public string CommitHash { get; set; }

        public string CommitMessage { get; set; }

        public DateTime CommitDate { get; set; }

        public bool HasBuild { get; set; }

        public DateTime Timestamp { get; set; }
    };
}
