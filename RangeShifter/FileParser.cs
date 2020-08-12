using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

// Represnts an AL-object //
namespace FileParser
{
	
	public class TextElement
	{
		public String m_keyword;
		public int m_objectNumber;
		public String m_objectName;

		public int new_objectNumber;
		public String new_objectName;

		public Boolean m_usesQuotes;
		public String m_path;

		public TextElement(String keyword, int objectNumber, String objectName, String path)
		{
			m_keyword = keyword;
			m_objectNumber = objectNumber;
			m_usesQuotes = objectName.StartsWith("\"");
			m_objectName = objectName;
			m_path = path;

			new_objectNumber = newObjNum();
			new_objectName = newObjName();
		}

		public TextElement(String path, String keyword, int objectNumber, String objectName, int newObjNumber, String newObjName)
		{
			m_keyword = keyword;
			m_objectNumber = objectNumber;
			m_usesQuotes = objectName.StartsWith("\"");
			m_objectName = objectName;
			m_path = path;
			new_objectName = newObjName;
			new_objectNumber = newObjNumber;
		}

		public String newObjName() // returns objName with prefix
		{
			if (m_usesQuotes)
			{
				return $"{RangeShifter.Program.prefix}{m_objectName.Substring(1, m_objectName.Length - 2)}";
			}
			else
			{
				return $"{RangeShifter.Program.prefix}{m_objectName}";
			}
		}
		public int newObjNum() // returns sum of objNum  nad shifting num
		{
			return RangeShifter.Program.shiftingNum + m_objectNumber;
		}


		public void replace(string sDir)
		{
			string text = System.IO.File.ReadAllText(sDir);
			// replace defenition
			text = Regex.Replace(text, $"{m_keyword}\\s+{m_objectNumber}\\s+{m_objectName}", $"{m_keyword} {new_objectNumber} {new_objectName}");

			// replace instantiation 
			if (m_keyword == "table")
			{
				text = Regex.Replace(text, $"Record\\s+{m_objectName}", $"Record {new_objectName}");
			}
			else
			{
				text = Regex.Replace(text, $"{m_keyword}\\s+{m_objectName}", $"{m_keyword} {new_objectName}");
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
				TextElement matchedElement = new TextElement(match.Groups[1].ToString(), int.Parse(match.Groups[2].ToString()), match.Groups[3].ToString(), sDir);
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

		public void exportCSV(string sDir)
		{
			string CSV = $"Path, Keyword, ObjectNumber, Objectname ->, newObjectNumber, newObjectName";

			foreach (TextElement textElement in textElements)
			{
				CSV += $"\n{textElement.m_path}, {textElement.m_keyword}, {textElement.m_objectNumber}, {textElement.m_objectName}, {textElement.new_objectNumber}, {textElement.new_objectName}";
			}
			File.WriteAllText(sDir, CSV);
		}

		public void importCSV(String sDir)
		{
			string CSV = File.ReadAllText(sDir);
			String[] CSVArr = CSV.Split("\n");
			textElements.Clear();
			for (int i = 1; i < CSVArr.Length; i++)
			{
				String[] rowArr = CSVArr[i].Split(",");
				if (rowArr.Length != 6) { break; }
				TextElement matchedElement = new TextElement(rowArr[0].Trim(' '), rowArr[1].Trim(' '), int.Parse(rowArr[2].Trim(' ')), rowArr[3].Trim(' '), int.Parse(rowArr[4].Trim(' ')), rowArr[5].Trim(' '));
				//(String path, String keyword, int objectNumber, String objectName, int newObjNumber, String newObjName)
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

	}
}