using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace RangeShifter
{
    //Range for table object ID numbers 	1 – 999,999,999
    //Maximum number of characters in variable names 	30

    class Program
    {
        const int shiftingCoefficient = 70443950;



        static void Main(string[] args)
        {
            Dictionary<String[], String> replacements;
            string prefix = "BGo";


            if (args.Length == 0 || args.Length > 2)
            {
                Console.Error.Write("[ERROR] wrong number of args");
                Console.In.ReadLine();
                return;
            }
            if (File.Exists(args[0]))
            {
                // Read entire text file content in one string    
                string text = File.ReadAllText(args[0]);
                shiftCoeffs(text);
            }
            else
            {
                Console.Error.Write("[ERROR] file dosent exist");
                Console.In.ReadLine();
                return;
            }

            void shiftCoeffs(String text)
            {
                //fix includes  Regex.Match(String, Int32)
                string nText = text;
                for (Match match = Regex.Match(nText, "{|(?mx)^(\\w{1,30}) \\s+? (\\d{1,9}) \\s+? (\\w{1,30}|\\\"\\S{1,30}\\\") #Keyword ID ObjectName etc"); match.Success; match = match.NextMatch())
                {
                    Console.WriteLine(match.Captures[0]);
                    Console.WriteLine(match.Index);

                    if (match.Captures[0].ToString().Equals("{")) // skip to "blockdepth" 0 and shorten text
                    {
                        int i = 1;
                        int idx = 0;
                        foreach (char chr in nText.Substring(match.Index))
                        {
                            if (chr == '}'){ i -= 1; }
                            else if (chr == '{'){ i += 1; }
                            if (i == 0)
                            {
                                Console.WriteLine(("a", i));
                                //Console.WriteLine(idx-1);
                                nText = nText.Substring(idx-1);
                                break;
                            }
                            idx += 1;
                        }
                    }
                    //else // edit 
                    //{
                    //    Console.WriteLine(1);
                    //   Console.Title = 1;
                    //}



                }


            }
        }
    }
}