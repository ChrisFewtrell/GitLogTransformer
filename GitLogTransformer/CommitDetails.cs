using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace GitLogTransformer
{
    public sealed class CommitDetails
    {
        /// <summary>
        /// Regex to identify a line of the form
        /// <code>
        /// |           Committish                  |            Date              |   Merge comment ........
        /// da9a9075992d880705004aa40c819546fea4d9f2 Wed Nov 29 11:34:41 2017 +0000 Merge pull request #8787 from tom-dudley/MI-13144-fix-explore-upgrade-v2
        /// </code>
        /// </summary>
        private static readonly Regex CommitRegex =
            new Regex(@"(?:\s*)(?<commitish>\w{40})(?<date>.{25})(?:.{6})(?<comment>.*$)");

        /// <summary>
        /// A regex to parse the string "Wed Nov 29 11:34:41 2017  ..." into the various year, month and day parts.
        /// </summary>
        private static readonly Regex DateRegex = new Regex(@"(?<day>\w{3})\s(?<month>\w{3})\s(?<dayNum>\d\d?)(?:).*(?<year>\d{4})$");

        private static readonly Dictionary<string, int> ShortMonthNameToNumber = new Dictionary<string, int>
        {
            {"Jan", 1},
            {"Feb", 2},
            {"Mar", 3},
            {"Apr", 4},
            {"May", 5},
            {"Jun", 6},
            {"Jul", 7},
            {"Aug", 8},
            {"Sep", 9},
            {"Oct", 10},
            {"Nov", 11},
            {"Dec", 12}
        };

        public string Committish { get; }

        public string Comment { get; }

        /// <summary>
        /// Returns the date portion of <see cref="Line"/> as a string.
        /// Not very useful - <see cref="Date"/> is more useful.
        /// </summary>
        public string DateString { get; }

        /// <summary>
        /// Returns the whole line from the git log. Not used but useful in debugging.
        /// </summary>
        public string Line { get; }

        public CommitStats CommitStats { get; set; }

        /// <summary>
        /// The date of the commit.
        /// </summary>
        public DateTime Date { get; }

        public CommitDetails(string line)
        {
            Line = line;
            Match match = CommitRegex.Match(line);
            Committish = match.Groups["commitish"]?.Value?.Trim();
            DateString = match.Groups["date"]?.Value?.Trim();
            Comment = match.Groups["comment"]?.Value?.Trim();

            try
            {
                Match dateMatch = DateRegex.Match(DateString);
                int day = ParseInt(dateMatch.Groups["dayNum"]);
                int year = ParseInt(dateMatch.Groups["year"]);
                string month = dateMatch.Groups["month"].Value;
                int monthNum = ShortMonthNameToNumber[month];

                Date = new DateTime(year, monthNum, day);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        private int ParseInt(Group matchGroup)
        {
            if (matchGroup.Success)
            {
                if (int.TryParse(matchGroup.Value.Trim(), out int i))
                {
                    return i;
                }
            }

            return -1;
        }

        /// <summary>
        /// Returns the column headings (each separated by <paramref name="sep"/> in the same order that <see cref="GetTsvLine"/> outputs.
        /// </summary>
        /// <param name="sep"></param>
        /// <returns></returns>
        public static string GetTsvHeader(string sep)
        {
            return WriteValues(sep, new object[]
            {
                "Committish",
                "FilesChanged",
                "Insertions",
                "Deletions",
                "Sum changes",
                "Date",
                "Month",
                "Comment",
            });
        }

        /// <summary>
        /// Outputs the properties in a line - separated by <paramref name="sep"/>.
        /// </summary>
        /// <param name="sep"></param>
        /// <returns></returns>
        public string GetTsvLine(string sep)
        {
            return WriteValues(sep, new object[]
            {
                Committish,
                CommitStats.FilesChanged,
                CommitStats.Insertions,
                CommitStats.Deletions,
                CommitStats.Deletions + CommitStats.Insertions,
                Date.ToShortDateString(),
                Date.ToString("yyyy-MM"),
                ("\"" + Comment + "\"")
            });
        }

        private static string WriteValues(string sep, IEnumerable<object> o)
        {
            var tw = new StringWriter();
            foreach (object obj in o)
            {
                tw.Write(obj.ToString());
                tw.Write(sep);
            }

            return tw.ToString();
        }
    }
}