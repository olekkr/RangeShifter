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
        const int shiftingNum = 70443950;
        const string prefix = "BGo";

        static void Main(string[] args)
        {
            //Dictionary<String, String[]> replacements;
            List<String[]> replacements = new List<String[]> { };
            List<String[]> tabelReps = new List<String[]> { };



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
                for (Match match = Regex.Match(nText, "(?mx)^(\\w{1,30}) \\s+ (\\d{1,9}) \\s+ (\\w{1,30}|\\\"\\S{1,30}\\\") #Keyword ID ObjectName etc"); match.Success; match = match.NextMatch())
                {
                    Console.WriteLine(("match.Groups[0]", match.Groups[1]));
                    if (match.Groups[1].ToString().Equals("table"))
                    {
                        tabelReps.Add(new String[] { match.Groups[1].ToString(), match.Groups[2].ToString(), match.Groups[3].ToString() });
                    }
                    else
                    {
                        replacements.Add(new String[] { match.Groups[1].ToString(), match.Groups[2].ToString(), match.Groups[3].ToString() });
                    }
                }
                foreach (string[] tabelElem in tabelReps) {
                    if (tabelElem[2][0].Equals("\""))
                    {
                        
                    }
                    else {
                        nText = Regex.Replace(nText, ("table\\s+" + tabelElem[1] + "\\s+" + tabelElem[2]), ("table " + (int.Parse(tabelElem[1]) + shiftingNum).ToString() + " " + prefix + tabelElem[2]));
                        nText = Regex.Replace(nText, ("Record\\s+" + tabelElem[2]), ("Record " + prefix + tabelElem[2]));
                    }
                }
                foreach (string[] elem in replacements)
                {
                    if (elem[2][0].Equals("\"")) { //if ""-mode
                        //nText = Regex.Replace(nText, (elem[0] + "\\s+" + elem[1] + "\\s+" + elem[2]), (elem[0] + (int.Parse(elem[1]) + shiftingNum).ToString() + " " + prefix + elem[2]));
                        //nText = Regex.Replace(nText, (elem[0] + "\\s+" + elem[2]), elem[0] + " " + prefix + elem[2]); 
                    }
                    else {
                        nText = Regex.Replace(nText, (elem[0] + "\\s+" + elem[1] + "\\s+" + elem[2]), (elem[0] + (int.Parse(elem[1]) + shiftingNum).ToString() + " " + prefix + elem[2]));
                        nText = Regex.Replace(nText, (elem[0] + "\\s+" + elem[2]), elem[0] + " " + prefix + elem[2]);
                    }

                }
            }
        }
    }
}