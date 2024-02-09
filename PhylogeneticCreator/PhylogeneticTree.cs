#define SPECIFY_Q00
#define SPECIFY_Q40
#define OWNERSHIP

using System.Diagnostics;
using System.Globalization;
using System.Text;
using Aspose.Svg;
using Aspose.Svg.Toolkit.Optimizers;
using DotNetGraph.Compilation;
using DotNetGraph.Core;
using DotNetGraph.Extensions;
using Graph;

class PhylogeneticTree
{
    private delegate float operateDictionary(float a, float b);

    /**
    public const string dataLocation = "Data/DaniTestModified.csv";
    public const string lastResult = "Data/outputPairs.txt";
    public const string outputLocation = "Data/output.dot";
    public static string graphOutputLocation = "Data/graph";
    public static bool simpleGraph = false;
    /*/
    public const string dataBasic = "Data/ToyTactics";
    public const string dataLocation = dataBasic + "/ProcessedMatrixLevenshtein.csv";
    public const string ownershipDataLocation = dataBasic + "/Ownership.csv";
    public const string lastResult = dataBasic + "/Results/outputPairs.txt";
    public const string outputLocation = dataBasic + "/Results/output.dot";
    public static string graphOutputLocation = dataBasic + "/Results/graph";
    public static string graphVizExecutable = @"C:\Program Files\Graphviz\bin\dot.exe";
    public static TreeTypes TreeProjectType = TreeTypes.AllProjects;
    public static bool processAsXMLGraph = true;
    
    public static bool newTests = true;
    public static bool newTestsGraph = true;

    public static GraphVizConfig GenericGraphVizConfig = new GraphVizConfig(true, true);
    /**/

    public static void Run(Tree_XMLParser treeXmlParser, bool exclusiveParents = true, bool compareValues = false, int threshold = 0)
    {
        bool usePredefined = false;

        // Read the file which is located in the project folder
        string projectSourcePath = ProjectSourcePath.Value;
        string[] dataLines = File.ReadAllLines(projectSourcePath + dataLocation);
        
        // Replace the commas with dots in the dataLines
        for (int i = 0; i < dataLines.Length; i++)
        {
            dataLines[i] = dataLines[i].Replace(',', '.');
        }
        
        List<DataStructures.OutputPair> outputPairs;
        string[] headers;
        
        if (!usePredefined)
        {
            if (!newTests)
            {
                string data = File.ReadAllText(projectSourcePath + dataLocation);
                DataStructures.PhylogeneticDictionary dataDictionary = setDictionary(data);
                outputPairs = new List<DataStructures.OutputPair>();
                // Original headers
                headers = dataDictionary.Headers ?? Array.Empty<string>();
                // Process the dictionary with linealMean
                DataStructures.PhylogeneticDictionary linealMeanDictionary = dictionaryIterator(dataDictionary, outputPairs, treeXmlParser, true);
                // Write the result to the output file
                exportOutputPairs(outputPairs, headers, projectSourcePath + lastResult);
            }
            else
            {
                outputPairs = new List<DataStructures.OutputPair>();
                headers = dataLines[0].Split(';');
                LoadDictionary(dataLines, ref outputPairs);
                exportOutputPairs(outputPairs, headers, projectSourcePath + lastResult);
            }
        }
        else
        {
            Tuple<List<DataStructures.OutputPair>, string[]> predefinedOutput = importOutputPairs(projectSourcePath + lastResult);
            outputPairs = predefinedOutput.Item1;
            headers = predefinedOutput.Item2;
        }

        if (!processAsXMLGraph)
        {
            treeXmlParser.Run(exclusiveParents, compareValues, threshold);

            graphOutputLocation += exclusiveParents ? "_inclusive" : "_exclusive";

            graphOutputLocation += compareValues ? "_ValuesByLevenshtein_"+threshold : "_noValuesCompared";

            //createASCIIGraph(outputPairs, headers);
            //exportASCIIGraph(outputPairs, headers, projectSourcePath + "Data/output.txt");
            graphOutputLocation += ".svg";
            createGraph(outputPairs, "phylogenetic_tree", treeXmlParser.CodeLines);
        }
        else
        {
            if (!newTestsGraph)
            {
                graphOutputLocation += ".svg";
                createSimpleGraph(outputPairs, "phylogenetic_tree");
            }
            else
            {
                graphOutputLocation += ".svg";
                creteDotNetGraph(outputPairs, headers, "phylogenetic_tree");
            }
        }
    }
    
