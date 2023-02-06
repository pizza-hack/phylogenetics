
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
        public string Node(string name, string label = "", string shape = "ellipse", string style = "", string color = "#87CEEB", string tooltip = "", string fontcolor = "#000000", float width = .3f, float height = .3f)
        {
            label = label == "" ? name : label;
            return $"{{node [xlabel=\"{tooltip}\",width=\"{width}\",height=\"{height}\",shape=\"{shape}\",style=\"{style}\",color=\"{color}\",label=\"{label}\", fontcolor=\"{fontcolor}\"] {name}}}";
        }
            
        public string Edge(string from, string to, string dir = "none", string style = "solid", string label = "", string color = "#000000")
        {
            return $"{{edge [color=\"{color}\",style=\"{style}\",label=\"{label}\",dir=\"{dir}\"] {from} -- {to}}}";
        }
            
        public string End()
        {
            return "}";
        }
    }
}    
