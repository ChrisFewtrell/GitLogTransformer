﻿using System;
using System.Collections.Generic;
using System.IO;

namespace GitLogTransformer
{
    /// <summary>
    /// A little program to read a file generated by git-log and consolidate into a TSV that can be more easily analyzed in Excel.
    /// This is something that I am sure is doable in one long line of powershell or bash.
    /// </summary>
    /// <remarks>
    /// <para>
    /// To generate the original log file use
    /// <code> git log --compact-summary --format="%H %ad %s"</code>
    /// </para>
    /// This program simply
    /// </remarks>
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                throw new ArgumentException("You must provide the path to the git log file.");
            }

            string inputFilePath = args[0];

            Console.WriteLine("Processing file: " + inputFilePath);
            using (StreamReader streamReader = OpenInput(inputFilePath))
            {
                List<CommitDetails> infos = GitLogParser.CreateCommitDetails(streamReader);
                Console.WriteLine("Finished processing. Writing output.");
                WriteOutput(inputFilePath, infos);
            }

            Console.WriteLine("Finished.");
        }

        private static StreamReader OpenInput(string inputFilePath)
        {
            try
            {
                return File.OpenText(inputFilePath);
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine("Cannot find file: " + inputFilePath);
                Console.WriteLine(e);
                return null;
            }
        }

        private static void WriteOutput(string inputFilePath, List<CommitDetails> infos)
        {
            const string Separater = "\t";
            var outputFile = new FileInfo(inputFilePath + ".tsv");
            using (var st = outputFile.OpenWrite())
            {
                using (var tw = new StreamWriter(st))
                {
                    // Output a line number - useful to restore original log order in excel after a sort.
                    int lineNum = 1;
                    tw.Write("Line#");
                    tw.Write(Separater);

                    tw.WriteLine(CommitDetails.GetTsvHeader(Separater));
                    foreach (CommitDetails i in infos)
                    {
                        tw.Write(lineNum++);
                        tw.Write(Separater);
                        tw.WriteLine(i.GetTsvLine(Separater));
                    }
                }
            }
        }
    }
}