namespace Day07;

internal class Program
{
    static void Main()
    {
        /**
         * https://adventofcode.com/2024/day/7
         * 
         */

        var lines = File.ReadAllLines("Data/sample.txt");
        var equations = lines
            .Select(line => line.Split(":"))
            .Select(parts =>
            {
                var target = long.Parse(parts[0].Trim());
                var numbers = parts[1].Trim().Split(" ").Select(long.Parse).ToArray();
                return (target, numbers);
            })
            .ToArray();

        Console.WriteLine(Part1(equations));
        Console.WriteLine(Part2(equations));
    }

    static long Part1((long Target, long[] Numbers)[] lines)
    {
        return lines
            .Where(line => CanAchieveTarget(line.Target, line.Numbers, true, true, false))
            .Sum(equation => equation.Target);
    }

    static long Part2((long Target, long[] Numbers)[] equations)
    {
        return equations
            .Where(equation => CanAchieveTarget(equation.Target, equation.Numbers, true, true, true))
            .Sum(equation => equation.Target);
    }

    static bool CanAchieveTarget(long target, long[] numbers, bool tryAdd, bool tryMul, bool tryCat)
    {
        return CheckCombinations(target, 0, numbers, tryAdd, tryMul, tryCat);
    }

    static bool CheckCombinations(long target, long currentValue, long[] remainingNumbers, bool tryAdd, bool tryMul, bool tryCat)
    {
        if (remainingNumbers.Length == 0)
        {
            return currentValue == target;
        }

        if (tryAdd)
        {
            var addedValue = currentValue + remainingNumbers[0];
            if (CheckCombinations(target, addedValue, remainingNumbers[1..], tryAdd, tryMul, tryCat))
            {
                return true;
            }
        }

        if (tryMul)
        {
            var multipliedValue = currentValue * remainingNumbers[0];
            if (CheckCombinations(target, multipliedValue, remainingNumbers[1..], tryAdd, tryMul, tryCat))
            {
                return true;
            }
        }

        if (tryCat)
        {
            long concatenatedValue = long.Parse(currentValue.ToString() + remainingNumbers[0].ToString());
            if (CheckCombinations(target, concatenatedValue, remainingNumbers[1..], tryAdd, tryMul, tryCat))
            {
                return true;
            }
        }

        return false;
    }
}
