using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace GitLogTransformer
{
    public class GitLogParser
    {
        private static readonly Regex CommitRegex = new Regex(@"\w{40}\s.*(\+|\-)\d{4}\s(.*$)");

        public enum LineType
        {
            Committish,
            Blank,
            Other,
            Stats
        }

        public static LineType GetLineType(string line)
        {
            Match match = CommitRegex.Match(line);
            if (match.Success)
            {
                return LineType.Committish;
            }

            if (string.IsNullOrWhiteSpace(line))
            {
                return LineType.Blank;
            }

            if (line.Contains("(+)") || line.Contains("(-)"))
            {
                return LineType.Stats;
            }

            return LineType.Other;
        }

        public static string GetComment(string line)
        {
            Match match = CommitRegex.Match(line);
            return match.Value;
        }

        public static List<CommitDetails> CreateCommitDetails(StreamReader streamReader)
        {
            CommitDetails details = null;
            string line;
            List<CommitDetails> infos = new List<CommitDetails>();
            int lineCount = 1;
            while ((line = streamReader.ReadLine()) != null)
            {
                var linetype = GetLineType(line);
                switch (linetype)
                {
                    case LineType.Committish:
                        if (details != null)
                        {
                            //throw new Exception("Already have an info in progress: " + lineCount);
                            infos.Add(details);
                        }

                        details = new CommitDetails(line);
                        break;
                    case LineType.Blank:
                    case LineType.Other:
                        break;
                    case LineType.Stats:
                        if (details == null)
                        {
                            throw new Exception("Found a stats line but we have no info in progress");
                        }

                        details.CommitStats = new CommitStats(line);
                        infos.Add(details);
                        details = null;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                lineCount++;
            }

            return infos;
        }
    }
}