using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Linq;
using System.Xml.Linq;
public class XMLToString
{
    // Handler for the comparison function to use
    public delegate bool CompareHandler(string xml1, string xml2, float threshold);

    private const string FolderPath = "XML/";
    private static string? FirstName;
    private static string? SecondName;
    
    public static Dictionary<string,object> Run(object filename1, object filename2, Dictionary<string, int> codeLines, bool parseExclusively = true, bool compareValues = false, float threshold = 0)
    {
        // If the filename is a Dictionary<string, object> then use the first key
        // If its a string, use the string
        FirstName = filename1 is Dictionary<string, object> ? null : (string) filename1;
        SecondName = filename2 is Dictionary<string, object> ? null : (string) filename2;
        
        // Minify the XML files (ProjectSourcePath.Value + FolderPath + FirstName)
        // and (ProjectSourcePath.Value + FolderPath + SecondName)
        // Only if they're not Dictionary<string, Dictionary<string, object>>
        // If they are, then they're already minified
        string path1 = filename1 is not Dictionary<string, object> ? MinifyXML(ProjectSourcePath.Value + FolderPath + FirstName) : null;
        string path2 = filename2 is not Dictionary<string, object> ? MinifyXML(ProjectSourcePath.Value + FolderPath + SecondName) : null;

        // Console.ForegroundColor = ConsoleColor.Blue;
        // // Dont count the lines with only a closing tag (e.g. </node>)
        // Console.WriteLine("Lines in " + FirstName + ": " + File.ReadLines(path1).Count(line => !line.Contains("</")));
        // Console.WriteLine("Lines in " + SecondName + ": " + File.ReadLines(path2).Count(line => !line.Contains("</")));
        // Console.ResetColor();

        object parsed1_xml;
        object parsed2_xml;
        if (parseExclusively)
        {
            //DONE: If parseExclusively is true, add the original line counter to the DONE: referenced Tree_XMLParser Dictionary
            // If the name doesnt contain a ^, then its a file
            if (FirstName is not null && !FirstName.Contains('^'))
            {
                // Add the File.ReadLines(path1).Count(line => !line.Contains("</")) to the codeLines
                codeLines.Add(FirstName, File.ReadLines(path1).Count(line => !line.Contains("</")));
            }

            if (SecondName is not null && !SecondName.Contains('^'))
            {
                // Add the File.ReadLines(path2).Count(line => !line.Contains("</")) to the codeLines
                codeLines.Add(SecondName, File.ReadLines(path2).Count(line => !line.Contains("</")));
            }

            // We cant modify the original objects (filename1 and filename2) so we need to create copy them
            object parsed1 = filename1.Copy();
            object parsed2 = filename2.Copy();
        
            // Set the parser if the file is not a dictionary
            parsed1_xml = filename1 is not Dictionary<string, object> ? SetXmlParser(path1) : parsed1;
            parsed2_xml = filename2 is not Dictionary<string, object> ? SetXmlParser(path2) : parsed2;
        }
        else
        {
            // Set the parser if the file is not a dictionary
            parsed1_xml = filename1 is not Dictionary<string, object> ? SetXmlParser(path1) : filename1;
            parsed2_xml = filename2 is not Dictionary<string, object> ? SetXmlParser(path2) : filename2;
        }
        
        var results = CompareXmlOutputs(parsed1_xml, parsed2_xml, compareValues, threshold);

        if (!parseExclusively)
        {
            // If the name doesnt contain a ^, then its a file. Add the individual file to the codeLines
            if (FirstName is not null && !FirstName.Contains('^'))
            {
                codeLines.Add(FirstName,results.Item2.Item1.Count + 1);
            }

            if (SecondName is not null && !SecondName.Contains('^'))
            {
                codeLines.Add(SecondName,results.Item2.Item2.Count + 1);
            }
        }
        
        return results.Item1;

    }
    
