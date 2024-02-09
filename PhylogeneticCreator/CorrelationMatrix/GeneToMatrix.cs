using System.Globalization;
using PhylogeneticCreator;
// ReSharper disable UnusedMember.Local
// ReSharper disable ConvertToUsingDeclaration

class GeneToMatrix
{
    // GeneticDistance intance choosen
    private readonly IGeneticDistance geneticDistanceBetween;
    private readonly string name;

    public GeneToMatrix(IGeneticDistance? geneticDistance = null)
    {
        geneticDistanceBetween = geneticDistance ?? new BaseGeneticDistance();
        name = geneticDistanceBetween.Name;
    }
    
    public void ConvertToCorrelationMatrix(string pathToRawGenes, int minDiff, string pathToSaveFile)
    {
        // Read the file
        string[] lines = File.ReadAllLines(pathToRawGenes);
        
        lines = lines.Skip(1).ToArray();
        List<Gene> genes = new();
        List<string> prefabNames = new();
        GetGenesAndHeaders(lines, ref genes, ref prefabNames);
        
        //EnsureUniqueNames(ref genes);
        
        float[,] matrix = new float[prefabNames.Count, prefabNames.Count];
        
        ProcessGeneticDistanceMatrix(ref matrix, genes, minDiff);
        
        string pathToSaveFileWithDistance = pathToSaveFile.Replace(".csv", name + ".csv");
        
        if (SaveMatrixToOutputFile(pathToSaveFileWithDistance, prefabNames.ToArray(), matrix))
        {
            Console.WriteLine("Matrix saved to " + pathToSaveFileWithDistance);
        }
        else
        {
            Console.WriteLine("Error saving the matrix to " + pathToSaveFileWithDistance);
        }
        
    }
    
    private void ProcessGeneticDistanceMatrix(ref float[,] matrix, IReadOnlyList<Gene> genes, int minDiff)
    {
        int numberOfGenes = genes.Count;
        
        //Preprocess diagonal (20% faster than branching in the loop)
        for (int i = 0; i < numberOfGenes; i++)
        {
            matrix[i, i] = 0;
        }
        
        for (int i = 0; i < numberOfGenes; i++)
        {
            for (int j = i+1; j < numberOfGenes; j++)
            {
                float difference = geneticDistanceBetween.CalculateDistance(genes[i], genes[j], minDiff);

                matrix[i, j] = difference;
                matrix[j, i] = difference;
            }
        }
    }
    
