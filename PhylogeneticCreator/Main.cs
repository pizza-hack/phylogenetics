using System.Runtime.Versioning;

namespace Graph;

[SupportedOSPlatform("windows")]
public class Phylogenetic
{
    static void Main(string[] args)
    {
        //*/
        //PhylogeneticTree.Run(args);
        /*/
        MatrixGenerator.Run(args);
        //*/
        //JSONToString.Run();
        
        //SVG_Parser.Run(new Dictionary<string, int>());
        
        XMLToString.Run(args);
    }
}