    private static string CreateXmlFromDict(Dictionary<string, object> parsed)
    {
        // For every key in the dictionary
        // Read the list<object> and foreach Dictionary<string, object> in the list
        // Create an xml formatted string from each __line__ in them
        // Then add the string to the xml file
        
        string xml = "";
        string closingLine = "";
        foreach (var (key, value) in parsed)
        {
            if (value is not List<object> list) continue;
            foreach (var item in list)
            {
                if (item is not Dictionary<string, object> dict) continue;
                
                // If the dictionary only contains 3 keys then it's a container (__name__, __line__, __depth__)
                // If it contains more than 3 keys then it's a line (__name__, __line__, __depth__, @VALUE)
                if (dict.Count == 3)
                {
                    // Try to get the __line__ key from the dictionary
                    if (dict.TryGetValue("__line__", out var line))
                    {
                        // If the line is a string then add it to the xml file
                        if (line is string str)
                        {
                            xml += closingLine;
                            closingLine = "";
                            // Get the string until the first new line character
                            xml += str.Substring(0, str.IndexOf('\r')) + "\r";
                            // Get the last line of the string
                            closingLine += str.Substring(str.LastIndexOf('\r') + 1);
                        }
                    }
                }
                
                // If the dictionary contains more than 3 keys then it's a line (__name__, __line__, __depth__, @VALUE)
                else if (dict.Count > 3)
                {
                    // Try to get the __line__ key from the dictionary
                    if (dict.TryGetValue("__line__", out var line))
                    {
                        // If the line is a string then add it to the xml file
                        if (line is string str)
                        {
                            // Get the string until the first new line character
                            xml += line + "\r";
                        }
                    }
                }
            }
        }
        xml += closingLine;

        return xml;

    }

    /// <summary>
    /// Removes all the unnecessary spaces and new lines from the XML file as well as comments
    /// Also moves the closing tag to the same line as the last attribute
    /// Saves the minified XML file to the same folder as the original file with the name of the original file + _minified.xml
    /// </summary>
    /// <param name="path"></param>
    /// <returns> Returns the path of the minified XML file </returns>
    public static string MinifyXML(string path)
    {
        // If path doesnt haev the .xml extension then add it
        if (!path.EndsWith(".xml")) path += ".xml";
        // Check if the minified file already exists which is the same name but in the Minified folder
        // XML/Filename.xml -> XML/Minified/Filename.xml
        string minifiedPath = path.Replace(FolderPath, FolderPath + "Minified\\");
        string processedPath = path.Replace(FolderPath, FolderPath + "Processed\\");
        if (File.Exists(minifiedPath)) return minifiedPath;
        if (File.Exists(processedPath)) return processedPath;
        
        // Load the XML file into an XDocument object
        XDocument doc = XDocument.Load(path);

        // Remove all the comments
        doc.DescendantNodes().OfType<XComment>().Remove();

        // Remove all the unnecessary spaces and new lines
        doc.DescendantNodes().OfType<XText>().Where(n => string.IsNullOrWhiteSpace(n.Value)).Remove();

        // Remove any metadata like DOCTYPE or XML declaration
        doc.Declaration = null;
        doc.DocumentType?.Remove();

        // Save the minified XML file to the Minified folder with the same name as the original file
        doc.Save(minifiedPath);
        
        return minifiedPath;

    }

    private static void PrintDict(Dictionary<string, object> dict, int indent = 0)
    {
        // Since there can be multiple nested lists, we need to make it recursive
        foreach (KeyValuePair<string, object> pair in dict)
        {
            switch (pair.Value)
            {
                case List<object> list:
                    PrintXmlData(list, indent + 2, false, 0);
                    break;
                case Dictionary<string, object> value:
                    PrintDict(value, indent + 2);
                    break;
                default:
                    Console.WriteLine(new string(' ', indent) + pair.Key + ": " + pair.Value);
                    break;
            }
        }
    }