    // NewTests
    // SetDictionaty with the new data format
    private static void LoadDictionary(IReadOnlyList<string> dataLines, ref List<DataStructures.OutputPair> returnPairs)
    {
        string[] headersNames = dataLines[0].Split(';');
        // Remove the first element of the headers as its blank
        headersNames = headersNames.Skip(1).ToArray();
        
        float[,] correlationMatrix = new float[headersNames.Length, headersNames.Length];
        
        // Load the correlation matrix
        for (int i = 1; i < dataLines.Count; i++)
        {
            string[] line = dataLines[i].Split(';');
            for (int j = 1; j < line.Length; j++)
            {
                // The decimals are separated by commas so we need to process them correctly
                correlationMatrix[i-1, j-1] = float.Parse(line[j], CultureInfo.InvariantCulture);
            }
        }

        int n = correlationMatrix.GetLength(0);
        for (int i = 0; CountComparableElements(correlationMatrix) > 1; i++)
        {
            // Step 2: Identify the pair with the highest correlation
            Tuple<int, int> maxCorrelationIndices = FindMaxCorrelation(correlationMatrix, headersNames);

            // Step 4: Create an OutputPair and store the information
            DataStructures.OutputPair outputPair = new DataStructures.OutputPair
            {
                First = headersNames[maxCorrelationIndices.Item1],
                Second = headersNames[maxCorrelationIndices.Item2],
                Value = correlationMatrix[maxCorrelationIndices.Item1, maxCorrelationIndices.Item2]
            };

            // Output the result or store it as needed
            returnPairs.Add(outputPair);
            
            // Replace the existing row and column with the combined values
            ReplaceRowAndColumn(
                correlationMatrix, 
                maxCorrelationIndices.Item1, 
                maxCorrelationIndices.Item2, 
                ref headersNames,
                i+n);
            
            // Set the second element to zero (or any sentinel value)
            correlationMatrix[maxCorrelationIndices.Item2, maxCorrelationIndices.Item1] = -1;

        }
        
        // Search for the last element in the matrix
        Tuple<int, int> lastCorrelationIndices = FindMaxCorrelation(correlationMatrix, headersNames);
        
        // Output the last element
        DataStructures.OutputPair lastOutputPair = new DataStructures.OutputPair
        {
            First = headersNames[lastCorrelationIndices.Item1],
            Second = headersNames[lastCorrelationIndices.Item2],
            Value = correlationMatrix[lastCorrelationIndices.Item1, lastCorrelationIndices.Item2]
        };
        
        returnPairs.Add(lastOutputPair);
    }
    
    static void ReplaceRowAndColumn(float[,] matrix, int rowIndex, int columnIndex, ref string[] elementNames, int iteration)
    {
        int n = matrix.GetLength(1);

        // Add a new header for the combined element substituting the first element
        /*/ 
        string combinedElementName = elementNames[rowIndex] + "^" + elementNames[columnIndex];
        elementNames[rowIndex] = combinedElementName;
        elementNames[columnIndex] = "*"+elementNames[columnIndex];
        /*/ 
        elementNames[rowIndex] = iteration.ToString();
        elementNames[columnIndex] = "*"+elementNames[columnIndex];
        //*/
        
        // Replace the new row with the combined values
        // Since its a symmetrical matrix we replace the column as well
        for (int i = 0; i < n; i++)
        {
            var mean = (matrix[rowIndex, i] + matrix[columnIndex, i]) / 2;
            matrix[rowIndex, i] = mean;
            matrix[i, rowIndex] = mean;
        }
        
        // Set the diagonal to 0
        matrix[rowIndex, rowIndex] = 0;
        
        // Set the second elements row and column to -1 (or any sentinel value)
        for (int i = 0; i < n; i++)
        {
            matrix[columnIndex, i] = -1;
            matrix[i, columnIndex] = -1;
        }
    }
    
