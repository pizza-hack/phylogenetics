using System.Globalization;

public class DataStructures
{
    private const float delta = 0.0001f;

    public struct Pair
    {
        public bool Equals(Pair other)
        {
            return First == other.First && Second == other.Second;
        }

        public override bool Equals(object? obj)
        {
            return obj is Pair other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(First, Second);
        }

        public Pair(string first, string second)
        {
            this.First = first;
            this.Second = second;
        }
        
        public string First { get; set; }

        public string Second { get; set; }


        public override string ToString()
        {
            return string.Format("{0}_{1}", First, Second);
        }
        
        public bool Contains(string value)
        {
            return First == value || Second == value;
        }
        
        public Pair Inverse()
        {
            return new Pair(Second, First);
        }
        
        // Overload the != operator
        public static bool operator !=(Pair a, Pair b)
        {
            return !(a == b);
        }
        
        // Overload the == operator
        public static bool operator ==(Pair a, Pair b)
        {
            return a.First == b.First && a.Second == b.Second;
        }
    }
    
    public struct OutputPair
    {
        public OutputPair(string first, string second, int value)
        {
            this.First = first;
            this.Second = second;
            this.Value = value;
        }

        public OutputPair(string first, string second, float value)
        {
            this.First = first;
            this.Second = second;
            this.Value = value;
        }
        
        public string First { get; set; }

        public string Second { get; set; }

        public float Value { get; set; }
        
        public override string ToString()
        {
            return string.Format("{0}_{1}={2}", First, Second, Value);
        }
    }

    public class PhylogeneticDictionary : Dictionary<Pair, float>
    {
        // Vector of header names
        public string[]? Headers { get; set; }
        // A Vector with string Vectors inside
        public int layer = 0;
        public bool Add(string first, string second, float value)
        {
            // Add the first or second header if it is not already in the list of headers
            if (Headers == null)
            {
                if (first != second)
                {
                    Headers = new string[] { first, second };
                }
                else
                {
                    Headers = new string[] { first };
                }
            }
            else
            {
                if (!Headers.Contains(first))
                {
                    Headers = Headers.Append(first).ToArray();
                }
                if (!Headers.Contains(second))
                {
                    Headers = Headers.Append(second).ToArray();
                }
            }

            // Add the pair to the dictionary if it is not already in it
            if (!ContainsKey(new Pair(first, second)))
            {
                Add(new Pair(first, second), value);
                // Add the inverse pair if it is not already in it
                if (!ContainsKey(new Pair(second, first)))
                {
                    Add(new Pair(second, first), value);
                }
                return true;
            }
            // Else if contains the inverse
            else if (!ContainsKey(new Pair(second, first)))
            {
                Add(new Pair(second, first), value);
                return true;
            }
            return false;
        }
        public float Get(string first, string second)
        {
            return this[new Pair(first, second)];
        }
        
        public bool Contains(string first, string second)
        {
            return ContainsKey(new Pair(first, second));
        }
        
        // Remove a pair from the dictionary and the inverse pair
        public bool Remove(string first, string second)
        {
            if (ContainsKey(new Pair(first, second)))
            {
                Remove(new Pair(first, second));
                Remove(new Pair(second, first));
                return true;
            }
            return false;
        }
        
        public void Set(string first, string second, float value)
        {
            this[new Pair(first, second)] = value;
            this[new Pair(second, first)] = value;
        }

        public string[] CheckCombinations(string first)
        {
            // Checks which combinations exist for the given first value and returns them
            List<string> combinations = new List<string>();
            foreach (var pair in this)
            {
                if (pair.Key.First == first && pair.Key.Second != first)
                {
                    combinations.Add(pair.Key.Second);
                }
            }
            return combinations.ToArray();
        }
        
        public float[] getAllValues()
        {
            // Returns all the values in the dictionary which are not the diagonal (i.e. the same pair string)
            List<float> values = new List<float>();
            foreach (var pair in this)
            {
                if (pair.Key.First != pair.Key.Second)
                {
                    values.Add(pair.Value);
                }
            }
            return values.ToArray();
        }