    private static List<object> SetXmlParser(string path)
    {
        // Load the XML file into an XDocument object
        XDocument doc = XDocument.Load(path);

        // Create a nested list to store the XML data
        List<object> data = new List<object>();

        // Traverse the XML tree recursively and populate the list
        if (doc.Root != null) ParseXml(doc.Root, data);

        return data;
    }
    
    // Recursive function to traverse the XML tree and populate the list
    private static void ParseXml(XElement element, List<object> list, int depth = 0)
    {
        // Create a dictionary to store the element attributes and parameters
        Dictionary<string, object> dict = new Dictionary<string, object>();

        // Add the element name to the dictionary
        dict.Add("__name__", element.Name.LocalName);

        // Add any attributes to the dictionary
        foreach (XAttribute attr in element.Attributes())
        {
            dict.Add("@" + attr.Name.LocalName, attr.Value);
        }
        
        // Add the line to the dictionary cutting it on the first \n
        dict.Add("__line__", element.ToString()); // .Split(new[] { Environment.NewLine }, StringSplitOptions.None)[0]);
        dict.Add("__depth__", depth);

        // If the element has no children, add the dictionary to the list and return
        if (!element.HasElements)
        {
            list.Add(dict);
            return;
        }

        // If the element has children, add the dictionary to the list and recurse
        list.Add(dict);

        foreach (XElement child in element.Elements())
        {
            List<object> childList = new List<object>();
            ParseXml(child, childList, depth + 1);

            list.Add(childList);
        }
    }
    
    // Function to print the XML data to the console
    private static void PrintXmlData(List<object> list, int indent = 0, bool showLines = true, int depth = 0)
    {
        foreach (object item in list)
        {
            if (item is List<object>)
            {
                PrintXmlData((List<object>)item, indent + 2, showLines, depth + 1);
            }
            else if (item is Dictionary<string, object>)
            {
                Dictionary<string, object> dict = (Dictionary<string, object>)item;
                string indentStr = new string(' ', indent);
                Console.WriteLine(indentStr + "[" + depth + "] " + dict["__name__"]);
    
                foreach (string key in dict.Keys)
                {
                    if (key != "__name__" && key != "__line__")
                    {
                        Console.WriteLine(indentStr + "  " + key + " = " + dict[key]);
                    }
                }
    
                if (showLines)
                {
                    Console.WriteLine(indentStr + "  __line__ = " + dict["__line__"]);
                }
            }
        }
    }

    /// <summary>
    /// Compare two XML files and return the common elements
    /// </summary>
    /// <param name="list1"> First list of XML data </param>
    /// <param name="list2"> Second list of XML data </param>
    /// <param name="compareValues" type="bool"> If true, compare the values of the elements, otherwise only compare the existence of the elements </param>
    /// <param name="threshold" type="int"> Number of differences allowed before the elements are considered different (Levenshtein comparison) </param>
    /// <returns></returns>
    public static Tuple<Dictionary<string,object>,Tuple<List<string>,List<string>>> CompareXmlOutputs(object output1, object output2, bool compareValues = false, float threshold = 0)
    {
        // If output1 is a List<object> we call XmlParserToDictionary to get the Dictionary<string, object>
        // If output1 is a Dictionary<string, object> we just assign it to fullDict1
        Dictionary<string, object> fullDict1 = output1 is List<object> list1 ? XmlParserToDictionary(list1) : (Dictionary<string, object>)output1;
        Dictionary<string, object> fullDict2 = output2 is List<object> list2 ? XmlParserToDictionary(list2) : (Dictionary<string, object>)output2;

        // Compare the two dictionaries
        var comparisonResults = CompareDictionaries(fullDict1, fullDict2, compareValues, threshold);

        return comparisonResults;
    }

