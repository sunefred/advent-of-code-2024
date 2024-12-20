namespace Day02;

internal class Program
{
    static void Main()
    {
        /**
         * https://adventofcode.com/2024/day/2
         * 
         */

        var lines = File.ReadAllLines("Data/sample.txt");

        var levels = lines
            .Select(line => line.Split())
            .Select(numbers => numbers.Select(x => int.Parse(x)).ToArray())
            .ToList();

        Console.WriteLine(Part1(levels));
        Console.WriteLine(Part2(levels));
    }

    public static int Part1(IEnumerable<int[]> levels)
    {
        var result = levels
            .Where(IsSafe)
            .Count();

        return result;
    }

    public static int Part2(IEnumerable<int[]> levels)
    {
        var result = levels
            .Select(level => level.Select<int, int[]>((_, i) => [.. level[..i], .. level[(i + 1)..]]))
            .Where(newLevels => newLevels.Any(IsSafe))
            .Count();

        return result;
    }

    private static bool IsSafe(int[] level)
    {
        var diffs = level.Zip(level.Skip(1), (a, b) => b - a).ToArray();

        bool isIncreasing = diffs.All(d => d >= 1 && d <= 3);
        bool isDecreasing = diffs.All(d => d >= -3 && d <= -1);

        return isIncreasing || isDecreasing;
    }
}
