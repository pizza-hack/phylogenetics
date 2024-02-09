using System.Diagnostics;
using System.Globalization;
using System.Text;

class _OG_PhylogeneticTree
{
    private delegate float operateDictionary(float a, float b);
    
    public const string dataLocation = "Data/DaniTestModified.csv";
    public const string lastResult = "Data/outputPairs.txt";
    public const string outputLocation = "Data/output.dot";
    public const string graphOutputLocation = "Data/graph.svg";

    public static void Run(string[] args)
    {
        bool usePredefined = false;

        // Read the file which is located in the project folder
        string projectSourcePath = ProjectSourcePath.Value;
        string data = File.ReadAllText(projectSourcePath + dataLocation);
        
        List<DataStructures.OutputPair> outputPairs;
        string[] headers;
        
        if (!usePredefined)
        {
            DataStructures.PhylogeneticDictionary dataDictionary = setDictionary(data);
            outputPairs = new List<DataStructures.OutputPair>();
            // Original headers
            headers = dataDictionary.Headers ?? Array.Empty<string>();
            // Process the dictionary with linealMean
            DataStructures.PhylogeneticDictionary linealMeanDictionary = dictionaryIterator(dataDictionary, outputPairs, true);
            // Write the result to the output file
            exportOutputPairs(outputPairs, headers, projectSourcePath + lastResult);
        }
        else
        {
            Tuple<List<DataStructures.OutputPair>, string[]> predefinedOutput = importOutputPairs(projectSourcePath + lastResult);
            outputPairs = predefinedOutput.Item1;
            headers = predefinedOutput.Item2;
        }

        //createASCIIGraph(outputPairs, headers);
        //exportASCIIGraph(outputPairs, headers, projectSourcePath + "Data/output.txt");
        createGraph(outputPairs, "phylogenetic_tree");
    }

    private static void createASCIIGraph(IReadOnlyCollection<DataStructures.OutputPair> results, string[] ogHeaders, int sameHeaderPad = 2)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Creating graph...");
        Console.WriteLine();
        Console.ResetColor();

        // We print the headers on top with proper padding (nearest multiple of 5 above the length of the longest header)
        // Get the max length of the headers
        int padding = ogHeaders.Max(x => x.Length) + 1;
        
        // Have a dictionary of integers with the header as key and the value as the center of that header
        Dictionary<string, int> headerCenters = new Dictionary<string, int>();
        Dictionary<string, int> currentlyWorking = new Dictionary<string, int>();
        int currentCenter = 0;

        // Round to the next multiple of 5 (min 2 more spaces)
        padding = (int) Math.Ceiling(padding / 5.0) * 5;
        foreach (string header in ogHeaders)
        {
            Console.Write(header.PadRight(padding));
            currentCenter += header.Length / 2;
            headerCenters.Add(header, currentCenter);
            currentlyWorking.Add(header, currentCenter);
            currentCenter += header.Length / 2;
            // Move the currentCenter to the next header, until the padding is reached
            while (currentCenter % padding != 0)
            {
                currentCenter++;
            }
        }
        Console.WriteLine();
        
        // Print a │ for each header
        foreach (string header in ogHeaders)
        {
            int index = headerCenters[header];
            Console.CursorLeft = index;
            Console.Write("│");
            // We need to add the text to the outputToReturn at the correct index
        }
        Console.WriteLine();
        
        List<DataStructures.OutputPair> copyResults = new List<DataStructures.OutputPair>(results);
        
        // Sort the copyResults by the values
        copyResults.Sort((x, y) => x.Value.CompareTo(y.Value));