    private static Dictionary<string,object> XmlParserToDictionary(List<object> xmlInput, bool debug = false)
    {
        Dictionary<string,object> fullDict = new Dictionary<string, object>();
        // Get the Dictionary<string, object> from the first list
        Dictionary<string, object> dict = GetDictionary(xmlInput).Item2 ? GetDictionary(xmlInput).Item1 : new Dictionary<string, object>();

        if (debug)
        {
            // Print the dictionary to the console
            foreach (var key in dict.Keys.Where(key => key != "__line__"))
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write(key);
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(" = ");
                Console.WriteLine(dict[key]);
                Console.ResetColor();
            }
            Console.WriteLine("--------------------");
        }
        
        // Add the values to the full dictionary (__name__ -> other values as List to expand if same names are found)
        // This means we search in the dictionary by the name and then, inside, we have a dictionary with lists of values (__name__ -> List of values)
        if (fullDict.ContainsKey(dict["__name__"].ToString()))
        {
            // If the key already exists, add the values to the list
            string name = dict["__name__"].ToString();
            ((List<object>)fullDict[name]).Add(dict);
        }
        else
        {
            // If the key doesn't exist, create a new list and add the values
            var list = new List<object>();
            string name = dict["__name__"].ToString();
            list.Add(dict);
            fullDict.Add(name, list);
        }
        
        // If there are List<object> in the list, call the function again
        foreach (var item in xmlInput.Where(item => item is List<object>))
        {
            var nestedDict = XmlParserToDictionary((List<object>)item);
            // Merge the dictionaries
            foreach (var key in nestedDict.Keys)
            {
                if (fullDict.ContainsKey(key))
                {
                    // If the key already exists, add the values to the list
                    ((List<object>)fullDict[key]).AddRange((List<object>)nestedDict[key]);
                }
                else
                {
                    // If the key doesn't exist, create a new list and add the values
                    var list = new List<object>();
                    list.AddRange((List<object>)nestedDict[key]);
                    fullDict.Add(key, list);
                }
            }
        }

