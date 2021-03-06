﻿using CommandLine;
using FileParser;
using System;
using System.IO;
using System.IO.Compression;


namespace RangeShifter
{
    class Program
    {
        public static AppDomain currentDomain = AppDomain.CurrentDomain;

        public static readonly int shiftingNum = 70443950;
        public static readonly string prefix = "BGo";

        static string unZipLoc = @"./tmp/zip/";
        static string zipLoc = @"./tmp/unzip/";

        public static FileParser.TextElementCollection tCollection;
        public static Action<string> collectHandler;
        public static Action<string> replaceHandler;

        // ---------------  options for collect ---------------  
        [Verb("collect", HelpText = "Adds textelement and outputs csv")]
        public class CollectOptions
        {
            [Option('t', "target-dir", Required = true, HelpText = "Target directory/file for reading; \nfileextension can also be .zip ")]
            public string targetDirectory { get; set; }

            [Option('c', "config-location", Default = "./config.csv", HelpText = "Location of new intermediate configfile")]
            public string configLoc { get; set; }
        }
        // --------------- options for write --------------- 
        [Verb("write", HelpText = "writes")]
        public class WriteOptions
        {
            [Option('c', "config-location", Default = "./config.csv", HelpText = "Location of intermediate configfile")]
            public string configLoc { get; set; }

            [Option('s', "source-dir", HelpText = "Source directory for copying; can be a zip/rar", Required = true)]
            public string sourceDir { get; set; }

            [Option('o', "output-dir", HelpText = "Output directory for writing", Required = true)]
            public string outputDir { get; set; }
        }

        static void Main(string[] args)
        {

            tCollection = new TextElementCollection();
            collectHandler = tCollection.collectAll;
            replaceHandler = tCollection.replaceAll;

            var result = Parser.Default.ParseArguments<CollectOptions, WriteOptions>(args) // has to be instance of class apperantly
                  .WithParsed<CollectOptions>(options => collectBranch(options))
                  .WithParsed<WriteOptions>(options => writeBranch(options))
                  .WithNotParsed(_ => Console.WriteLine("ERROR Failed to parse"));
        }




        static void collectBranch(CollectOptions options)
        {
            Console.WriteLine("[collect]");

            FileInfo fi = new FileInfo(options.targetDirectory);


            if (Directory.Exists(options.targetDirectory))
            {
                doForEachFile(options.targetDirectory, collectHandler);
                tCollection.exportCSV(options.configLoc);
            }
            else if (new FileInfo(options.targetDirectory).Extension == ".rar" || new FileInfo(options.targetDirectory).Extension == ".zip")
            {
                Console.WriteLine("ddddd");
                unzip(options.targetDirectory, unZipLoc);
                doForEachFile(unZipLoc, collectHandler);
                tCollection.exportCSV(options.configLoc);
            }

            wipe();

        }
        static void writeBranch(WriteOptions options)
        {

            Console.WriteLine("[write]");
            if (!File.Exists(options.configLoc)) // config is required to write
            {
                Console.Error.WriteLine("ERROR Config file dosent exist.");
                return;
            }


            tCollection.importCSV(options.configLoc);


            string source = options.sourceDir;
            string output = options.outputDir;



            if (Directory.Exists(source) || File.Exists(source))
            {
                // sets source loc
                if (options.sourceDir.EndsWith(".zip") || options.sourceDir.EndsWith(".rar"))
                {
                    unzip(source, unZipLoc);
                    source = unZipLoc;
                }
            }
            else
            {
                Console.Error.WriteLine("no zip or dir at " + source);
                return;
            }

            // sets write loc
            if (options.outputDir.EndsWith(".zip") || options.outputDir.EndsWith(".rar"))
            {
                doForEachFile(source, replaceHandler);
                zip(source, options.outputDir);
            }
            else
            {
                DirectoryCopy(source, output, true);
                doForEachFile(options.outputDir, replaceHandler);
            }

            wipe();
        }

        static void wipe()
        {
            if (Directory.Exists("./tmp/"))
            {
                Directory.Delete("./tmp/", true);

            }

        }

        static void doForEachFile(string sDir, Action<String> func)
        {
            if (!File.GetAttributes(sDir).HasFlag(FileAttributes.Directory)) // checks if its a file
            {
                func(sDir);
            }
            else
            {
                foreach (string f in Directory.GetFiles(sDir))
                {
                    func(f);
                }
                foreach (string d in Directory.GetDirectories(sDir))
                {
                    doForEachFile(d, func);
                }
            }
        }

        static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!File.GetAttributes(sourceDirName).HasFlag(FileAttributes.Directory))
            {
                FileInfo file = new FileInfo(sourceDirName);
                file.CopyTo(destDirName);
                return;
            }

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string temppath = Path.Combine(destDirName, file.Name);
                file.CopyTo(temppath, true);

            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                }
            }
        }

        static void zip(string inPath, string outPath)
        {
            ZipFile.CreateFromDirectory(inPath, outPath);
        }
        static void unzip(string inPath, string outPath)
        {
            using (ZipArchive archive = ZipFile.OpenRead(inPath))
            {
                archive.ExtractToDirectory(outPath);
            }
        }
    }
}