        public Pair[] getMinPairs()
        {
            // Returns the pairs which have the minimum value in the dictionary
            List<Pair> minPairs = new List<Pair>();
            float minValue = getAllValues().Min();
            foreach (var pair in this)
            {
                // If the pairs strings are the same, skip
                if (pair.Key.First == pair.Key.Second)
                    continue;
                if (Math.Abs(pair.Value - minValue) < delta)
                {
                    // If the inverse pair isnt already in the list, add it
                    if (!minPairs.Contains(new Pair(pair.Key.Second, pair.Key.First)))
                    {
                        minPairs.Add(pair.Key);
                    }
                }
            }
            return minPairs.ToArray();
        }

        // Remove a value from the header[]
        public bool RemoveHeader(string header)
        {
            if ((Headers ?? Array.Empty<string>()).Contains(header))
            {
                Headers = (Headers ?? Array.Empty<string>()).Where(val => val != header).ToArray();
                return true;
            }
            return false;
        }
    }
    
    // Genetic dictionary class (string, string) with an ADN like structure
    public class GeneticDictionary : Dictionary<string,string>
    {
        // Const values with "diff" "inf" and "sim" to be called by the user without knowing them
        public const string TYPE_SIMPLE_DIFFERENCES = "diff";
        public const string TYPE_INFERENCE_IN_GENE = "inf";
        public const string TYPE_SIMILITUDE_ALONG_GENES = "sim";
        
        private string type;
        
        // On creation, set which type of gene will be used (diff, inf or sim)
        public GeneticDictionary(string type)
        {
            this.type = type;
        }
        
        // Add a new string to the dictionary
        public bool AddGene(string label, string gen)
        {
            // Add the label to the dictionary if it is not already in it
            if (!ContainsKey(label))
            {
                Add(label, gen);
                return true;
            }
            return false;
        }
        
        // Print all the genes in the dictionary
        public void PrintGenes(string path = "")
        {
            if (path == "")
            {
                // Is there only one gene?
                if (Count == 1)
                {
                    // Print the gene with a new line for every " or space
                    foreach (var gene in this)
                    {
                        Console.WriteLine(gene.Key + " -> " + gene.Value.Replace("\"", "\n").Replace(" ", "\n"));
                    }
                }
                else
                {
                    foreach (var gene in this)
                    {
                        Console.WriteLine(gene.Key + " -> " + gene.Value);
                    }
                }
            }
            else
            {
                using (StreamWriter sw = new StreamWriter(path))
                {
                    // Is there only one gene?
                    if (Count == 1)
                    {
                        // Print the gene with a new line for every " or space
                        foreach (var gene in this)
                        {
                            sw.WriteLine(gene.Key + " -> " + gene.Value.Replace("\"", "\n").Replace(" ", "\n"));
                        }
                    }
                    else
                    {
                        foreach (var gene in this)
                        {
                            sw.WriteLine(gene.Key + " -> " + gene.Value);
                        }
                    }
                }
            }
        }

        // Calculate the new genetic value for two given entrances
        public Tuple<string,string> CalculateNewGene(string first, string second, bool removeParents = false)
        {
            // Get the string representing the first and second value a
            string firstGen = this[first];
            string secondGen = this[second];
            
            // create a new string that calls switch( diff, inf or sim) depending on the type defined
            string newGen = "";

            switch (type)
            {
                case TYPE_SIMPLE_DIFFERENCES:
                    newGen = SimpleDifferences(firstGen, secondGen);
                    break;
                case TYPE_INFERENCE_IN_GENE:
                    newGen = InferenceInGene(firstGen, secondGen);
                    break;
                case TYPE_SIMILITUDE_ALONG_GENES:
                    newGen = SimilitudeAlongGenes(firstGen, secondGen);
                    break;
                default:
                    newGen = SimpleDifferences(firstGen, secondGen);
                    break;
            }
            
            // Add the new value to the dictionary (string, string) (first + '_' + second = newString)
            string newLabel = first + '_' + second;
            Console.WriteLine(newLabel + " -> " + AddGene(first + '_' + second, newGen));
            // Print the newGene value with all * in red and the rest in default
            WriteLineWithColoredLetter(newGen, '*', ConsoleColor.Red);
            if (removeParents)
            {
                // Remove the parents from the dictionary
                Remove(first);
                Remove(second);
            }
            
            // Return a tuple with the new label and the new gene
            return new Tuple<string, string>(newLabel, newGen);
        }
        