        return fullDict;
    }

    private static Tuple<Dictionary<string,object>,Tuple<List<string>,List<string>>> CompareDictionaries(Dictionary<string,object> dict1, Dictionary<string,object> dict2,
        bool compareValues = false, float threshold = 0, bool debug = false)
    {
        Dictionary<string, object> commonDict = new Dictionary<string, object>();
        List<string> commonLines = new List<string>();
        List<string> individualLines1 = new List<string>();
        List<string> individualLines2 = new List<string>();
        int commonLinesCount = 0;
        bool switched = false;

        // Dict1 should be the smaller dictionary
        if (dict1.Count > dict2.Count)
        {
            (dict1, dict2) = (dict2, dict1);
            switched = true;
        }
        
        // Compare the two dictionaries (if the keys are the same, we process their values after adding the line to the dictionary)
        for (int i = 0; i < dict1.Count; i++)
        {
            string key = dict1.Keys.ElementAt(i);

            if (!dict2.ContainsKey(key)) continue;
            if (debug)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Generic key found in " + FirstName.Remove(FirstName.Length - 4) +"^"+SecondName.Remove(FirstName.Length - 4) + ": " + key);
                Console.ResetColor();
            }
                
            // If the key exists in both dictionaries, we move to the inner dictionaries
            List<object> list1 = (List<object>)dict1[key];
            List<object> list2 = (List<object>)dict2[key];

            // Each dictionary in the list has the same values, we need to compare one dictionary with all the dictionaries in the other list
            // and if there is a match, we add the line to the commonLines list and remove the dictionary from the list
            ListOfDictionariesComparison(list1, list2, out List<object> listToDict, out List<string> commonLinesTemp, 
                out List<string> individualLines1Temp, out List<string> individualLines2Temp, 
                new CompareHandler(LevenshteinNormalizedCompare),
                compareValues, threshold, false);

            // Add the common lines to the commonLines list
            commonLines.AddRange(commonLinesTemp);
            // Add the individual lines to the individualLines lists
            individualLines1.AddRange(individualLines1Temp);
            individualLines2.AddRange(individualLines2Temp);
                
            // Add the full original elements to the common dictionary (used for future comparisons keeping the same structure)
            commonDict.Add(key, listToDict);

            // Check which list is empty and remove the key from the dictionary
            if (list1.Count == 0)
            {
                // If the list is empty, remove the key from the dictionary
                dict1.Remove(key);
                i--;
            }
            if (list2.Count == 0)
            {
                // If the list is empty, remove the key from the dictionary
                dict2.Remove(key);
            }
        }
        
        // Add the leftover dictionaries in dict1 to the individualLines1 list
        foreach (var key in dict1.Keys)
        {
            if (debug)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Generic key found only in " + FirstName.Remove(FirstName.Length - 4) + ": " + key);
            }

            foreach (Dictionary<string,object> item in (List<object>)dict1[key])
            {
                individualLines1.Add(item["__line__"].ToString().Split('\n')[0]);
            }
        }

        
        // Add the leftover dictionaries in dict2 to the individualLines2 list
        foreach (var key in dict2.Keys)
        {
            if (debug)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("Generic key found only in " + SecondName.Remove(FirstName.Length - 4) + ": " + key);
            }

            individualLines2.AddRange(from Dictionary<string, object> item in (List<object>)dict2[key] select item["__line__"].ToString().Split('\n')[0]);
        }

        if (FirstName != null && SecondName != null)
        {
            // Get the number of lines in the commonLines list (each line can contain multiple lines (\n))
            commonLinesCount = commonLines.Sum(line => line.Split('\n').Length) + 1;
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine("Common lines between " + FirstName.Remove(FirstName.Length - 4) + "^" + SecondName.Remove(SecondName.Length - 4) + ": " + commonLinesCount);
            // Get the number of lines in the individualLines lists (switch the lists if the dict1 is the smaller dictionary)
            Console.WriteLine("Individual lines in " + FirstName.Remove(FirstName.Length - 4) + ": " + (switched ? individualLines2.Count : individualLines1.Count));
            Console.WriteLine("Individual lines in " + SecondName.Remove(SecondName.Length - 4) + ": " + (switched ? individualLines1.Count : individualLines2.Count));
            Console.ResetColor();
        }

        // Save the common lines to a file after removing \n and \r
        // File.WriteAllLines(ProjectSourcePath.Value + FolderPath +FirstName.Remove(FirstName.Length - 4) + "^" + SecondName.Remove(FirstName.Length - 4) + "_commonlines.txt", 
        //     commonLines.Select(line => line.Replace("\n", "").Replace("\r", "")));
        
        // Return a tuple commonDict and Tuple(individualLines1, individualLines2)
        // or a tuple commonDict and Tuple(individualLines2, individualLines1) if the dict1 is the smaller dictionary
        return Tuple.Create(commonDict, switched ? Tuple.Create(individualLines2, individualLines1) : Tuple.Create(individualLines1, individualLines2));
    }

    public static void ListOfDictionariesComparison(List<object> dict1, List<object> dict2, out List<object> listToDict,
        out List<string> commonLines, out List<string> individualLines1, out List<string> individualLines2, 
        CompareHandler compareHandler,
        bool compareValues = false, float threshold = 0, bool debug = true)
    {
        // Compare each dictionary in the list with all the dictionaries in the other list for a match or the closest match
        // If there is a match, add the line to the commonLines list and remove the dictionary from the list
        // If there is no match, add the line to the individualLines1 list and remove the dictionary from the list
        // The leftover dictionaries in the other list are added to the individualLines2 list as __line__ values
        commonLines = new List<string>();
        individualLines1 = new List<string>();
        individualLines2 = new List<string>();
        listToDict = new List<object>();
        
        // dict1 should be the bigger list
        if (dict1.Count > dict2.Count)
        {
            (dict1, dict2) = (dict2, dict1);
        }
        
        for (int i = 0; i < dict1.Count; i++)
        {
            Dictionary<string, object> item1 = (Dictionary<string, object>)dict1[i];
            
            bool matchFound = false;
            for (int j = 0; j < dict2.Count && !matchFound; j++)
            {
                Dictionary<string, object> item2 = (Dictionary<string, object>)dict2[j];
                // If the dictionaries are the same, add the line to the commonLines list and remove the dictionary from the list
                if (!PureDictionaryComparison(item1, item2, compareHandler, compareValues, threshold, debug)) continue;
                listToDict.Add(item1);
                commonLines.Add(item1["__line__"].ToString().Split('\n')[0]);
                dict1.Remove(item1);
                dict2.Remove(item2);
                matchFound = true;
                    
                // Since we removed the dictionary from the list, we need to decrease the index
                i--;
            }
        }
    }

    private static bool PureDictionaryComparison(Dictionary<string, object> dict1, Dictionary<string, object> dict2, 
        CompareHandler compareHandler,
        bool compareValues = false, float threshold = 0, bool debug = true)
    {
        // Compare each key in the dictionary with all the keys in the other dictionary for a match or the closest match
        // If there is a match, compare the values
        // If there is no match, return false
        // If all the keys match, return true
        // For each key that is not __name__, __line__ or __depth__
        foreach (var key in dict1.Keys.Where(key => key is not ("__name__" or "__line__" or "__depth__")))
        {
            if (dict2.ContainsKey(key))
            {
                // If compareValues is true, compare the values, otherwise return true
                if (!compareValues) continue;
                // If the values are the same, return true
                if (dict1[key].ToString() == dict2[key].ToString()) continue;
                // If the values are not the same, compare them using the compareHandler
                if (!compareHandler(dict1[key].ToString(), dict2[key].ToString(), threshold))
                {
                    // If the values are not the same, return false
                    return false;
                }
            }
            else
            {
                // If the key doesn't exist in the second dictionary, return false
                return false;
            }
        }

        return true;
    }

    private static Tuple<Dictionary<string, object>,bool> GetDictionary(List<object> list)
    {
        Dictionary<string, object> dict = new Dictionary<string, object>();

        foreach (var item in list.Where(item => item is Dictionary<string, object>))
        {
            dict = (Dictionary<string, object>)item;
            return new Tuple<Dictionary<string, object>, bool>(dict, true);
        }

        return new Tuple<Dictionary<string, object>, bool>(dict, false);
    }

    public static bool LevenshteinCompare(string str1, string str2, float threshold)
    {
        int[,] d = new int[str1.Length + 1, str2.Length + 1];
        int cost;

        if (Math.Abs(str1.Length - str2.Length) > threshold)
        {
            return false;
        }

        for (int i = 0; i <= str1.Length; i++)
        {
            d[i, 0] = i;
        }

        for (int j = 0; j <= str2.Length; j++)
        {
            d[0, j] = j;
        }

        for (int i = 1; i <= str1.Length; i++)
        {
            for (int j = 1; j <= str2.Length; j++)
            {
                cost = (str2[j - 1] == str1[i - 1]) ? 0 : 1;

                d[i, j] = Math.Min(
                    Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                    d[i - 1, j - 1] + cost);
            }
        }

        return d[str1.Length, str2.Length] <= threshold;
    }

    public static bool LevenshteinNormalizedCompare(string str1, string str2, float threshold)
    {
        int[,] d = new int[str1.Length + 1, str2.Length + 1];
        int cost;

        if (Math.Abs(str1.Length - str2.Length) > threshold)
        {
            return false;
        }

        for (int i = 0; i <= str1.Length; i++)
        {
            d[i, 0] = i;
        }

        for (int j = 0; j <= str2.Length; j++)
        {
            d[0, j] = j;
        }

        for (int i = 1; i <= str1.Length; i++)
        {
            for (int j = 1; j <= str2.Length; j++)
            {
                cost = (str2[j - 1] == str1[i - 1]) ? 0 : 1;

                d[i, j] = Math.Min(
                    Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                    d[i - 1, j - 1] + cost);
            }
        }

        return d[str1.Length, str2.Length] <= threshold * Math.Max(str1.Length, str2.Length);
    }

}         
