using System.Collections.Generic;
using System.Xml;
using System.IO;
using System.Linq;
using System.Xml.Linq;
public class XMLToString
{
    
    public static void Run(string[] args)
    {
        List<object> parsed1_xml = SetXmlParser(ProjectSourcePath.Value + "XML/Maia.xml");
        List<object> parsed2_xml = SetXmlParser(ProjectSourcePath.Value + "XML/Argos.xml");
        
        PrintXmlData(parsed1_xml, showLines: false);
    }

    static List<object> SetXmlParser(string path)
    {
        // Load the XML file into an XDocument object
        XDocument doc = XDocument.Load(path);

        // Create a nested list to store the XML data
        List<object> data = new List<object>();

        // Traverse the XML tree recursively and populate the list
        ParseXml(doc.Root, data);
        
        return data;
    }
    
    // Recursive function to traverse the XML tree and populate the list
    static void ParseXml(XElement element, List<object> list)
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

        // Add the original XML line to the dictionary
        dict.Add("__line__", element.ToString());

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
            ParseXml(child, childList);
            list.Add(childList);
        }
    }

    // Function to print the XML data to the console
    static // Function to print the XML data to the console
        void PrintXmlData(List<object> list, int indent = 0, bool showLines = true)
    {
        foreach (object item in list)
        {
            if (item is List<object>)
            {
                PrintXmlData((List<object>)item, indent + 2, showLines);
            }
            else if (item is Dictionary<string, object>)
            {
                Dictionary<string, object> dict = (Dictionary<string, object>)item;
                string indentStr = new string(' ', indent);
                Console.WriteLine(indentStr + dict["__name__"]);

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

}         
