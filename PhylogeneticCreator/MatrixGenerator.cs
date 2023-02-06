using System.Runtime.Versioning;
using csdot.Attributes.DataTypes;
using System.Drawing;
using static DataStructures;
using Color = System.Drawing.Color;

namespace Graph;

[SupportedOSPlatform("windows")]
public class MatrixGenerator
{
    public const string dataLocation = "Data/Genes/daniGenes.txt";
    public static void Run(string[] args)
    {
        
        string projectSourcePath = ProjectSourcePath.Value;
        string data = File.ReadAllText(projectSourcePath + dataLocation);

        string[] lines = data.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

        // Create the genetic dicctionary
        var geneticDictionary = new GeneticDictionary(GeneticDictionary.TYPE_SIMPLE_DIFFERENCES);

        List<string> geneNames = new List<string>();
        // Add the genes to the genetic dictionary
        foreach (string line in lines)
        {
            // Get the gene name (until the first space) and also remove the first space
            string geneName = line.Substring(0, line.IndexOf('"')).Trim();
            geneNames.Add(geneName);
            // Get the gene sequence (after the first space)
            string geneSequence = line.Substring(line.IndexOf('"')).Trim();
            // Add the gene to the genetic dictionary
            geneticDictionary.AddGene(geneName, geneSequence);
        }
        
        // Console.WriteLine("Value Vermis_Teuthus: " + geneticDictionary.CalculateValue("Vermis", "Teuthus"));
        
        // Remove all files in the Process folder
        string processFolder = ProjectSourcePath.Value + "Data/Genes/Process/";
        foreach (string file in Directory.GetFiles(processFolder))
        {
            File.Delete(file);
        }

        // While we have more than a single gene
        int iteration = 0;
        string newGene = "";
        while (geneticDictionary.Count > 1)
        {
            var dict = new PhylogeneticDictionary();
            foreach (string geneName in geneNames)
            {
                foreach (string otherGeneName in geneNames)
                {
                    dict.Add(geneName, otherGeneName, geneticDictionary.CalculateValue(geneName, otherGeneName));
                    //Console.WriteLine("Value " + geneName + "_" + otherGeneName + ": " + geneticDictionary.CalculateValue(geneName, otherGeneName));
                }
            }
            
            // Generate the correlation matrix 
            // Print all the gene labels
            string matrix = "";
            foreach (string geneName in geneNames)
            {
                matrix += ";" + geneName;
            }
            matrix += Environment.NewLine;
        
            // Print one line for each label followed by the value of the correlation
            foreach (string geneName in geneNames)
            {
                matrix += geneName;
                foreach (string otherGeneName in geneNames)
                {
                    matrix += ";" + geneticDictionary.CalculateValue(geneName, otherGeneName);
                }
                matrix += Environment.NewLine;
            }
            
            // Save the matrix to a file
            File.WriteAllText(ProjectSourcePath.Value + "Data/Genes/Process/Matrix" + iteration + ".csv", matrix);

            if (iteration == 0)
            {
                BarcodeGenerator genr = new BarcodeGenerator(2, 100, Color.Black, Color.White);
                // Get the gene value with label newGene, remove the spaces and "
                char[] inversableChars = new char[] {'1'};
                foreach (string geneName in geneNames)
                {
                    string genee = geneticDictionary.GetGen(geneName).Replace(" ", "").Replace("\"", "");
                    genr.CreateBarcode(genee, inversableChars, ProjectSourcePath.Value + "Data/Genes/Process/Barcode_" + geneName + ".png");
                }
                
            }
            // get the minimum value inside the dictionary and the pair of genes that have that value
            var min = dict.getMinPairs()[0];
        
            // create the new gene
            newGene = geneticDictionary.CalculateNewGene(min.First, min.Second, true).Item1;
            
            // update the geneNames list
            geneNames.Remove(min.First);
            geneNames.Remove(min.Second);
            geneNames.Add(newGene);
            
            geneticDictionary.PrintGenes(ProjectSourcePath.Value + "Data/Genes/Process/GeneProcessed" + iteration + ".txt");

            iteration++;
        }
        
        //Move the last gene to the final folder overwriting the previous one
        File.Copy(ProjectSourcePath.Value + "Data/Genes/Process/GeneProcessed" + (iteration - 1) + ".txt", ProjectSourcePath.Value + "Data/Genes/GeneFinal.txt", true);
        
        BarcodeGenerator generator = new BarcodeGenerator(2, 100, Color.Red, Color.White);
        // Get the gene value with label newGene, remove the spaces and "
        string gene = geneticDictionary.GetGen(newGene).Replace(" ", "").Replace("\"", "");
        generator.CreateBarcode(gene, new char[] {'*'}, ProjectSourcePath.Value + "Data/Genes/Barcode.png");
        
    }

    public void AddSpacesToGenes(IEnumerable<string> lines)
    {
        List<string> newLines = new List<string>();
        foreach (string line in lines)
        {
            // data starts with an " at the beginning and an " at the end
            // we need to add an space every 16x4 characters to make it easier to read
            
            // separate the name from the data (data is between the first and second " )
            string[] split = line.Split('"');
            string name = split[0];
            string gen = split[1];
            string newGene = "";
            for (int i = 0; i < gen.Length; i++)
            {
                if (i % (16*4) == 0 && i != 0)
                {
                    newGene += " ";
                }
                newGene += gen[i];
            }
            newLines.Add(name + '"' + newGene + '"');
        }
        
        string newData = string.Join(Environment.NewLine, newLines);
        Console.WriteLine(newData);
        File.WriteAllText(ProjectSourcePath.Value + dataLocation, newData);
    }
}