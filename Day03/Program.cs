using System.Text.RegularExpressions;

namespace Day03;

internal class Program
{
    private enum MatchType
    {
        Do,
        Dont,
        Mul
    }

    static void Main()
    {
        /**
         * https://adventofcode.com/2024/day/3
         * 
         */

        var input = File.ReadAllText("Data/sample2.txt");

        Console.WriteLine(Part1(input));
        Console.WriteLine(Part2(input));
    }

    public static int Part1(string input)
    {
        var matches = Regex.Matches(input, @"mul\((\d+),(\d+)\)");

        var pairs = matches
            .Select(match =>
            {
                var a = int.Parse(match.Groups[1].Value);
                var b = int.Parse(match.Groups[2].Value);
                return (a, b);
            })
            .ToArray();

        var result = pairs.Select(x => x.Item1 * x.Item2).Sum();
        return result;
    }

    public static int Part2(string input)
    {
        var result = 0;
        var index = 0;
        var mulActive = true;

        while (true)
        {
            var (match, matchType) = GetFirstMatch(input[index..]);
            if (match == null)
            {
                break;
            }

            switch (matchType)
            {
                case MatchType.Do:
                {
                    mulActive = true;
                    index += match.Index + match.Length;
                    break;
                }
                case MatchType.Dont:
                {
                    mulActive = false;
                    index += match.Index + match.Length;
                    break;
                }
                case MatchType.Mul:
                {
                    if (mulActive)
                    {
                        var a = int.Parse(match.Groups[1].Value);
                        var b = int.Parse(match.Groups[2].Value);
                        result += a * b;
                    }
                    index += match.Index + match.Length;
                    break;
                }
            }
        }

        return result;
    }

    private static (Match, MatchType) GetFirstMatch(string input)
    {
        var doMatch = Regex.Match(input, @"do\(\)");
        var dontMatch = Regex.Match(input, @"don't\(\)");
        var mulMatch = Regex.Match(input, @"mul\((\d+),(\d+)\)");

        var matches = new[]
        {
            (doMatch, MatchType.Do),
            (dontMatch, MatchType.Dont),
            (mulMatch, MatchType.Mul)
        };

        return matches
            .Where(m => m.Item1.Success)
            .OrderBy(m => m.Item1.Index)
            .FirstOrDefault();
    }
}