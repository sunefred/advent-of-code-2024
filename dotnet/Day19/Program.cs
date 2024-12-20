using System.Diagnostics;
using System.Text.RegularExpressions;
using Microsoft.VisualBasic;

internal class Program
{
    static void Main()
    {
        var input = File.ReadAllText("Data/input.txt");

        Console.WriteLine(Part1(input));
        Console.WriteLine(Part2(input));
    }

    static int Part1(string input)
    {
        var patterns = GetPatterns(input);
        var designs = GetDesigns(input);
        var designsWithSolutions = designs.Count(d => HasSolution(d, patterns));
        return designsWithSolutions;
    }

    static long Part2(string input)
    {
        var patterns = GetPatterns(input);
        var designs = GetDesigns(input);
        var memo = new Dictionary<string, long>();
        var totalSolutions = designs.Sum(d => CountSolutions(d, patterns, memo));
        return totalSolutions;
    }

    static bool HasSolution(string design, string[] patterns)
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

    static long CountSolutions(string design, string[] patterns, Dictionary<string, long> memo)
    {
        if (memo.ContainsKey(design))
        {
            return memo[design];
        }

        if (design.Length == 0)
        {
            return 1;
        }

        long count = 0;
        foreach (var pattern in patterns)
        {
            if (design.StartsWith(pattern))
            {
                string newDesign = design[pattern.Length..];
                count += CountSolutions(newDesign, patterns, memo);
            }
        }

        memo[design] = count;
        return count;
    }

    static string[] GetPatterns(string input)
    {
        var first = input.Split("\n\n")[0];
        var matches = Regex.Matches(first, @"(\w+)");
        return [.. matches.Select(m => m.Value)];
    }

    static string[] GetDesigns(string input)
    {
        var second = input.Split("\n\n")[1];
        var matches = Regex.Matches(second, @"(\w+)");
        return [.. matches.Select(m => m.Value)];
    }
}