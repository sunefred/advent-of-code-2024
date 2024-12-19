using System.Diagnostics;
using System.Text.RegularExpressions;

internal class Program
{
    static void Main()
    {
        var input = File.ReadAllText("Data/sample.txt");

        //Console.WriteLine(Part1(input));
        Console.WriteLine(Part2(input));
    }

    static int Part1(string input)
    {
        int possibleDesigns = 0;
        var patterns = GetPatterns(input);
        var designs  = GetDesigns(input);

        foreach (var design in designs)
        {
            var sw = Stopwatch.StartNew();
            var hasMatch = HasMatchingPatterns1(design, patterns);
            if (hasMatch)
            {
                possibleDesigns++;
            }
        }

        return possibleDesigns;
    }

    static int Part2(string input)
    {
        int possibleDesigns = 0;
        var patterns = GetPatterns(input);
        var designs  = GetDesigns(input);

        foreach (var design in designs)
        {
            var matching = NumberOfDesigns2(design, patterns);
            possibleDesigns += matching;
        //     PrintDesignOptions(design, sequences);

        //     if (sequences.Length > 0)
        //     {
        //         possibleDesigns += sequences.Length;
        //     }
        }

        return possibleDesigns;
    }

    static bool HasMatchingPatterns1(string design, string[] patterns)
    {
        var stack = new Stack<string>();
        stack.Push(design);

        while (stack.Count > 0)
        {
            var remaining = stack.Pop();
            if (remaining.Length == 0)
            {
                return true;
            }

            foreach (var pattern in patterns)
            {
                if (remaining.StartsWith(pattern))
                {
                    string newRemaining = remaining[pattern.Length..];
                    stack.Push(newRemaining);
                }
            }
        }

        return false;
    }

    static int  NumberOfDesigns2(string design, string[] patterns)
    {
       
        // Dict for keeping track of number of matches
        // For a specific design (so far)
        var matchDict = new Dictionary<string, int>();

        var stack = new Stack<(string remaining, string[] sequence)>();
        stack.Push((design, []));

        while (stack.Count > 0)
        {
            var (remaining, sequence) = stack.Pop();

            if (remaining.Length == 0)
            {
                for (int i = 0; i <= sequence.Length; i++)
                {
                    var key = string.Join("", sequence[..i]);
                    if (!matchDict.ContainsKey(key))
                    {
                        matchDict[key] = 0;
                    }

                    matchDict[key] = matchDict[key] + 1;
                }

                continue;
            }

            if (matchDict.ContainsKey(remaining))
            {
                for (int i = 0; i < sequence.Length; i++)
                {
                    var key = string.Join("", sequence[..i]);
                    if (!matchDict.ContainsKey(key))
                    {
                        matchDict[key] = 0;
                    }

                    matchDict[key] = matchDict[key] + 1;
                }
            }

            foreach (var pattern in patterns.Reverse())
            {
                if (remaining.StartsWith(pattern))
                {
                    string newRemaining = remaining[pattern.Length..];
                    string[] newSequence = [.. sequence, pattern];
                    stack.Push((newRemaining, newSequence));
                }
            }
        }

        return matchDict[design];
    }

    static void PrintDesignOptions(string design, string[][] sequences)
    {
            Console.WriteLine(design);
            Console.WriteLine("---------");
            foreach (var sequence in sequences)
            {
                Console.WriteLine(string.Join(",", sequence));
            }
            Console.WriteLine("");

    }   

    static string[] GetPatterns(string input)
    {
        var first = input.Split("\n\r")[0];
        var matches = Regex.Matches(first, @"(\w+)");
        return [.. matches.Select(m => m.Value)];
    }

    static string[] GetDesigns(string input)
    {
        var second = input.Split("\n\r")[1];
        var matches = Regex.Matches(second, @"(\w+)");
        return [.. matches.Select(m => m.Value)];
    }
}