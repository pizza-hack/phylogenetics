using System.Runtime.Versioning;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using PhylogeneticCreator;

namespace Graph;

[SupportedOSPlatform("windows")]
public class Phylogenetic
{
    static void Main(string[] args)
    {
        Tree_XMLParser treeXmlParser = new Tree_XMLParser();
        //*/
        PhylogeneticTree.Run(treeXmlParser, false, true, 0);
        /*/
        MatrixGenerator.Run(args);
        //*/
        //JSONToString.Run();
        
        //SVG_Parser.Run(new Dictionary<string, int>());
        
        //XMLToString.Run();
    }
    
    static void Main2(string[] args)
    
    {
        var app = new GeneToMatrix(new LevenshteinGeneticDistance());
        
        app.ConvertToCorrelationMatrix(
            pathToRawGenes:ProjectSourcePath.Value + "Data/ToyTactics/Small/SmallProject.csv",
            minDiff:0,
            pathToSaveFile:ProjectSourcePath.Value + "Data/ToyTactics/Small/ProcessedMatrix.csv"
            );
        
        app.ConvertToOwnership(
            pathToRawGenes: ProjectSourcePath.Value + "Data/ToyTactics/Small/SmallProject.csv",
            pathToSave: ProjectSourcePath.Value + "Data/ToyTactics/Small/Ownership.csv"
            );
        
        // int maxRowColumns = 450;
        //
        // app.CutCorrelationMatrix(
        //     pathToCorrelationMatrix: ProjectSourcePath.Value + "Data/ToyTactics/Small/ProcessedMatrix.csv",
        //     pathToSaveFile: ProjectSourcePath.Value + "Data/ToyTactics/Small/ProcessedMatrix" + maxRowColumns + ".csv",
        //     maxRowColumns: maxRowColumns
        //     );
    }
    
    static void Main3(string[] args)
    {
        //*/
        var summary = BenchmarkRunner.Run<FindMinimunPairsInMatrix>();
        /*/
        var summary = new FindMinimunPairsInMatrix();
        summary.Base();
        summary.Immutable();
        
        summary.Dispose();
        //*/
    }
}