    // Helper method to find the indices of the maximum correlation in the matrix
    static Tuple<int, int> FindMaxCorrelation(float[,] matrix, string[] headerNames)
    {
        int n = matrix.GetLength(0);
        float maxCorrelation = 2;
        Tuple<int, int> maxIndices = null;

        for (int i = 0; i < n; i++)
        {
            // If the header has been merged (contains a *) we skip it
            if (headerNames[i].Contains("*"))
            {
                continue;
            }
            
            for (int j = i + 1; j < n; j++)
            {
                if (matrix[i, j] < maxCorrelation && matrix[i, j] >= 0 && i != j)
                {
                    maxCorrelation = matrix[i, j];
                    maxIndices = Tuple.Create(i, j);
                }
            }
        }
        // Log the result
        // Console.WriteLine("Max correlation: " + maxCorrelation);
        return maxIndices;
    }
    
    static int CountComparableElements(float[,] matrix)
    {
        // Count the number of elements that are higher than 0 and divide by 2 as the matrix is symmetrical
        return matrix.Cast<float>().Count(value => value > 0) / 2;
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
    
    private static void createGraph(IReadOnlyList<DataStructures.OutputPair> results, string graphName, Dictionary<string,int> xmlLineCount)
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
                int lines = xmlLineCount[firstHeader];
                nodes.Add(firstHeader, firstHeader);
                outputToReturn.AppendLine("\t"+gf.Node(firstHeader, "-1", new Dictionary<string, object>(), firstHeader + " (" + lines + ")", "octagon", "filled", "skyblue",firstHeader + " (" + lines + ")"));
            }
            if (!nodes.ContainsKey(secondHeader))
            {
                int lines = xmlLineCount[secondHeader];
                nodes.Add(secondHeader, secondHeader);
                outputToReturn.AppendLine("\t"+gf.Node(secondHeader, "-1", new Dictionary<string, object>(),secondHeader + " (" + lines + ")", "octagon", "filled", "skyblue",secondHeader + " (" + lines + ")"));
            }

            nodes.Add(newHeader, newHeader);
            string color = "#006400";
            
            // Retrieve the number from the Dictionary
            int value = xmlLineCount[newHeader.Replace('_','^')];
            
