using System.Text;
using System.Text.Json;

namespace Graph;

public class JSONToString
{
    public static void Run(string path = "XML/maia.json")
    {
        // Load the JSON file
        string json = File.ReadAllText(ProjectSourcePath.Value + path);
        
        // Construct a multidimensional array from the JSON file
        // We dont know the size nor the dimensions of the array, so we use a list
        var list = new List<List<string>>();
        var options = new JsonSerializerOptions
        {
            AllowTrailingCommas = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            PropertyNameCaseInsensitive = true,
        };
        var jsonElement = JsonSerializer.Deserialize<JsonElement>(json, options);
        // Foreach in the list of lists
        foreach (var innerList in jsonElement.EnumerateArray())
        {
            // Create a new list
            var newList = new List<string>();
            // Foreach in the list
            foreach (var element in innerList.EnumerateArray())
            {
                // Add the element to the list
                newList.Add(element.GetString());
            }
            // Add the list to the list of lists
            list.Add(newList);
        }
        
        // Use a StringBuilder to construct the string representation
        StringBuilder builder = new StringBuilder();
        
        // Print all to the string builder and to the console
        foreach (var innerList in list)
        {
            foreach (var element in innerList)
            {
                builder.Append(element + " ");
                Console.Write(element + " ");
            }
            builder.Append(Environment.NewLine);
            Console.WriteLine();
        }
    }
}