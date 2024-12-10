namespace Day11;

internal class Program
{
    static void Main()
    {
        /**
         * https://adventofcode.com/2024/day/11
         *
         */

        var input = File.ReadAllText("Data/input.txt");
        var stones = input.Split(" ").Select(long.Parse).ToList();

        Console.WriteLine(Part1(stones));
        Console.WriteLine(Part2(stones));
    }

    static long Part1(List<long> stones)
    {
        var blinks = 25;

        for (int i = 0; i < blinks; i++)
        {
            stones = Blink(stones);
            Console.WriteLine($"After {i + 1} blinks: {stones.Count}");
        }

        return stones.Count;
    }

    static long Part2(List<long> stones)
    {
        var blinks = 75;
        var stonesDict = stones.ToDictionary(x => x, x => 1L);

        for (int i = 0; i < blinks; i++)
        {
            stonesDict = BlinkButWithoutDuplicates(stonesDict);
            Console.WriteLine($"After {i + 1} blinks: {stonesDict.Sum(x => x.Value)}");
        }

        return stonesDict.Sum(x => x.Value);
    }

    static List<long> Blink(List<long> stones)
    {
        var newStones = new List<long>();

        foreach (var stone in stones)
        {
            // Rule 1: Stone with number 0 becomes 1
            if (stone == 0)
            {
                newStones.Add(1);
            }

            // Rule 2: Stone splits if it has an even number of digits
            else if (stone.ToString().Length % 2 == 0)
            {
                string stoneStr = stone.ToString();
                int mid = stoneStr.Length / 2;
                long left = long.Parse(stoneStr[..mid]);
                long right = long.Parse(stoneStr[mid..]);
                newStones.Add(left);
                newStones.Add(right);
            }

            // Rule 3: Stone is multiplied by 2024
            else
            {
                newStones.Add(stone * 2024);
            }
        }

        return newStones;
    }

    static Dictionary<long, long> BlinkButWithoutDuplicates(Dictionary<long, long> stones)
    {
        var newStones = new Dictionary<long, long>();

        foreach (var (stone, count) in stones)
        {
            // Rule 1: Stone with number 0 becomes 1
            if (stone == 0)
            {
                newStones[1] = newStones.TryGetValue(1, out long value) ? value + count : count;
            }

            // Rule 2: Stone splits if it has an even number of digits
            else if (stone.ToString().Length % 2 == 0)
            {
                string stoneStr = stone.ToString();
                int mid = stoneStr.Length / 2;
                long left = long.Parse(stoneStr[..mid]);
                long right = long.Parse(stoneStr[mid..]);
                newStones[left] = newStones.TryGetValue(left, out long value1) ? value1 + count : count;
                newStones[right] = newStones.TryGetValue(right, out long value2) ? value2 + count : count;
            }

            // Rule 3: Stone is multiplied by 2024
            else
            {
                var big = stone * 2024;
                newStones[big] = newStones.TryGetValue(big, out long value) ? value + count : count;
            }
        }

        return newStones;
    }
}