        // We get the headers that are connected in the currentLayerResults
        foreach (DataStructures.OutputPair currentLayerResult in copyResults)
        {
            string firstHeader = currentLayerResult.First;
            string secondHeader = currentLayerResult.Second;
            // We get the center of the first header
            int firstHeaderIndex = headerCenters[firstHeader];
            // We get the center of the second header
            int secondHeaderIndex = headerCenters[secondHeader];

            // We move the cursor to the smallest index between the two headers
            int minIndex = Math.Min(firstHeaderIndex, secondHeaderIndex);
            Console.CursorLeft = minIndex;
            // └
            Console.Write("└");
            // We move the cursor to the largest index between the two headers 
            int maxIndex = Math.Max(firstHeaderIndex, secondHeaderIndex);
            Console.CursorLeft = maxIndex;
            // ┘
            Console.Write("┘");
            // Between the two headers we print a line
            // ─
            // At the middle of the line we print ┬ instead of ─
            for (int i = minIndex + 1; i < maxIndex; i++)
            {
                Console.CursorLeft = i;
                Console.Write(i == ((minIndex + maxIndex) / 2)-sameHeaderPad ? "┬" : "─");
            }
                          
            // The headers that werent on the current or previous layers get a │ in their position before the next layer
            foreach (string header in currentlyWorking.Keys)
            {
                if (header != firstHeader && header != secondHeader)
                {
                    int index = currentlyWorking[header];
                    Console.CursorLeft = index;
                    Console.Write("│");
                }
            }
            Console.WriteLine();
            
            
            // Add the layer value to the headerCenters
            headerCenters.Add(firstHeader +"^"+ secondHeader, ((minIndex + maxIndex) / 2)-sameHeaderPad);
            currentlyWorking.Add(firstHeader +"^"+ secondHeader, ((minIndex + maxIndex) / 2)-sameHeaderPad);
            // We remove the headers from the currentlyWorking dictionary
            currentlyWorking.Remove(firstHeader);
            currentlyWorking.Remove(secondHeader);
            
            // The headers that werent on the current or previous layers get a │ in their position before the next layer
            foreach (string header in currentlyWorking.Keys)
            {
                Console.CursorLeft = headerCenters[header];
                Console.Write("│");
            }
        }
    }
    
    private static void createGraph(IReadOnlyList<DataStructures.OutputPair> results, string graphName)
    {
        Graph.GraphStructures gf = new Graph.GraphStructures();
        
        // Create a string output to put into the file
        StringBuilder outputToReturn = new StringBuilder();
        
        outputToReturn.Append(gf.Init(graphName));
        
        // Get the last element of the string (path so last element is the name of the file)
        string filename = dataLocation.Split('/').Last();
        
        outputToReturn.Append(gf.Parameters(
            "neato",
            "Phylogenetic Tree from: " + filename,
            "major",
            "shortpath",
            "",
            "fill",
            "portrait"
            ));
        
        Dictionary<string,string> nodes = new Dictionary<string, string>();

        // We process the results
        for (int i = 0; i < results.Count; i++)
        {
            outputToReturn.AppendLine("#" + i);
            
            DataStructures.OutputPair currentResult = results[i];
            
            string firstHeader = currentResult.First;
            
            bool containsSpecialChar1 = firstHeader.Contains('^');
            if (containsSpecialChar1)
            {
                firstHeader = firstHeader.Replace('^', '_');
            }
            
            string secondHeader = currentResult.Second;
            bool containsSpecialChar2 = secondHeader.Contains('^');
            if (containsSpecialChar2)
            {
                secondHeader = secondHeader.Replace('^', '_');
            }
            
            string newHeader = firstHeader + "_" + secondHeader;

            // We add the nodes to the nodes dictionary if they are not already there
            if (!nodes.ContainsKey(firstHeader))
            {
                nodes.Add(firstHeader, firstHeader);
                outputToReturn.AppendLine("\t"+gf.Node(firstHeader,"-1",null, "","octagon","filled","skyblue"));
            }
            if (!nodes.ContainsKey(secondHeader))
            {
                nodes.Add(secondHeader, secondHeader);
                outputToReturn.AppendLine("\t"+gf.Node(secondHeader,"-1", null,"","octagon","filled","skyblue"));
            }

            nodes.Add(newHeader, newHeader);
            if(containsSpecialChar1 && containsSpecialChar2)
            { 
                // If its not the last element
                if (i != results.Count - 1)
                {
                    // A blue color just a little darker than the skyblue
                    string color = "#00bfff";
                    outputToReturn.AppendLine("\t"+gf.Node(newHeader,"-1", null,i.ToString(), "diamond","",color,currentResult.Value.ToString(CultureInfo.InvariantCulture), color));
                }
                else
                {
                    // Deep red
                    string color = "#8B0000";
                    outputToReturn.AppendLine("\t"+gf.Node(newHeader,"-1", null,i.ToString(), "diamond", "diagonals", color,currentResult.Value.ToString(CultureInfo.InvariantCulture), color));
                }
            }
            else
            {
                if (i != results.Count - 1)
                {
                    // Green
                    string color = "#006400";
                    // TODO, add comment with currentResult.Value.ToString(CultureInfo.InvariantCulture)
                    outputToReturn.AppendLine("\t"+gf.Node(newHeader,"-1", null,i.ToString(), "diamond", "rounded", color,currentResult.Value.ToString(CultureInfo.InvariantCulture),color));
                }
                else
                {
                    // Deep red
                    string color = "#8B0000";
                    outputToReturn.AppendLine("\t"+gf.Node(newHeader,"-1", null,i.ToString(), "diamond", "diagonals", color,currentResult.Value.ToString(CultureInfo.InvariantCulture), color));
                }
                
            }

            outputToReturn.Append("\t");
            outputToReturn.AppendLine(containsSpecialChar1
                ? gf.Edge(firstHeader, newHeader, "forward", "dashed")
                : gf.Edge(firstHeader, newHeader, "forward"));
            outputToReturn.Append("\t");
            outputToReturn.AppendLine(containsSpecialChar2
                ? gf.Edge(secondHeader, newHeader, "forward", "dashed")
                : gf.Edge(secondHeader, newHeader, "forward"));

        }
        
        outputToReturn.AppendLine();
        
        outputToReturn.Append(gf.End());
        
        // We write the file
        File.WriteAllText(ProjectSourcePath.Value + outputLocation, outputToReturn.ToString());

        // Create the image from the dot file as a svg
        string dotPath = @"C:\Program Files\Graphviz\bin\dot.exe";
        string dotFile = ProjectSourcePath.Value + outputLocation;
        string svgFile = ProjectSourcePath.Value + graphOutputLocation;
        
        // Get the extension of the file (graphOutputLocation)
        string extension = Path.GetExtension(svgFile);
        
        // Create the command depending on the extension
        string command = extension switch
        {
            ".svg" => $"-Tsvg {dotFile} -o {svgFile}",
            ".png" => $"-Tpng {dotFile} -o {svgFile}",
            ".jpg" => $"-Tjpg {dotFile} -o {svgFile}",
            ".gif" => $"-Tgif {dotFile} -o {svgFile}",
            ".pdf" => $"-Tpdf {dotFile} -o {svgFile}",
            _ => throw new Exception("Invalid extension")
        };

        ProcessStartInfo startInfo = new ProcessStartInfo(dotPath, command)
        {
            UseShellExecute = false,
            RedirectStandardOutput = false,
            CreateNoWindow = true
        };
        Process process = new Process();
        process.StartInfo = startInfo;
        process.Start();
        process.WaitForExit();
        
    }
    
    private static DataStructures.PhylogeneticDictionary dictionaryIterator(DataStructures.PhylogeneticDictionary dict, ICollection<DataStructures.OutputPair> Output, bool outputLayers = true)
    {
        if(outputLayers){
            // Empty the output folder (ProjectSourcePath.Value + "Data/Layers/)
            string[] files = Directory.GetFiles(ProjectSourcePath.Value + "Data/Layers/");
            foreach (string file in files)
            {
                File.Delete(file);
            }
        }

        // While the dictionary has more than one element (outside the diagonal and the repeated elements mirrowed)
        int layerDepth = 0;
        while (dict.Count() > 4)
        {
            dict = properProcessDic(dict, Mean, Output, layerDepth);
            // Export the dictionary to a csv inside Data/Layers/layerDepth.csv
            if (outputLayers)
            {
                exportDictionaryAsCSV(dict, ProjectSourcePath.Value + "Data/Layers/" + layerDepth + ".csv");
            }
            layerDepth++;
        }
        
        // Add the last two elements to the output at the last layer
        DataStructures.OutputPair lastPair = new DataStructures.OutputPair(dict.Headers?[0] ?? string.Empty, dict.Headers?[1] ?? string.Empty, dict.getAllValues().Min());
        Output.Add(lastPair);

        return dict;
    }
    
    // operateDictionary method for lineal mean
    /*
    private static Dictionary processDic(Dictionary dict, operateDictionary operation, List<OutputPair> outputPairs, int layer)
    {
        // Find the smallest value (lowest distance) which pair isnt with itself
        Pair[] smallestPairs = dict.getMinPairs();
        Dictionary dataDictionary = dict;
        
        // If the pairs have a value in common, the second checked pair (the one we detect the value in common)
        // is meged with the other pair (the one we detect the value in common)
        // Else we dont do anything
        smallestPairs = checkForUniquePairs(smallestPairs);

        // The smallest pairs will be combined into a new unique pair (Pair1, Pair2) -> (Pair1 + Pair2)
        // that will be a "new column" to compare the rest of the columns with
        foreach (Pair pair in smallestPairs)
        {
            // If the diccionary size is 4 or less, we dont need to merge the pairs so we break
            Console.WriteLine("Dictionary size: " + dataDictionary.Count());
            if (dataDictionary.Count() <= 4)
            {
                foreach (Pair p in smallestPairs)
                {
                    outputPairs.Add(new OutputPair(p.First, p.Second, layer));
                }
                return dataDictionary;
            }
            Dictionary newDict = new Dictionary();
            string firstKey = pair.First;
            string secondKey = pair.Second;

            // Create the new key
            string newKey = firstKey +"^"+ secondKey;
            
            // The new dictionary will have all the original values
            // but every key combination will be replaced with the new key
            // and the value will be the mean of the two values ((A,B),(C)) -> ((A,C)+(B,C))/2
            foreach (KeyValuePair<Pair, float> entry in dataDictionary)
            {
                Pair key = entry.Key;
                float value = entry.Value;
                
                // If the pair contains both elements, its the diagonal so we set it to 0
                if ((key.Contains(firstKey) && key.Contains(secondKey)) ||
                    (key.First == firstKey && key.Second == firstKey) ||
                    (key.First == secondKey && key.Second == secondKey))
                {
                    newDict.Add(newKey, newKey, 0);
                }
                // if(N1,N2) -> (A,N2) || (N1,A)
                else if (key.First == firstKey)
                {
                    // if(N1,N2) -> (A,N2) && !(A,B)
                    if (key.Second != secondKey)
                    {
                        value = operation(dataDictionary[new Pair(firstKey, key.Second)], value);
                        newDict.Add(newKey, key.Second, value);
                    }
                }
                else if (key.Second == firstKey)
                {
                    if (key.First != secondKey)
                    {
                        value = operation(dataDictionary[new Pair(key.First, secondKey)], value);
                        newDict.Add(key.First, newKey, value);
                    }
                }
                // if the secondkey is in the pair we skip it since it'll be added from the firstkey
                else if (key.Contains(secondKey))
                {
                    continue;
                }
                else
                {
                    newDict.Add(key.First,key.Second, value);
                }
                
            }
            
            // Print the new dictionary
            printDictionaryAsTable(newDict,-1);

            // Set the new dictionary as the data dictionary
            dataDictionary = newDict;
        }
        
        // Add the merged pairs to the outputPairs
        foreach (Pair pair in smallestPairs)
        {
            outputPairs.Add(new OutputPair(pair.First, pair.Second, layer));
        }
        
        return dataDictionary;
    }
    */
    
    private static DataStructures.PhylogeneticDictionary properProcessDic(DataStructures.PhylogeneticDictionary dict, operateDictionary operation, ICollection<DataStructures.OutputPair> outputPairs, int layer)
    {
        // Find the smallest value in the dictionary that isnt with itself
        DataStructures.Pair[] smallestPairs = dict.getMinPairs();
        DataStructures.Pair smallestPair = smallestPairs[0];
        DataStructures.PhylogeneticDictionary dataDictionary = dict;

        // The smallest pairs will be combined into a new unique pair (Pair1, Pair2) -> (Pair1 + Pair2)
        // that will be a "new column" to compare the rest of the columns with

        Console.WriteLine("Dictionary size: " + dataDictionary.Count());
        if (dataDictionary.Count() <= 4)
        {
            outputPairs.Add(new DataStructures.OutputPair(smallestPair.First, smallestPair.Second, dict.getAllValues().Min()));
            return dataDictionary;
        }
        
        Console.WriteLine(smallestPair.First + "^" + smallestPair.Second);
        string firstKey = smallestPair.First;
        string secondKey = smallestPair.Second;

        // Create the new key
        string newKey = firstKey +"^"+ secondKey;
        
        // Copy the dictionary into a temporary dictionary to avoid modifying the original
        DataStructures.PhylogeneticDictionary tempDict = new DataStructures.PhylogeneticDictionary();
        foreach (KeyValuePair<DataStructures.Pair, float> entry in dataDictionary)
        {
            DataStructures.Pair key = entry.Key;
            float value = entry.Value;
            tempDict.Add(key.First, key.Second, value);
        }

        // The new dictionary will have all the original values
        // but every key combination will be replaced with the new key
        // and the value will be the mean of the two values ((A,B),(C)) -> ((A,C)+(B,C))/2
        foreach (KeyValuePair<DataStructures.Pair, float> entry in dataDictionary)
        {
            DataStructures.Pair key = entry.Key;

            // If the pair contains one of the elements we need to update the value
            if (key.Contains(firstKey))
            {
                // If it doesnt contain the second key, we need to update the value
                if (!key.Contains(secondKey))
                {
                    // Get the string of the pair that doesnt contain the first key
                    string otherKey = key.First == firstKey ? key.Second : key.First;
                    
                    if (otherKey == firstKey)
                    {
                        continue;
                    }
                    
                    // Get the value of the pair otherKey, firstKey and otherKey, secondKey
                    float firstValue = tempDict[new DataStructures.Pair(otherKey, firstKey)];
                    float secondValue = tempDict[new DataStructures.Pair(otherKey, secondKey)];
                    float newValue = operation(firstValue, secondValue);
                    // Add the new pair to the dictionary
                    tempDict.Add(otherKey,newKey, newValue);
                    
                }
            }
            else if (key.Contains(secondKey) && !key.Contains(firstKey))
            {
                // Get the string of the pair that doesnt contain the second key
                string otherKey = key.First == secondKey ? key.Second : key.First;
                
                if (otherKey == secondKey)
                {
                    continue;
                }
                
                // Get the value of the pair otherKey, firstKey and otherKey, secondKey
                float firstValue = tempDict[new DataStructures.Pair(otherKey, firstKey)];
                float secondValue = tempDict[new DataStructures.Pair(otherKey, secondKey)];
                float newValue = operation(firstValue, secondValue);
                // Add the new pair to the dictionary
                tempDict.Add(otherKey,newKey, newValue);
            }
        }
        
        // Add the diagonal for the new key
        tempDict.Add(newKey, newKey, 0);
        
        // Remove all pairs that contain the first or second key
        foreach (KeyValuePair<DataStructures.Pair, float> entry in dataDictionary)
        {
            DataStructures.Pair key = entry.Key;
            if (key.Contains(firstKey) || key.Contains(secondKey))
            {
                tempDict.Remove(key);
            }
        }
        
        // Remove the first and second key from the dictionary headers
        tempDict.RemoveHeader(firstKey);
        tempDict.RemoveHeader(secondKey);
        
        // Print the new dictionary
        printDictionaryAsTable(tempDict,-1);

        dataDictionary = tempDict;

        outputPairs.Add(new DataStructures.OutputPair(smallestPair.First, smallestPair.Second, dict.getAllValues().Min()));
        
        return dataDictionary;
    }

    private static DataStructures.Pair[] checkForUniquePairs(DataStructures.Pair[] smallestPairs)
    {
        List<string> keysString = new List<string>();
        DataStructures.Pair[] newSmallestPairs = smallestPairs;
        foreach (DataStructures.Pair pair in smallestPairs)
        {
            // If the values of the pair are not in the keysString list, add them
            // Else (one or both of the values are in the keysString list) remove from the smallestPairs list
            if (!keysString.Contains(pair.First) && !keysString.Contains(pair.Second))
            {
                keysString.Add(pair.First);
                keysString.Add(pair.Second);
            }
            else
            {
                // Get the other pair that has the same value as the current pair
                DataStructures.Pair otherPair = smallestPairs.First(x => x.First == pair.First || x.Second == pair.First);
                
                // Check the non common value of the current pair, the one with less ^ values prevails, the other is removed
                // If both have the same amount of ^ values, this one is removed
                
                string[] nonCommonValues = GetNonCommonValues(pair, otherPair);
                int thisNonCommonValue = nonCommonValues[0].Split('^').Length;
                int otherNonCommonValue = nonCommonValues[1].Split('^').Length;
                
                // Whichever value is bigger is removed
                if (thisNonCommonValue < otherNonCommonValue)
                {
                    // Remove the other pair
                    newSmallestPairs = newSmallestPairs.Where(x => x != otherPair).ToArray();
                }
                else if (thisNonCommonValue > otherNonCommonValue)
                {
                    // Remove the current pair
                    newSmallestPairs = newSmallestPairs.Where(x => x != pair).ToArray();
                }
                else
                {
                    // Remove the current pair
                    newSmallestPairs = newSmallestPairs.Where(x => x != pair).ToArray();
                }
                
            }
        }
        return newSmallestPairs;
    }
    
    private static string[] GetNonCommonValues(DataStructures.Pair pair, DataStructures.Pair otherPair)
    {
        string[] nonCommonValues = new string[2];
        if (pair.First != otherPair.First && pair.First != otherPair.Second)
        {
            nonCommonValues[0] = pair.First;
        }
        else if (pair.Second != otherPair.First && pair.Second != otherPair.Second)
        {
            nonCommonValues[0] = pair.Second;
        }
        if (otherPair.First != pair.First && otherPair.First != pair.Second)
        {
            nonCommonValues[1] = otherPair.First;
        }
        else if (otherPair.Second != pair.First && otherPair.Second != pair.Second)
        {
            nonCommonValues[1] = otherPair.Second;
        }
        return nonCommonValues;
    }

    private static float Mean(float newRData, float extRData)
    {
        return (newRData + extRData) / 2;
    }

    private static DataStructures.PhylogeneticDictionary setDictionary(string data)
    {
        // First line is the header with:
        // Empty; COLUMN_NAMES
        // The other lines are the data:
        // COLUMN_NAME; DATA
        string[] columnNames = data.Split(new[] { Environment.NewLine }, StringSplitOptions.None)[0].Split(';');
        // Clean the column names and remove the first empty column
        columnNames = columnNames.Select(x => x.Trim()).Skip(1).ToArray();
        
        // Get the data
        string[] lines = data.Split(new[] { Environment.NewLine }, StringSplitOptions.None).Skip(1).ToArray();
        // Clean the data and split it removing the separator
        lines = lines.Select(x => x.Trim()).ToArray();
        string[][] dataLines = lines.Select(x => x.Split(';')).ToArray();
        // Trim the data and remove the first column
        dataLines = dataLines.Select(x => x.Select(y => y.Trim()).Skip(1).ToArray()).ToArray();
        // Parse as float
        float[][] dataAsFloat = dataLines.Select(x => x.Select(y => float.Parse(y, CultureInfo.InvariantCulture)).ToArray()).ToArray();

        // New diccionary to store the data
        return setPairs(dataAsFloat, columnNames, true);
    }

    private static DataStructures.PhylogeneticDictionary setPairs(IReadOnlyList<float[]> dataAsFloat, string[] headers, bool includeDiagonal = false)
    {
        DataStructures.PhylogeneticDictionary pairs = new DataStructures.PhylogeneticDictionary();
        // Each pair is a combination of two columns, so we need to iterate over all the columns 
        // For each value, the pair is headers[i] + headers[j] and the value is dataAsFloat[i][j]
        for (int i = 0; i < headers.Length; i++)
        {
            for (int j = 0; j < headers.Length; j++)
            {
                // Skip the diagonal
                if (i == j && !includeDiagonal)
                    continue;
                // Skip the already calculated pairs
                if (i > j)
                    continue;
                // Add the pair
                pairs.Add(headers[i], headers[j], dataAsFloat[i][j]);
            }
        }
        return pairs;
    }
    
    private static void printDictionaryAsTable(DataStructures.PhylogeneticDictionary dict, int padding = -1)
    {
        // Get the headers
        string[] headers = dict.Headers ?? Array.Empty<string>();
        
        // Randomize a console color for each header (unique color for each header if possible)
        Dictionary<string, ConsoleColor> colors = new Dictionary<string, ConsoleColor>();
        // Random with a static seed to get the same colors every time
        Random random = new Random(0);
        
        // Possible colors
        ConsoleColor[] possibleColors = new ConsoleColor[]
        {
            ConsoleColor.Blue,
            ConsoleColor.Cyan,
            ConsoleColor.Green,
            ConsoleColor.Magenta,
            ConsoleColor.Red,
            ConsoleColor.Yellow
        };
        
        // Get the colors
        foreach (string header in headers)
        {
            // Get a random color
            ConsoleColor color = possibleColors[random.Next(0, possibleColors.Length)];
            // If the color is already in the dictionary, get a new one
            while (colors.ContainsValue(color) && colors.Count < possibleColors.Length)
            {
                color = possibleColors[random.Next(0, possibleColors.Length)];
            }
            // Add the color to the dictionary
            colors.Add(header, color);
        }
        

        if (padding == -1)
        {
            // Get the max length of the headers
            padding = headers.Max(x => x.Length);
            // Round to the next multiple of 5
            padding = (int) Math.Ceiling(padding / 3.0) * 3;
        }

        // Print the headers
        Console.Write("".PadRight(padding));
        foreach (string header in headers.Reverse())
        {
            Console.ForegroundColor = colors[header];
            Console.Write(header.PadRight(padding));
        }
        Console.WriteLine();
        // Print the data
        foreach (string header in headers.Reverse())
        {
            Console.ForegroundColor = colors[header];
            Console.Write(header.PadRight(padding));
            foreach (string header2 in headers.Reverse())
            {
                Console.ForegroundColor = colors[header2];
                Console.Write(dict[new DataStructures.Pair(header, header2)].ToString().PadRight(padding));
            }
            Console.WriteLine();
        }
        Console.WriteLine();
        Console.ResetColor();
    }

    private static void exportDictionaryAsCSV(DataStructures.PhylogeneticDictionary dict, string path, char separator = ';')
    {
        // Exports the diccionary as a CSV
        // Get the headers
        string[] headers = dict.Headers ?? Array.Empty<string>();
        // Create the string builder
        StringBuilder sb = new StringBuilder();
        // Print the headers
        sb.Append(separator);
        foreach (string header in headers)
        {
            sb.Append(header);
            sb.Append(separator);
        }
        sb.AppendLine();
        // Print the data
        foreach (string header in headers)
        {
            sb.Append(header);
            sb.Append(separator);
            foreach (string header2 in headers)
            {
                sb.Append(dict[new DataStructures.Pair(header, header2)]);
                sb.Append(separator);
            }
            sb.AppendLine();
        }
        // Write the file
        File.WriteAllText(path, sb.ToString());
    }

    private static void exportOutputPairs(List<DataStructures.OutputPair> results, IEnumerable<string> headers, string path)
    {
        // Exports the output pairs as a CSV
        // Create the string builder
        StringBuilder sb = new StringBuilder();
        // Print the data
        // Print the headers
        foreach (string header in headers)
        {
            sb.Append(header);
            sb.Append(";");
        }
        sb.AppendLine();
        foreach (DataStructures.OutputPair result in results)
        {
            sb.Append(result);
            sb.AppendLine();
        }
        // Write the file
        File.WriteAllText(path, sb.ToString());
    }

    private static Tuple<List<DataStructures.OutputPair>,string[]> importOutputPairs(string path)
    {
        // Each line is a pair and value as: pair.First_pair.Second=value
        // Read the file
        string[] lines = File.ReadAllLines(path);
        
        // Get the headers
        string[] headers = lines[0].Split(';');
        // Remove the last empty element
        headers = headers.Take(headers.Length - 1).ToArray();
        // Remove this line
        lines = lines.Skip(1).ToArray();
        
        // Create the list
        List<DataStructures.OutputPair> results = new List<DataStructures.OutputPair>();
        // Parse the lines
        foreach (string line in lines)
        {
            // Split the line
            string[] split = line.Split('=');
            // Get the pair
            string[] pair = split[0].Split('_');
            // Get the value
            float value = float.Parse(split[1]);
            // Add the pair
            results.Add(new DataStructures.OutputPair(pair[0], pair[1], value));
        }
        
        Tuple<List<DataStructures.OutputPair>,string[]> tuple = new Tuple<List<DataStructures.OutputPair>, string[]>(results, headers);
        return tuple;
    }
    
}