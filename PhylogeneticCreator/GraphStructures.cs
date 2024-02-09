
namespace Graph
{
    public class GraphStructures
    {
        
        public string Init(string graphName, string type = "graph")
        {
            return $"{type} {graphName} \n{{\n";
        }

        public string Parameters(string layout = "neato", string label = "", string mode = "major", string model = "shortpath", string size = "", string ratio = "fill",
            string orientation = "portrait")
        {
            return "\tlayout=" + layout + "\n\tlabel=\"" + label + "\"\n\tmode=" + mode + "\n\tmodel=" + model + "\n\tsize=\"" + size + "\"\n\tratio=" + ratio +
                   "\n\torientation=" + orientation + "\n\n";
        }
        public string Node(string name, string id, Dictionary<string, object> options = null, string label = "", string shape = "ellipse", string style = "", string color = "#87CEEB", string tooltip = "", string fontcolor = "#000000", float width = .3f, float height = .3f)
        {
            label = label == "" ? name : label;
            
            if (options != null && options.Count > 0)
            {
                // The basic option cases:
                // - label with ownership
                // - data
                
                // Label with ownership
                // If options contains "ownership" then we process the origin of the label and add it to the label
                // Or if the tooltip is NOT a number
                if (options.TryGetValue("ownership", out object ownershipData) && ownershipData is string ownership)
                {
                    string addLabel = "<table border=\"0\" cellborder=\"1\" cellspacing=\"0\">";
                    string emptyCell = "<td border=\"0\" width=\"20\" height=\"20\"></td>";
                    // We search the name in the data_location file which is a .csv file with the following format:
                    // - First column: name of the node
                    // - First row: origin of the node
                    // - Other cells: 0/1 if the node is owned by the origin
                    // We search the name in the first column and then we search the 1 in the row of the origin
                    // If we find it, we add the origin to the label
                    // We get the lines from the
                    string[] lines = File.ReadAllLines(ProjectSourcePath.Value + ownership);
                    string[] firstColumn = lines.Select(line => line.Split(';')[0]).ToArray();
                    string[] firstRow = lines[0].Split(';');
                    List<string> owners = new List<string>();
                    int index = Array.IndexOf(firstColumn, name);
                    if (index != -1)
                    {
                        string[] row = lines[index].Split(';');
                        for (int i = 1; i < row.Length; i++)
                        {
                            if (row[i] == "1")
                            {
                                owners.Add(firstRow[i]);
                            }
                        }
                    }
                    
                    // We have 3 owners per row, calculate the number of rows
                    int rows = (int) Math.Ceiling((double) owners.Count / 3);
                    
                    // Add the first row with the name of the node
                    string function = $"<td bgcolor=\"lightcoral\" rowspan=\"{rows}\">{name}</td>";
                    
                    addLabel += $"<tr>{function}{emptyCell}";
                    
                    Dictionary<string, string> ownershipColor = new Dictionary<string, string>();
                    
                    // Try to get the ownershipColors from the options which is a Dictionary<string, string>
                    if (options.TryGetValue("ownershipColors", out object ownershipColorsData) && ownershipColorsData is Dictionary<string, string> ownershipColors)
                    {
                        ownershipColor = ownershipColors;
                    }
                    
                    // If there are more than 3 owners, we add the first 3 to the first row
                    // and the rest to the next rows
                    if (owners.Count > 3)
                    {
                        for (int i = 0; i < 3; i++)
                        {
                            addLabel += $"<td bgcolor=\"{GetColorFromDictionaryOrDefault(ownershipColor, owners[i])}\">{owners[i]}</td>";
                        }
                        addLabel += "</tr>";
                        for (int i = 3; i < owners.Count; i += 3)
                        {
                            addLabel += $"<tr>{emptyCell}";
                            for (int j = i; j < i + 3 && j < owners.Count; j++)
                            {
                                addLabel += $"<td bgcolor=\"{GetColorFromDictionaryOrDefault(ownershipColor, owners[j])}\">{owners[j]}</td>";
                            }
                            addLabel += "</tr>";
                        }
                    }
                    else
                    {
                        for (int i = 0; i < owners.Count; i++)
                        {
                            addLabel += $"<td bgcolor=\"{GetColorFromDictionaryOrDefault(ownershipColor, owners[i])}\">{owners[i]}</td>";
                        }
                        addLabel += "</tr>";
                    }
                    
                    addLabel += "</table>";
                    label = addLabel;
                }
            }

            if (id == "-1")
            {
                return $"{{node [tooltip=\"{tooltip}\",width=\"{width}\",height=\"{height}\",shape=\"{shape}\",style=\"{style}\",color=\"{color}\",label=<<FONT COLOR=\"{fontcolor}\">{label}</FONT>>] {name}}}";
            }
            else
            {
                return $"{{node [tooltip=\"{tooltip}\",width=\"{width}\",height=\"{height}\",shape=\"{shape}\",style=\"{style}\",color=\"{color}\",label=<<FONT COLOR=\"{fontcolor}\">{label}</FONT>>] {id}}}";
            }
    
        }
        
        private string GetColorFromDictionaryOrDefault(Dictionary<string, string> dictionary, string key, string defaultValue = "lightgoldenrodyellow")
        {
            return dictionary.TryGetValue(key, out string value) ? value : defaultValue;
        }
            
        public string Edge(string from, string to, string dir = "none", string style = "solid", string label = "", string color = "#000000")
        {
            return $"{{edge [color=\"{color}\",style=\"{style}\",label=\"{label}\",dir=\"{dir}\"] {from} -- {to}}}";
        }
        
        // Legend is a special node that is used to display a legend for the graph
        // It has an undefined number of Tuple<string, string> that are used to display the legend
        // The first string is the text to display and the second is the color of the text
        public string Legend(string margin1 = "2.5", string margin2 = "1.25", int fontSize = 14, int border = 0, int cellBorder = 1, int cellSpacing = 0, int cellPadding = 4, string label = "Legend", params Tuple<string, string>[] elements)
        {
            string rank = "{rank=max; legend}";
            string legend = $"{{node [shape=plaintext, fontsize={fontSize}, label=<<TABLE BORDER=\"{border}\" CELLBORDER=\"{cellBorder}\" CELLSPACING=\"{cellSpacing}\" CELLPADDING=\"{cellPadding}\">\n";
            legend += $"<TR><TD COLSPAN=\"2\"><B>{label}</B></TD></TR>\n";
            foreach (var element in elements)
            {
                legend += $"<TR><TD>{element.Item1}</TD><TD BGCOLOR=\"{element.Item2}\"><FONT COLOR=\"{element.Item2}\">Foo</FONT></TD></TR>\n";
            }
            legend += $"</TABLE>>, margin=\"{margin1},{margin2}\"] legend}}";
            /*pos="0,0!"*/
            return legend + "\n" + rank;
        }
            
        public string End()
        {
            return "}";
        }
    }
}    
