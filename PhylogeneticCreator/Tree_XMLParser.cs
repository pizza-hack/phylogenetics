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
    public void Run()
    {
        // For each XML_To_Parse, call XMLToString and add the result to XMLParsers
        foreach (var xmlToParse in XML_To_Parse)
        {
            object file1 = xmlToParse.Value.Item1;
            object file2 = xmlToParse.Value.Item2;
            string name = xmlToParse.Value.Item1 + '_' + xmlToParse.Value.Item2;
            // Check if the name already exists in XMLParsers and retrieve the Dictionary if it does
            if (XMLParsers.ContainsKey(xmlToParse.Value.Item1))
            {
                file1 = XMLParsers[name];
            }

            if (XMLParsers.ContainsKey(xmlToParse.Value.Item2))
            {
                file2 = XMLParsers[name];
            }

            // Call XMLToString with either the retrieved Dictionary or the string 
            XMLParsers.Add(name, XMLToString.Run(file1, file2));
        }
        
        // Fill CodeLines with the number of lines in each file
        foreach (var xmlParser in XMLParsers)
        {
            CodeLines.Add(xmlParser.Key, NumberOfLinesFromDictionary(xmlParser.Value));
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