            //outputToReturn.AppendLine("\t"+gf.Node(newHeader,i.ToString(), "diamond", "rounded", color,currentResult.Value.ToString(CultureInfo.InvariantCulture),color));
            outputToReturn.AppendLine("\t"+gf.Node(newHeader, "-1", new Dictionary<string, object>(), value.ToString(), "diamond", "rounded", color,"",color));

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

    
    private static void creteDotNetGraph(IReadOnlyList<DataStructures.OutputPair> results, string[] headers, string graphName)
    {
        Graph.GraphStructures gf = new Graph.GraphStructures();

#if OWNERSHIP
        GenericGraphVizConfig.baseNodeConfig.Options.Add("ownership", ownershipDataLocation);
        // Read the first line of the ownership file to get the number of owners
        string[] ownershipLines = File.ReadLines(ProjectSourcePath.Value + ownershipDataLocation).ToArray()[0].Split(';');
        // Remove the first element as its blank
        ownershipLines = ownershipLines.Skip(1).ToArray();
        // Asign a color to each owner
        Dictionary<string,string> ownershipColors = new Dictionary<string, string>();
        // The colors to choose from are:
        // aqua
        // aquamarine
        // bisque
        // darkorchid
        // gray
        // hotpink
        // cadetblue
        // chocolate
        // coral
        // cornflowerblue
        // crimson
        // greenyellow
        // gold
        // orchid
        // peru
        // red
        // orange
        // teal
        // yellow
        for (int i = 0; i < ownershipLines.Length; i++)
        {
            string owner = ownershipLines[i];
            string color = "";
            switch (i)
            {
                case 0:
                    color = "aqua";
                    break;
                case 1:
                    color = "aquamarine";
                    break;
                case 2:
                    color = "bisque";
                    break;
                case 3:
                    color = "darkorchid";
                    break;
                case 4:
                    color = "gray";
                    break;
                case 5:
                    color = "hotpink";
                    break;
                case 6:
                    color = "cadetblue";
                    break;
                case 7:
                    color = "chocolate";
                    break;
                case 8:
                    color = "coral";
                    break;
                case 9:
                    color = "cornflowerblue";
                    break;
                case 10:
                    color = "crimson";
                    break;
                case 11:
                    color = "greenyellow";
                    break;
                case 12:
                    color = "gold";
                    break;
                case 13:
                    color = "orchid";
                    break;
                case 14:
                    color = "peru";
                    break;
                case 15:
                    color = "red";
                    break;
                case 16:
                    color = "orange";
                    break;
                case 17:
                    color = "teal";
                    break;
                case 18:
                    color = "yellow";
                    break;
                default:
                    color = "white";
                    break;
            }
            ownershipColors.Add(owner, color);
        }
        
        GenericGraphVizConfig.baseNodeConfig.Options.Add("ownershipColors", ownershipColors);
#endif
        // ProjectSourcePath.Value + outputLocation outputLocation
        using (StreamWriter outfile = new(ProjectSourcePath.Value + outputLocation)) 
        {
            outfile.Write(gf.Init(graphName));

            outfile.Write(gf.Parameters(
                "dot",
                "Phylogenetic Tree from: " + dataLocation.Split('/').Last(),
                "major",
                "shortpath",
                "",
                "fill",
                "portrait"
            ));
            
            // For each header we add a node
            GraphVizNodeConfig currentConfig = GenericGraphVizConfig.baseNodeConfig;
            // We also save the nodes id to later relate to them without having to search for them
            Dictionary<string, string> nodes = new Dictionary<string, string>();
            headers = headers.Where(x => !string.IsNullOrEmpty(x)).ToArray();
            for (int i = 0; i < headers.Length; i++)
            {
                outfile.WriteLine("#" + i);
                string header = headers[i];
                // We add the nodes to the nodes dictionary if they are not already there
                outfile.WriteLine("\t"+gf.Node(
                    name: header,
                    id: (i).ToString(),
                    options: currentConfig.Options,
                    label: "",
                    shape: currentConfig.Shape,
                    style: currentConfig.Style,
                    color: currentConfig.Color,
                    fontcolor: currentConfig.FontColor)
                );
                nodes.Add(header, (i).ToString());
            }
            
            // We process the results
            currentConfig = GenericGraphVizConfig.linkNodeConfig;
            for (int i = 0; i < results.Count-1; i++)
            {
                outfile.WriteLine("#" + i);
                DataStructures.OutputPair currentResult = results[i];
                string firstHeader = currentResult.First;
                string secondHeader = currentResult.Second;
                
                // Inside each header, the combined headers are separated by a ^, we get the concatenatiopn of all of them in id
                string id = getIdFromHeader(firstHeader, ref nodes);
                string id2 = getIdFromHeader(secondHeader, ref nodes);
                
                // The new id is the concatenation of the ids of the headers
                string newId = (nodes.Count + i).ToString() ;
                
                outfile.WriteLine("\t"+gf.Node(
                    name: i.ToString(),
                    id: newId,
                    options: currentConfig.Options,
                    label: "",
                    tooltip: currentResult.Value.ToString(CultureInfo.InvariantCulture),
                    shape: currentConfig.Shape,
                    style: currentConfig.Style,
                    color: currentConfig.Color,
                    fontcolor: currentConfig.FontColor)
                );
                
                outfile.Write("\t");
                // We add the link between the first header and the new header
                outfile.WriteLine(gf.Edge(id, newId, "forward"));
                outfile.Write("\t");
                // We add the link between the second header and the new header
                outfile.WriteLine(gf.Edge(id2, newId, "forward"));
            }
            
            currentConfig = GenericGraphVizConfig.finalNodeConfig;
            outfile.WriteLine("#" + (results.Count-1));
            DataStructures.OutputPair lastResult = results[^1];
            string firstHeaderLast = lastResult.First;
            string secondHeaderLast = lastResult.Second;
            string idLast = getIdFromHeader(firstHeaderLast, ref nodes);
            string id2Last = getIdFromHeader(secondHeaderLast, ref nodes);
            string newIdLast = idLast + id2Last;
            outfile.WriteLine("\t"+gf.Node(
                name: (results.Count-1).ToString(),
                id: newIdLast,
                options: currentConfig.Options,
                label: "",
                tooltip: lastResult.Value.ToString(CultureInfo.InvariantCulture),
                shape: currentConfig.Shape,
                style: currentConfig.Style,
                color: currentConfig.Color,
                fontcolor: currentConfig.FontColor)
            );
            
            outfile.Write("\t");
            outfile.WriteLine(gf.Edge(idLast, newIdLast, "forward"));
            outfile.Write("\t");
            outfile.WriteLine(gf.Edge(id2Last, newIdLast, "forward"));

            outfile.WriteLine();

            outfile.Write(gf.End());
        }

        // Create the image from the dot file as a svg
        string dotPath = graphVizExecutable;
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
            UseShellExecute = true,
            RedirectStandardOutput = false,
            CreateNoWindow = false
        };
        
