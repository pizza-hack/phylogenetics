using System.Text.RegularExpressions;
using System.Xml;

namespace Graph;

public class SVG_Parser
{
    public static void Run(Dictionary<string,int> codeLines, string path = "Data/graph.svg")
    {
        // Load SVG file
        XmlDocument svgDoc = new XmlDocument();
        svgDoc.Load(ProjectSourcePath.Value + path);

        // Find all nodes with titles containing underscores
        // <g id="NODE_NAME" class="node">
        //     <title>NAME</title>
        //     <text> </text>
        XmlNamespaceManager namespaceManager = GetNamespaceManager(svgDoc);
        XmlNodeList? nodeList = svgDoc.SelectNodes("//svg:g[@class='node']/svg:title[contains(text(), '_')]", namespaceManager);
        if (nodeList != null)
            foreach (XmlNode node in nodeList)
            {
                if (node.ParentNode == null) continue;
                string nodeName = node.ParentNode.SelectSingleNode("svg:title", namespaceManager)?.InnerText ?? string.Empty;

                var innerNode = node.ParentNode.SelectSingleNode("svg:text", namespaceManager);
                string innerText = innerNode?.InnerText ?? string.Empty;
                Console.WriteLine(nodeName + " " + innerText);
                // TODO: Substitute the placeholders with the correct numbers
                // Get the number of lines from the dictionary
                innerText = codeLines[nodeName].ToString();
                if (innerNode != null) innerNode.InnerText = innerText;
            }

        // Save modified SVG file
        svgDoc.Save(ProjectSourcePath.Value +"output.svg");
    }
    
    static XmlNamespaceManager GetNamespaceManager(XmlDocument svgDoc)
    {
        XmlNamespaceManager namespaceManager = new XmlNamespaceManager(svgDoc.NameTable);
        namespaceManager.AddNamespace("svg", "http://www.w3.org/2000/svg");
        namespaceManager.AddNamespace("xlink", "http://www.w3.org/1999/xlink");
        namespaceManager.AddNamespace("dot", "http://www.graphviz.org/doc/info");
        return namespaceManager;
    }
}