        private void WriteLineWithColoredLetter(string text, char c, ConsoleColor color = ConsoleColor.Red) {
            int o = text.IndexOf(c);
            while (o != -1)
            {
                Console.Write(text.Substring(0, o));
                Console.ForegroundColor = color;
                Console.Write(c);
                Console.ResetColor();
                text = text.Substring(o + 1);
                o = text.IndexOf(c);
            }
            Console.Write(text);
            Console.WriteLine();
        }

        // Calculate the new genetic value for two given entrances by using the SimpleDifferences method
        public string SimpleDifferences(string firstGen, string secondGen)
        {
            // If the characters match, we add the character to the new string
            // If they dont match, we add * to the new string
            string newString = "";
            for (int i = 0; i < firstGen.Length; i++)
            {
                if (firstGen[i] == secondGen[i])
                {
                    newString += firstGen[i];
                }
                else
                {
                    newString += '*';
                }
            }
            return newString;
        }
        
        // Calculate the new genetic value for two given entrances by using the InferenceInGene method
        public string InferenceInGene(string firstGen, string secondGen)
        {
            // If the characters match, we add the character to the new string
            // If they dont match, we see if it matches with the characters from the previous space to the next space
            // If it matches, we add the character to the new string
            // If it doesnt match, we add * to the new string
            string newString = "";
            // While we have characters to check
            while (firstGen.Length > 0)
            {
                // Get all genes until the next space or the end of the string
                var currentGen1 = firstGen.Substring(0, firstGen.IndexOf(' ') != -1 ? firstGen.IndexOf(' ') : firstGen.Length);
                var currentGen2 = secondGen.Substring(0, secondGen.IndexOf(' ') != -1 ? secondGen.IndexOf(' ') : secondGen.Length);
                
                // Add the contained genes to the new string and * when they dont match
                // ATC & TCB = *TC
                foreach (var gene in currentGen1)
                {
                    // If the gene is contained in the other gene, add it to the new string
                    if (currentGen2.Contains(gene))
                    {
                        newString += gene;
                    }
                    // If the gene is not contained in the other gene, add * to the new string
                    else
                    {
                        newString += '*';
                    }
                }
                
                // Remove the genes from the strings
                firstGen = firstGen.Substring(currentGen1.Length);
                secondGen = secondGen.Substring(currentGen2.Length);
                
                // If the strings are not empty, add a space to the new string
                if (firstGen.Length > 0)
                {
                    newString += ' ';
                }
            }
            return newString;
        }
        
        // Calculate the new genetic value for two given entrances by using the SimilitudeAlongGenes method
        public string SimilitudeAlongGenes(string firstGen, string secondGen)
        {
            // Get all genes in an array for each string
            string[] firstGenes = firstGen.Split(' ');
            string[] secondGenes = secondGen.Split(' ');
            
            // We have to search the genes which similarities are higher and match them (reduce differences)
            // We get the inference value for each gene compared to the other genes
            // We have to find the combination of genes that have the minimun inference value
            // We have to add the genes that are in the combination to the new string and * to the rest
            
            // Get the minimum pairs of genes
            float[][] minPairs = Array.Empty<float[]>();
            for (int i = 0; i < firstGenes.Length; i++)
            {
                for (int j = 0; j < secondGenes.Length; j++)
                {
                    minPairs[i][j] = this.CalculateGeneValueInf(firstGenes[i], secondGenes[j]);
                }
            }
            
            // Now we have to find the combination of genes that have the minimun inference value
            // The value is unique, columns can only be used for one row value
            // We have to find the minimum value combination and then remove the row and column from the array
            // We have to repeat this until the array is empty
            
            // Create a new string to add the genes to
            string newString = "";
            // While we have genes to check
            while (firstGenes.Length > 0 || secondGenes.Length > 0)
            {
                // TODO: Find the minimum value combination and then remove the row and column from the array
            }

            return newString;
        }
        