        Console.WriteLine(command);
        Process process = new Process();
        process.StartInfo = startInfo;
        process.Start();
        process.WaitForExit();
    }
    
    private static string getIdFromHeader(string header, ref Dictionary<string, string> nodes)
    {
        string[] headers = header.Split('^');
        string id = "";
        
        //If the header is already a number we return it
        if (int.TryParse(header, out _))
        {
            return header;
        }
        
        if (headers.Length == 1)
        {
            id = nodes[header];
        }
        else
        {
            id = nodes[headers[0]];
            for (var index = 1; index < headers.Length; index++)
            {
                var h = headers[index];
                id += nodes[h];
            }
        }
        
        return id;
    }

    private static int invertOrderInteger(int a)
    {
        // We invert the order of the integer
        string aString = a.ToString();
        char[] aArray = aString.ToCharArray();
        Array.Reverse(aArray);
        string invertedString = new string(aArray);
        return int.Parse(invertedString);
    }
    
    private static void createSimpleGraph(IReadOnlyList<DataStructures.OutputPair> results, string graphName)
    {
        Graph.GraphStructures gf = new Graph.GraphStructures();

#if OWNERSHIP
        GenericGraphVizConfig.baseNodeConfig.Options.Add("ownership", ownershipDataLocation);
        // Read the first line of the ownership file to get the number of owners
        string[] ownershipLines = File.ReadLines(ProjectSourcePath.Value + ownershipDataLocation).ToArray()[0].Split(';');
        // Remove the first element as its blank
        ownershipLines = ownershipLines.Skip(1).ToArray();
        // Asign a color to each owner
        Dictionary<string,string> ownershipColors = new Dictionary<string, string>();
        // The colors to choose from are:
        // aqua
        // aquamarine
        // bisque
        // darkorchid
        // gray
        // hotpink
        // cadetblue
        // chocolate
        // coral
        // cornflowerblue
        // crimson
        // greenyellow
        // gold
        // orchid
        // peru
        // red
        // orange
        // teal
        // yellow
        for (int i = 0; i < ownershipLines.Length; i++)
        {
            string owner = ownershipLines[i];
            string color = "";
            switch (i)
            {
                case 0:
                    color = "aqua";
                    break;
                case 1:
                    color = "aquamarine";
                    break;
                case 2:
                    color = "bisque";
                    break;
                case 3:
                    color = "darkorchid";
                    break;
                case 4:
                    color = "gray";
                    break;
                case 5:
                    color = "hotpink";
                    break;
                case 6:
                    color = "cadetblue";
                    break;
                case 7:
                    color = "chocolate";
                    break;
                case 8:
                    color = "coral";
                    break;
                case 9:
                    color = "cornflowerblue";
                    break;
                case 10:
                    color = "crimson";
                    break;
                case 11:
                    color = "greenyellow";
                    break;
                case 12:
                    color = "gold";
                    break;
                case 13:
                    color = "orchid";
                    break;
                case 14:
                    color = "peru";
                    break;
                case 15:
                    color = "red";
                    break;
                case 16:
                    color = "orange";
                    break;
                case 17:
                    color = "teal";
                    break;
                case 18:
                    color = "yellow";
                    break;
                default:
                    color = "white";
                    break;
            }
            ownershipColors.Add(owner, color);
        }
        
        GenericGraphVizConfig.baseNodeConfig.Options.Add("ownershipColors", ownershipColors);
#endif
        
        // Create a string output to put into the file
        StringBuilder outputToReturn = new StringBuilder();
        
        outputToReturn.Append(gf.Init(graphName));
        
        // Get the last element of the string (path so last element is the name of the file)
        string filename = dataLocation.Split('/').Last();
        
        outputToReturn.Append(gf.Parameters(
            "dot",
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
            GraphVizNodeConfig currentConfig = GenericGraphVizConfig.baseNodeConfig;
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

            if (containsSpecialChar2 && containsSpecialChar1)
            {
                currentConfig = GenericGraphVizConfig.linkNodeConfig;
            }
            
            string newHeader = firstHeader + "_" + secondHeader;

            // We add the nodes to the nodes dictionary if they are not already there
            if (!nodes.ContainsKey(firstHeader))
            {
#if SPECIFY_Q40
                if (firstHeader.Contains("_Q40") && firstHeader.Length <= 7)
                {
                    currentConfig.Color = "#D98DDB";
                }
#endif
#if SPECIFY_Q00
                if (firstHeader.Contains("Q00") && firstHeader.Length <= 3)
                {
                    currentConfig.Color = "#DCFF2F";
                }
#endif
                
                nodes.Add(firstHeader, firstHeader);
                outputToReturn.AppendLine("\t"+gf.Node(
                    name: firstHeader,
                    id: "-1",
                    options: currentConfig.Options,
                    label: "",
                    shape: currentConfig.Shape,
                    style: currentConfig.Style,
                    color: currentConfig.Color,
                    fontcolor: currentConfig.FontColor)
                );
            }
            
            if (!nodes.ContainsKey(secondHeader))
            {
#if SPECIFY_Q40
                if (secondHeader.Contains("_Q40") && secondHeader.Length <= 7)
                {
                    currentConfig.Color = "#D98DDB";
                }
#endif
#if SPECIFY_Q00
                if (secondHeader.Contains("Q00") && secondHeader.Length <= 3)
                {
                    currentConfig.Color = "#DCFF2F";
                }
#endif
                
                nodes.Add(secondHeader, secondHeader);
                outputToReturn.AppendLine("\t"+gf.Node(
                    name: secondHeader,
                    id: "-1",
                    options: currentConfig.Options,
                    label: "",
                    shape: currentConfig.Shape,
                    style: currentConfig.Style,
                    color: currentConfig.Color,
                    fontcolor: currentConfig.FontColor)
                );
            }

            nodes.Add(newHeader, newHeader);
            if (results.Count - i <= 1)
            {
                currentConfig = GenericGraphVizConfig.finalNodeConfig;
                outputToReturn.AppendLine("\t" + gf.Node(
                    name: newHeader,
                    id: "-1",
                    options: currentConfig.Options,
                    label: i.ToString(),
                    shape: currentConfig.Shape,
                    style: currentConfig.Style,
                    color: currentConfig.Color,
                    tooltip: currentResult.Value.ToString(CultureInfo.InvariantCulture),
                    fontcolor: currentConfig.FontColor));
            }
            else
            {
                currentConfig = GenericGraphVizConfig.linkNodeConfig;
                outputToReturn.AppendLine("\t"+gf.Node(
                    name: newHeader,
                    id:"-1",
                    options: currentConfig.Options,
                    label: i.ToString(),
                    shape: currentConfig.Shape,
                    style: currentConfig.Style,
                    color: currentConfig.Color,
                    tooltip: currentResult.Value.ToString(CultureInfo.InvariantCulture),
                    fontcolor: currentConfig.FontColor));
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
        
        // Add the legend
        outputToReturn.AppendLine(GenericGraphVizConfig.LegendBuilder(TreeProjectType, gf,
            new("Nodes", GenericGraphVizConfig.baseNodeConfig.Color),
            // new("Q40", "#D98DDB"),
            new("Links", GenericGraphVizConfig.linkNodeConfig.Color),
            new("Final", GenericGraphVizConfig.finalNodeConfig.Color))
        );
        
        outputToReturn.Append(gf.End());
        
        // We write the file
        File.WriteAllText(ProjectSourcePath.Value + outputLocation, outputToReturn.ToString());

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

    private static DataStructures.PhylogeneticDictionary dictionaryIterator(DataStructures.PhylogeneticDictionary dict, ICollection<DataStructures.OutputPair> Output, 
        Tree_XMLParser treeXmlParser ,bool outputLayers = true)
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
            dict = properProcessDic(dict, Mean, Output, layerDepth, out Tuple<string, string> processedPair);
            
            treeXmlParser.AddXMLToParse(layerDepth, processedPair);
            
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
        
        // Add the pair to the tree
        treeXmlParser.AddXMLToParse(layerDepth, new Tuple<string, string>(lastPair.First, lastPair.Second));

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
    
    private static DataStructures.PhylogeneticDictionary properProcessDic(DataStructures.PhylogeneticDictionary dict, operateDictionary operation, ICollection<DataStructures.OutputPair> outputPairs, int layer
        , out Tuple<string,string> processed)
    {
        processed = new Tuple<string, string>("","");
        
        // Find the smallest value in the dictionary that isnt with itself
        DataStructures.Pair[] smallestPairs = dict.getMinPairs();
        DataStructures.Pair smallestPair = smallestPairs[0];
        DataStructures.PhylogeneticDictionary dataDictionary = dict;

        // The smallest pairs will be combined into a new unique pair (Pair1, Pair2) -> (Pair1 + Pair2)
        // that will be a "new column" to compare the rest of the columns with

        //Console.WriteLine("Dictionary size: " + dataDictionary.Count());
        if (dataDictionary.Count() <= 4)
        {
            outputPairs.Add(new DataStructures.OutputPair(smallestPair.First, smallestPair.Second, dict.getAllValues().Min()));
            return dataDictionary;
        }
        
        //Console.WriteLine(smallestPair.First + "^" + smallestPair.Second);
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
        
        processed = new Tuple<string, string>(firstKey, secondKey);
        
        // Print the new dictionary
         //printDictionaryAsTable(tempDict,-1);

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
        // Get the first line (until the first \n character)
        string firstLine = data.Split("\n")[0];
        string[] columnNames = firstLine.Split(';');
        // Clean the column names and remove the first empty column
        columnNames = columnNames.Select(x => x.Trim()).Skip(1).ToArray();
        
        // Get the data
        string[] lines = data.Split("\n").Skip(1).ToArray();
        // Clean the data and split it removing the separator
        lines = lines.Select(x => x.Trim()).ToArray();
        string[][] dataLines = lines.Select(x => x.Split(';')).ToArray();
        // Trim the data and remove the first column
        dataLines = dataLines.Select(x => x.Select(y => y.Trim()).Skip(1).ToArray()).ToArray();
        // Parse as float
        float[][] dataAsFloat = dataLines.Select(x => x.Select(y =>
        {
            return float.Parse(y, CultureInfo.InvariantCulture);
        }).ToArray()).ToArray();

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

class GraphVizConfig
{
    public bool legend = true;
    public bool useColorLabels = true;

    public GraphVizNodeConfig baseNodeConfig =
        new GraphVizNodeConfig("plaintext", "filled", "#87CEEB", "#000000", .3f, .3f);
    public GraphVizNodeConfig linkNodeConfig = new GraphVizNodeConfig("diamond", "filled", "#006400", "#006400", .3f, .3f);
    public GraphVizNodeConfig finalNodeConfig = new GraphVizNodeConfig("diamond", "filled", "#8B0000", "#8B0000", .3f, .3f);
    
    public GraphVizConfig(bool legend, bool useColorLabels)
    {
        this.legend = legend;
        this.useColorLabels = useColorLabels;
    }
    
    // Change the baseNodeConfig
    public void setBaseNodeConfig(GraphVizNodeConfig config)
    {
        baseNodeConfig = config;
    }
    
    // Change the linkNodeConfig
    public void setLinkNodeConfig(GraphVizNodeConfig config)
    {
        linkNodeConfig = config;
    }
    
    // Change the finalNodeConfig
    public void setFinalNodeConfig(GraphVizNodeConfig config)
    {
        finalNodeConfig = config;
    }
    
    public string LegendBuilder(TreeTypes type, GraphStructures gf, params Tuple<string, string>[] elements)
    {
        if (legend)
        {
            switch (type)
            {
                case TreeTypes.Projects :
                    return gf.Legend(
                        margin1:"2.5",
                        margin2: "1.25",
                        fontSize: 14,
                        border: 0,
                        cellBorder: 1,
                        cellSpacing: 0,
                        cellPadding: 4,
                        label: "Legend",
                        elements: elements
                    );
                case TreeTypes.ProjectsWithQ40 :
                    return gf.Legend(
                        margin1:"3",
                        margin2: "0.5",
                        fontSize: 14,
                        border: 0,
                        cellBorder: 1,
                        cellSpacing: 0,
                        cellPadding: 4,
                        label: "Legend",
                        elements: elements
                    );
                case TreeTypes.AllProjects :
                    return gf.Legend(
                        margin1:"0",
                        margin2: "0",
                        fontSize: 14,
                        border: 0,
                        cellBorder: 1,
                        cellSpacing: 0,
                        cellPadding: 4,
                        label: "Legend",
                        elements: elements
                    );
                case TreeTypes.defaultTree:
                default:
                    return gf.Legend(
                        margin1:"0",
                        margin2: "0",
                        fontSize: 12,
                        border: 0,
                        cellBorder: 1,
                        cellSpacing: 0,
                        cellPadding: 4,
                        label: "Legend",
                        elements: elements
                    );
                
            }
        }

        return "";
    }
    
}

/**
 * Class that contains the configuration for a GraphViz node 
 * - shape: The shape of the node
 * - style: The style of the node
 * - color: The color of the node
 * - fontColor: The color of the font
 * - width: The width of the node
 * - height: The height of the node
 */
struct GraphVizNodeConfig
{
    // Shape of the node
    public string Shape = "plaintext";
    public string Style = "rounded";
    public string Color = "#87CEEB";
    public string FontColor = "#000000";
    public Dictionary<string, object> Options = null;
    float width = .3f;
    float height = .3f;
    
    public GraphVizNodeConfig(string shape, string style, string color, string fontColor, float width, float height, Dictionary<string, object> options = null)
    {
        this.Shape = shape;
        this.Style = style;
        this.Color = color;
        this.FontColor = fontColor;
        this.width = width;
        this.height = height;
        this.Options = options ?? new Dictionary<string, object>();
    }
}

enum TreeTypes
{
    Projects,
    ProjectsWithQ40,
    AllProjects,
    defaultTree
}