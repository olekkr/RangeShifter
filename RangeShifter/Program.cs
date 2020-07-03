using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Text;

namespace RangeShifter
{
    //Range for table object ID numbers 	1 – 999,999,999
    //Maximum number of characters in variable names 	30

    class Program
    {
        public const int shiftingNum = 70443950;
        public const string prefix = "BGo";

        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.WriteLine(args.Length);
                Console.Error.Write("[ERROR] wrong number of args");

                return;
            }


            TextElementCollection t = new TextElementCollection();

            Action<string> collectHandler = t.collectAll;
            Action<string> replaceHandler = t.replaceAll;
            Action<string> test = Console.WriteLine;
            DirectoryCopy(args[0], args[1], true);
            doForEachFile(args[0], test);
            doForEachFile(args[0], collectHandler);
            doForEachFile(args[1], replaceHandler);

            Console.In.ReadLine();
        }


        // Represnts an AL-object //
        public class TextElement
        {
            public String m_keyword;
            public int m_objectNumber;
            public String m_objectName;
            public Boolean m_usesQuotes;

            public TextElement(String keyword, int objectNumber, String objectName)
            {
                m_keyword = keyword;
                m_objectNumber = objectNumber;
                m_usesQuotes = objectName.StartsWith("\"");
                m_objectName = objectName;
            }

            public String newObjName() // returns objName with prefix
            {
                if (m_usesQuotes)
                {
                    return $"\"{prefix}{m_objectName.Substring(1, m_objectName.Length - 2)}\"";
                }
                else
                {
                    return $"{prefix}{m_objectName}";
                }
            }
            public int newObjNum() // returns sum of objNum  nad shifting num
            {
                return shiftingNum + m_objectNumber;
            }


            public void replace(string sDir)
            {
                string text = System.IO.File.ReadAllText(sDir);
                // replace defenition
                text = Regex.Replace(text, $"{m_keyword}\\s+{m_objectNumber}\\s+{m_objectName}", $"{m_keyword} {newObjNum()} {newObjName()}");

                // replace instantiation 
                if (m_keyword == "table")
                {
                    text = Regex.Replace(text, $"Record\\s+{m_objectName}", $"Record {newObjName()}");
                }
                else
                {
                    text = Regex.Replace(text, $"{m_keyword}\\s+{m_objectName}", $"{m_keyword} {newObjName()}");
                }
                System.IO.File.WriteAllText(sDir, text);
            }
            // overload for == and != operator to compare TextElements
            public static Boolean operator ==(TextElement a, TextElement b)
                => a.m_keyword == b.m_keyword &&
                a.m_objectNumber == b.m_objectNumber &&
                a.m_objectName == b.m_objectName;
            public static Boolean operator !=(TextElement a, TextElement b)
                => !(a.m_keyword == b.m_keyword &&
                a.m_objectNumber == b.m_objectNumber &&
                a.m_objectName == b.m_objectName);
        }

        //Class that represents a collection of text elements
        public class TextElementCollection
        {
            List<TextElement> textElements = new List<TextElement>();

            // collects all symbols and puts them into "textElements" //
            public void collectAll(string sDir)
            {
                String nText = File.ReadAllText(sDir);
                for (Match match = Regex.Match(nText, "(?mx)^(\\w{1,30}) \\s+ (\\d{1,9}) \\s+ (\\w{1,30}|\\\"\\S{1,30}\\\") #Keyword ID ObjectName etc"); match.Success; match = match.NextMatch())
                {
                    TextElement matchedElement = new TextElement(match.Groups[1].ToString(), int.Parse(match.Groups[2].ToString()), match.Groups[3].ToString());
                    Boolean isADupe = false;
                    foreach (TextElement elem in textElements)
                    {
                        if (matchedElement == elem)
                        {
                            isADupe = true;
                            break;
                        }
                    }
                    if (!isADupe)
                    {
                        textElements.Add(matchedElement);
                    }
                }
            }

            public void replaceAll(string sDir) // replaces all instances of old names and numbers with current versions, file at location.
            {
                foreach (TextElement textElement in textElements)
                {
                    textElement.replace(sDir);
                }
            }

            public string toSCV()
            {
                string scv
                for 

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
    }
}