        // Get the genetic value for a given label
        public string GetGen(string label)
        {
            return this[label];
        }
        
        // Calculates the value between two labels
        public float CalculateValue(string first, string second)
        {
            if (first == second)
            {
                return 0;
            }
            // Get the length of the first and second string without counting the spaces or "
            int length = this[first].Replace(" ", "").Replace("\"", "").Length;

            // Get the string representing the first and second value
            string[] firstGen = this[first].Split(' ');
            string[] secondGen = this[second].Split(' ');
            
            // For each gene, calculate the value
            float value = 0;
            for (int i = 0; i < firstGen.Length; i++)
            {
                value += this.CalculateGeneValue(firstGen[i], secondGen[i],i);
            }
            Console.WriteLine("\n\nValue: " + value + " / " + length);
            value /= length;
            return value;
        }
        
        // Calculates the value between two specific genes
        public float CalculateGeneValue(string first, string second, int it, bool debug = false)
        {
            // Remove the " from the string
            first = first.Replace("\"", "");
            second = second.Replace("\"", "");
            
            int value = 0;
            if (debug)
            {
                string name = "";
                switch (it)
                {
                    case 0:
                        name = "VI -Vital - Puntos debiles";
                        break;
                    case 1:
                        name = "ME -Melee - Armas de contacto";
                        break;
                    case 2:
                        name = "BU - Bullet - Armas de balas";
                        break;
                    case 3:
                        name = "HO - Homing - Armas de proyectiles guiados";
                        break;
                    case 4:
                        name = "LA - Laser - Armas laser";
                        break;
                    case 5:
                        name = "HU - Hull - Cascos habilitados";
                        break;
                    case 6:
                        name = "LI - Link - Padres de los cascos, vía enlace: 0..9a..zA..Z[]";
                        break;
                    case 7:
                        name = "BE - Behaviour - Cascos que lideran el comportamiento";
                        break;
                    default:
                        name = "Unknown";
                        break;
                }
                Console.WriteLine("\n\n" + name + ":");
            }
            for (int i = 0; i < first.Length; i++)
            {
                if (first[i] != second[i])
                {
                    value++;
                }

                if (debug)
                {
                    if(i % (16*4) == 0 && i != 0)
                    {
                        Console.SetCursorPosition(0, Console.CursorTop + 2);
                    }
                    // Print the value of the first and second gene (one per line) 
                    // If they match, we print the character normally
                    // If they dont match, we print the character in red
                    if (first[i] == second[i])
                    {
                        // Print the character normally on the first line and the second line
                        Console.Write(first[i]);
                        Console.SetCursorPosition(Console.CursorLeft - 1, Console.CursorTop + 1);
                        Console.Write(second[i]);
                        Console.SetCursorPosition(Console.CursorLeft, Console.CursorTop - 1);
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        // Print the character in red on the first line and the second line
                        Console.Write(first[i]);
                        Console.SetCursorPosition(Console.CursorLeft-1, Console.CursorTop + 1);
                        Console.Write(second[i]);
                        Console.SetCursorPosition(Console.CursorLeft, Console.CursorTop - 1);
                        Console.ResetColor();
                    }
                }
            }

            if (debug)
            {
                Console.SetCursorPosition(0, Console.CursorTop + 1);
            }
            
            return value;
        }
        
        // Calculates the value between two specific genes (inf)
        public float CalculateGeneValueInf(string first, string second)
        {
            int value = 0;
            for (int i = 0; i < first.Length; i++)
            {
                // If the gene is contained in the other gene, add it to the new string
                // If we have two same values we expect to also have two on the other side
                if (!second.Contains(first[i]))
                {
                    value++;
                }
            }
            return value;
        }
    }
}