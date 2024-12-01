using System.Text.RegularExpressions;

namespace Day01;

internal class Program
{
    private static void Main()
    {
        /**
         * https://adventofcode.com/2024/day/1
         * 
         */

        var input = File.ReadAllText("Data/input.txt");
        var matches = Regex.Matches(input, @"(\d+)\s+(\d+)");

        var pairs = matches
            .Select(x => (x.Groups[1].Value, x.Groups[2].Value))
            .Select(x => (int.Parse(x.Item1), int.Parse(x.Item2)))
            .ToArray();

        Console.WriteLine(Part1(pairs));
        Console.WriteLine(Part2(pairs));
    }

    public static int Part1(IEnumerable<(int, int)> pairs)
    {
        var firsts = pairs.Select(x => x.Item1).Order();
        var seconds = pairs.Select(x => x.Item2).Order();

        var score = Enumerable.Zip(firsts, seconds)
            .Select(x => Math.Abs(x.First - x.Second))
            .Sum();

        return score;
    }

    public static int Part2(IEnumerable<(int, int)> pairs)
    {
        var firsts = pairs.Select(x => x.Item1).Order();
        var seconds = pairs.Select(x => x.Item2)
            .GroupBy(x => x)
            .ToDictionary(x => x.Key, x => x.Count());

        var score = firsts
            .Select(first => first * (seconds.TryGetValue(first, out var second) ? second : 0))
            .Sum();

        return score;
    }
}
