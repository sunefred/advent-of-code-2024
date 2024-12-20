using MathNet.Numerics;
using MathNet.Numerics.LinearAlgebra;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Day13;

internal class Program
{
    record class Button(int MoveX, int MoveY, int Cost);
    record class Machine(Button A, Button B, Vector2 Prize);
    record class Line(Vector2 Start, Vector2 Step);

    record class Vector2(long X, long Y)
    {
        public static Vector2 operator +(Vector2 p, Vector2 v) => new(p.X + v.X, p.Y + v.Y);
        public static Vector2 operator -(Vector2 p, Vector2 v) => new(p.X - v.X, p.Y - v.Y);
        public static Vector2 operator *(Vector2 v, long s) => new(v.X * s, v.Y * s);
        public static Vector2 operator *(long s, Vector2 v) => new(v.X * s, v.Y * s);
        public static Vector2 operator /(Vector2 v, long s) => new(v.X / s, v.Y / s);
    }

    static void Main()
    {
        /**
         * https://adventofcode.com/2024/day/13
         * 
         */

        var input = File.ReadAllText("Data/sample.txt");

        Console.WriteLine($"Part 1: {Part1(input)}");
        Console.WriteLine();
        Console.WriteLine($"Part 2: {Part2(input)}");
    }

    static long Part1(string input)
    {
        var totalCost = 0L;
        var machines = ParseInput(input);

        foreach (var (machine, i) in machines.Select((m, i) => (m, i)))
        {
            if (TryGetSolutionUsingBruteForce(machine, out var solution))
            {
                var (A, B) = solution;
                var cost = A * machine.A.Cost + B * machine.B.Cost;
                Console.WriteLine($"{i:D3}: A={A}, B={B}, Cost={cost}");

                totalCost += cost;
            }
        }

        return totalCost;
    }

    static long Part2(string input)
    {
        var totalCost = 0L;
        var offset = 10_000_000_000_000L;
        var machines = ParseInput(input);
        var newMachines = machines.Select(m => new Machine(m.A, m.B, new Vector2(m.Prize.X + offset, m.Prize.Y + offset)));

        foreach (var (machine, i) in newMachines.Select((m, i) => (m, i)))
        {
            if (TryGetSolutionWithFancyMath(machine, out var solution))
            {
                var (A, B) = solution;
                var cost = A * machine.A.Cost + B * machine.B.Cost;
                Console.WriteLine($"{i:D3}: A={A}, B={B}, Cost={cost}");

                totalCost += cost;
            }
        }

        return totalCost;
    }

    static bool TryGetSolutionUsingBruteForce(Machine machine, [NotNullWhen(true)] out Vector2? solution)
    {
        solution = default;

        for (int x = 0; x < 100; x++)
        {
            for (int y = 0; y < 100; y++)
            {
                if (machine.A.MoveX * x + machine.B.MoveX * y == machine.Prize.X &&
                    machine.A.MoveY * x + machine.B.MoveY * y == machine.Prize.Y)
                {
                    solution = new Vector2(x, y);
                    return true;
                }
            }
        }

        return false;
    }

    static bool TryGetSolutionWithFancyMath(Machine machine, [NotNullWhen(true)] out Vector2? solution)
    {
        /**
         * I dont know how to solve a system of Diophantine equations, but I do know how to solve
         * a single Diophantine equation. So, I solve for the number of integer steps in X and Y directions
         * separately and then find the shared solution where the steps between the two match.
         *
         * Assume the integer solutions in X direction has A and B button presses, where A and B are:
         *
         *      A = A0 + k * stepA
         *      B = B0 + k * stepB
         *
         * This can be expressed as a line in N2 space (solutions represented by points on this line):
         *
         *      lineX = (A0, B0) + k * (stepA, stepB)
         *
         * Similarly, the integer solution in Y direction can be expressed as lineY. The shared
         * solution is the point where lineX and lineY intersect, which is expressed as a 
         * linear system of equations:
         *
         *      A x = b
         *
         * We use a math library to solve this system of equations in R2. Since our solutions are
         * are in N2, we can round the solution to the nearest integer. A solution that is different
         * after rounding is not and integer solutions to the original problem and can be discarded.
         */

        solution = default;

        if (!TryGetDiophantineSolutions(machine.A.MoveX, machine.B.MoveX, machine.Prize.X, out var lineX))
        {
            return false;
        }

        if (!TryGetDiophantineSolutions(machine.A.MoveY, machine.B.MoveY, machine.Prize.Y, out var lineY))
        {
            return false;
        }

        var A = Matrix<double>.Build.DenseOfArray(new double[,] {
                { lineX.Step.X, -lineY.Step.X },
                { lineX.Step.Y, -lineY.Step.Y }
            });

        var b = Vector<double>.Build.Dense([
            lineY.Start.X - lineX.Start.X,
                lineY.Start.Y - lineX.Start.Y
        ]);

        if (A.Determinant() == 0)
        {
            return false;
        }

        var x = A.Solve(b);
        var solutionX = lineX.Start + (long)Math.Round(x[0]) * lineX.Step;
        var solutionY = lineY.Start + (long)Math.Round(x[1]) * lineY.Step;

        if (solutionX != solutionY)
        {
            return false;
        }

        solution = solutionX;
        return true;
    }

    static bool TryGetDiophantineSolutions(long a, long b, long c, [NotNullWhen(true)] out Line? line)
    {
        line = null;

        /**
         * Compute GCD using the extended Euclidean algorithm. x and y are the
         * solutions to the following equation (Bezout's identity):
         *
         *      a * x + b * y = gcd(a, b)
         */

        long gcd = Euclid.ExtendedGreatestCommonDivisor(a, b, out var x, out var y);

        // No solutions exist if c is not divisible by gcd(a, b)
        if (c % gcd != 0)
        {
            return false;
        }

        /*            
         * Scale the extended Euclidean solution with c / gcd to get
         * _one_ solution (x0, y0) for the original Diophantine equation:
         *
         *      a * x * c/gcd + b * y * c/gcd = c
         *          ----.----       ----.----
         *              x0              y0
         */

        var x0 = x * c / gcd;
        var y0 = y * c / gcd;

        /**
         * _all_ solutions are then given by:
         *
         *      x = x0 + k *  b/gcd
         *      y = y0 + k * -a/gcd
         */

        line = new Line(new Vector2(x0, y0), new Vector2(b / gcd, -a / gcd));
        return true;
    }

    static List<Machine> ParseInput(string input)
    {
        var lines = input.Split("\n", StringSplitOptions.RemoveEmptyEntries);
        var machines = new List<Machine>();

        for (int i = 0; i < lines.Length; i += 4)
        {
            var a = ParseButton(lines[i], 3);
            var b = ParseButton(lines[i + 1], 1);
            var prize = ParsePrize(lines[i + 2]);

            machines.Add(new Machine(a, b, prize));
        }

        return machines;
    }

    static Button ParseButton(string line, int cost)
    {
        var match = Regex.Match(line, @"X\+(?<x>\d+), Y\+(?<y>\d+)");
        return new Button(int.Parse(match.Groups["x"].Value), int.Parse(match.Groups["y"].Value), cost);
    }

    static Vector2 ParsePrize(string line)
    {
        var match = Regex.Match(line, @"X=(?<x>\d+),\sY=(?<y>\d+)");
        return new Vector2(int.Parse(match.Groups["x"].Value), int.Parse(match.Groups["y"].Value));
    }
}
