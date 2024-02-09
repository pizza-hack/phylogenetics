// Daniel Muñoz
// 09-02-2024
// PhylogeneticCreator - GeneticDistance.cs

namespace PhylogeneticCreator;

internal interface IGeneticDistance
{
    float CalculateDistance(Gene gene1, Gene gene2, int minDiff);
    
    string Name { get; }
}

public class BaseGeneticDistance : IGeneticDistance
{
    public string Name => "Base";
    public float CalculateDistance(Gene gene1, Gene gene2, int minDiff = 0)
    {
        float difference = 0;
        int numGenes = gene1.Values.Count;
        for (int k = 0; k < numGenes; k++)
        {
            // If the difference is more than minDifference, add 1
            if (Math.Abs(gene1.Values[k] - gene2.Values[k]) > minDiff)
            {
                difference++;
            }
        }
        
        return difference / numGenes;
    }
}

public class LevenshteinGeneticDistance : IGeneticDistance
{
    public string Name => "Levenshtein";
    public float CalculateDistance(Gene gene1, Gene gene2, int minDiff = 0)
    {
        string str1 = gene1.ConcatenateValues();
        string str2 = gene2.ConcatenateValues();
        
        int[,] distance = new int[str1.Length + 1, str2.Length + 1];

        // Initialize the first row and column
        for (int i = 0; i <= str1.Length; i++) {
            distance[i, 0] = i;
        }

        for (int j = 0; j <= str2.Length; j++) {
            distance[0, j] = j;
        }

        // Calculate Levenshtein distance
        for (int i = 1; i <= str1.Length; i++) {
            for (int j = 1; j <= str2.Length; j++) {
                int cost = (str1[i - 1] == str2[j - 1]) ? 0 : 1;
                distance[i, j] = Math.Min(
                    Math.Min(distance[i - 1, j] + 1, distance[i, j - 1] + 1),
                    distance[i - 1, j - 1] + cost);
            }
        }

        int maxLength = Math.Max(str1.Length, str2.Length);
        return (float)distance[str1.Length, str2.Length] / maxLength;
    }
}