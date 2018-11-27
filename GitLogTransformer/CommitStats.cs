using System;
using System.Text.RegularExpressions;
using static System.Int32;

namespace GitLogTransformer
{
    /// <summary>
    /// A data structure that represents a line in the log file like " 296 files changed, 63188 deletions(-)" and parses it to extract the numbers.
    /// </summary>
    public struct CommitStats
    {
        private static readonly Regex StatsRegex = new Regex(@"\s*(?<files>\d*)(?:\sfiles? change.*)\s(?<insertions>\d*)(?:\sinsertions?\(\+\).*)\s(?<deletions>\d*)(?:\sdeletions?\(\-\).*)");

        public int FilesChanged { get; }

        public int Insertions { get; }

        public int Deletions { get; }

        /// <summary>
        /// Returns the original line form the log. Not used but useful when debugging.
        /// </summary>
        public string Line { get; }

        public CommitStats(string line) : this()
        {
            Line = line;
            Match match = StatsRegex.Match(line);
            Deletions = ParseInt(match.Groups["deletions"]);
            Insertions = ParseInt(match.Groups["insertions"]);
            FilesChanged = ParseInt(match.Groups["files"]);
        }

        private int ParseInt(Group matchGroup)
        {
            if (matchGroup.Success)
            {
                if (TryParse(matchGroup.Value.Trim(), out int i))
                {
                    return i;
                }

                return -1;
            }

            return 0;
        }
    }
}