    /// <summary>
    /// We are going to output the matrix to a CSV
    /// The structure of the CSV is:
    /// 1st row: prefab names
    /// 1st column: prefab names
    /// The rest of the columns: 0 - 1 times the prefab is similar to the other prefab
    /// </summary>
    /// <param name="pathToSaveFile"> Path to save the file </param>
    /// <param name="prefabNames"> List of prefab names (optimization) </param>
    /// <param name="matrix"> Matrix with the genetic distances </param>
    /// <returns></returns>
    private static bool SaveMatrixToOutputFile(string pathToSaveFile, IReadOnlyList<string> prefabNames, float[,] matrix)
    {
        const string separator = ";";
        int numberOfGenes = prefabNames.Count;

        try
        {
            using (StreamWriter outfile = new(pathToSaveFile))
            {
                string csv = "";

                // Add the first row
                foreach (var prefabName in prefabNames)
                {
                    csv += separator + prefabName;
                }

                outfile.Write(csv);

                // Add the rest of the rows
                for (int i = 0; i < numberOfGenes; i++)
                {
                    // Substitute spaces with underscores and the extension with nothing
                    var name = prefabNames[i];

                    outfile.Write("\n" + name);
                    for (int j = 0; j < numberOfGenes; j++)
                    {
                        // We make sure the decimal separator is passed as "." with all the decimal values without trimming them
                        outfile.Write(separator + matrix[i, j].ToString(CultureInfo.InvariantCulture));
                    }
                }
            }
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }
    
    private void EnsureUniqueNames(ref List<Gene> genes)
    {
        // Ensure the function names are unique
        for (int i = 0; i < genes.Count; i++)
        {
            for (int j = i + 1; j < genes.Count; j++)
            {
                // If the names are different, we continue
                if (genes[i].Name != genes[j].Name) continue;
                
                // Calculate the gene merging the two genes
                List<int> values = new();
                for (int k = 0; k < genes[i].Values.Count; k++)
                {
                    values.Add(genes[i].Values[k] + genes[j].Values[k]);
                }

                // We remove the j gene and update the i gene
                genes.RemoveAt(j);
                genes[i] = new Gene(genes[i].Name, values);

                // Move the indexes back so we don't skip a gene
                j--;
            }
        }
    }
    
    /// <summary>
    /// The structure of the CSV is:
    /// 1st row: component names
    /// 1st column: prefab names
    /// The rest of the columns: 0 - n times the component is used in the prefab
    /// </summary>
    /// <param name="lines"> Lines of the CSV (WITHOUT the header) </param>
    /// <param name="genes"> REF List of genes to fill </param>
    /// <param name="prefabNames"> REF List of prefab names to fill </param>
    private static void GetGenesAndHeaders(IEnumerable<string> lines,ref List<Gene> genes,ref List<string> prefabNames)
    {
        foreach (var line in lines)
        {
            // If the line is empty, skip it
            if (line == "")
            {
                continue;
            }
            // We split the line by the separator
            string[] splitLine = line.Split(';');

            // We create a list of values, one per column
            List<int> values = new();
            for (int i = 1; i < splitLine.Length - 1; i++)
            {
                values.Add(int.Parse(splitLine[i]));
            }

            // We create a Gene with the name of the prefab and the values
            //Make sure the entry is unique, if not, add a number to the end
            string prefabName = splitLine[0];
            prefabName = prefabName
                .Replace(" ", "")
                .Replace(".", "")
                .Replace("_", "")
                .Replace("-", "")
                .Replace("(", "")
                .Replace(")", "")
                .Replace(".prefab", "")
                .Replace("prefab", "");
            int count = 1;
            while (prefabNames.Contains(prefabName))
            {
                prefabName = splitLine[0] + count;
                count++;
            }
            genes.Add(new Gene(prefabName, values));
            prefabNames.Add(prefabName);
        }
    }

    public void ConvertToOwnership(string pathToRawGenes, string pathToSave)
    {
        // Read the file
        string[] lines = File.ReadAllLines(pathToRawGenes);
        
        // The structure of the CSV is:
        // 1st row: component names
        // 1st column: prefab names
        // The rest of the columns: 0 - n times the component is used in the prefab
        
        // We have to substitute any value that is higher than 1 with 1
        using (StreamWriter outfile = new(pathToSave))
        {
            outfile.Write(lines[0]);
            
            List<string> prefabNames = new();
            for (int i = 1; i < lines.Length; i++)
            {
                string[] splitLine = lines[i].Split(';');
                
                // Ensure the function names are unique
                string prefabName = splitLine[0];
                prefabName = prefabName
                    .Replace(" ", "")
                    .Replace(".", "")
                    .Replace("_", "")
                    .Replace("-", "")
                    .Replace("(", "")
                    .Replace(")", "")
                    .Replace(".prefab", "")
                    .Replace("prefab", "");
                int count = 1;
                while (prefabNames.Contains(prefabName))
                {
                    prefabName = splitLine[0] + count;
                    count++;
                }
                outfile.Write("\n" + prefabName);
                prefabNames.Add(prefabName);
                
                for (int j = 1; j < splitLine.Length; j++)
                {
                    if (splitLine[j] != "0")
                    {
                        outfile.Write(";1");
                    }
                    else
                    {
                        outfile.Write(";0");
                    }
                }
            }
        }
    }
    
    public void CutCorrelationMatrix(string pathToCorrelationMatrix, string pathToSaveFile, int maxRowColumns)
    {
        // Read the file
        string[] lines = File.ReadAllLines(pathToCorrelationMatrix);
        
        // The structure of the CSV is:
        // 1st row: prefab names
        // 1st column: prefab names
        // The rest of the columns: 0 - 1 times the prefab is similar to the other prefab
        
        // We are going to output the matrix to a CSV
        // The structure of the CSV is:
        // 1st row: prefab names
        // 1st column: prefab names
        // The rest of the columns: 0 - 1 times the prefab is similar to the other prefab
        string separator = ";";
       
        // Output stream
        using (StreamWriter outfile = new(pathToSaveFile))
        {
            string csv = "";
            
            // Add the first row
            string[] splitLine = lines[0].Split(';');
            csv += splitLine[0];
            for (int i = 1; i < maxRowColumns; i++)
            {
                csv += separator + splitLine[i];
            }
            outfile.Write(csv);
            
            // Add the rest of the rows
            for (int i = 1; i < maxRowColumns; i++)
            {
                splitLine = lines[i].Split(';');
                outfile.Write("\n" + splitLine[0]);
                for (int j = 1; j < maxRowColumns; j++)
                {
                    // We make sure the decimal separator is passed as "." with all the decimal values without trimming them
                    outfile.Write(separator + splitLine[j]);
                }
            }
        }
        
    }

    
}

public class Gene
{
    public string Name;
    public List<int> Values { get; }

    public Gene(string name, List<int> values)
    {
        Name = name;
        Values = values;
    }
    
    public string ConcatenateValues()
    {
        string result = "";
        foreach (var value in Values)
        {
            result += value;
        }
        return result;
    }
}