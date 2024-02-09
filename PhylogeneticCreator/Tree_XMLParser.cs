namespace Graph;

public class Tree_XMLParser
{
    public Dictionary<string,Dictionary<string, object>> XMLParsers;
    public Dictionary<int, Tuple<string, string>> XML_To_Parse;
    public Dictionary<string,int> CodeLines;
    
    // Setters
    public void AddXMLToParse(int index, Tuple<string, string> xmlToParse)
    {
        XML_To_Parse.Add(index, xmlToParse);
    }
    
    // Constructors
    public Tree_XMLParser()
    {
        XMLParsers = new Dictionary<string, Dictionary<string, object>>();
        XML_To_Parse = new Dictionary<int, Tuple<string, string>>();
        CodeLines = new Dictionary<string, int>();
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="parseExclusively"> If true, parsed values will be exclusive (removing lines from parents), if false, parsed values will be inclusive (building on top of parents) </param>
    public void Run(bool parseExclusively = true, bool compareValues = false, int threshold = 0)
    {
        // For each XML_To_Parse, call XMLToString and add the result to XMLParsers
        foreach (var xmlToParse in XML_To_Parse)
        {
            object file1 = xmlToParse.Value.Item1;
            object file2 = xmlToParse.Value.Item2;
            string name = xmlToParse.Value.Item1 + '^' + xmlToParse.Value.Item2;
            // Check if the name already exists in XMLParsers and retrieve the Dictionary if it does
            if (XMLParsers.ContainsKey(xmlToParse.Value.Item1))
            {
                file1 = XMLParsers[xmlToParse.Value.Item1];
            }

            if (XMLParsers.ContainsKey(xmlToParse.Value.Item2))
            {
                file2 = XMLParsers[xmlToParse.Value.Item2];
            }

            // Call XMLToString with either the retrieved Dictionary or the string 
            XMLParsers.Add(name, XMLToString.Run(file1, file2, CodeLines, parseExclusively, compareValues, threshold));
        }
        
        // Fill CodeLines with the number of lines in each file
        for (int i = 0; i < XMLParsers.Count; i++)
        {
            string name = XMLParsers.ElementAt(i).Key;
            Dictionary<string, object> dictionary = XMLParsers.ElementAt(i).Value;
            CodeLines.Add(name, NumberOfLinesFromDictionary(dictionary));
        }
    }
    
    private int NumberOfLinesFromDictionary(Dictionary<string, object> dictionary)
    {
        int numberOfLines = 0;
        foreach (var keyValuePair in dictionary)
        {
            if (keyValuePair.Value is not List<object> list) continue;
            foreach (var value in list)
            {
                if (value is not Dictionary<string, object> objects) continue;
                // Get the __line__ value from the dictionary and get until the first newline
                string line = objects["__line__"].ToString().Split('\n')[0];
                // If the line is not empty, add 1 to the number of lines
                if (line != string.Empty)
                {
                    numberOfLines++;
                }
            }
        }

        return numberOfLines;
    }
}