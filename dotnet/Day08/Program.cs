using System.Numerics;

namespace Day08;

record class Vector2(long X, long Y)
{
    public static Vector2 operator +(Vector2 v, Vector2 u) => new(v.X + u.X, v.Y + u.Y);
    public static Vector2 operator -(Vector2 v, Vector2 u) => new(v.X - u.X, v.Y - u.Y);
    public static Vector2 operator *(Vector2 v, long s) => new(v.X * s, v.Y * s);
    public static Vector2 operator *(long s, Vector2 v) => new(v.X * s, v.Y * s);
    public static Vector2 operator /(Vector2 v, long s) => new(v.X / s, v.Y / s);
}

internal class Program
{
    static void Main()
    {
        /**
         * https://adventofcode.com/2024/day/8
         * 
         */

        var lines = File.ReadAllLines("Data/input.txt");
        var map = lines.Select(line => line.ToCharArray()).ToArray();

        Console.WriteLine(Part1(map));
        Console.WriteLine(Part2(map));
    }

    static int Part1(char[][] map)
    {
        List<Vector2> antidotePositions = [];
        var antennas = GetAntennas(map);

        foreach (var antenna in antennas)
        {
            var positions = antenna.Value;
            foreach (var (pos1, i) in positions.Select((p, i) => (p, i)))
            {
                foreach (var pos2 in positions.Skip(i + 1))
                {
                    var delta = pos2 - pos1;
                    var candidate1 = pos1 - delta;
                    if (IsInBounds(map, candidate1))
                    {
                        antidotePositions.Add(candidate1);
                    }

                    var candidate2 = pos2 + delta;
                    if (IsInBounds(map, candidate2))
                    {
                        antidotePositions.Add(candidate2);
                    }
                }
            }
        }

        PrintMap(map, antidotePositions);
        return antidotePositions.Distinct().Count();
    }

    static int Part2(char[][] map)
    {
        List<Vector2> antidotePositions = [];
        var antennas = GetAntennas(map);

        foreach (var antenna in antennas)
        {
            var freq = antenna.Key;
            var positions = antenna.Value;

            foreach (var (pos1, i) in positions.Select((p, i) => (p, i)))
            {
                foreach (var pos2 in positions.Skip(i + 1))
                {
                    var delta = pos2 - pos1;
                    int gcd = (int)BigInteger.GreatestCommonDivisor(delta.X, delta.Y);
                    var normalizedDelta = delta / gcd;

                    // Cheat, we know the answer is within 50 units
                    for (int k = -50; k < 50; k++)
                    {
                        var candidate = pos1 + k * normalizedDelta;
                        if (IsInBounds(map, candidate))
                        {
                            antidotePositions.Add(candidate);
                        }
                    }
                }
            }
        }

        PrintMap(map, antidotePositions);
        return antidotePositions.Distinct().Count();
    }

    static Dictionary<char, List<Vector2>> GetAntennas(char[][] map)
    {
        var antennas = new Dictionary<char, List<Vector2>>();

        var cols = map[0].Length;
        var rows = map.Length;

        for (var y = 0; y < rows; y++)
        {
            for (var x = 0; x < cols; x++)
            {
                var freq = map[y][x];
                if (freq == '.')
                {
                    continue;
                }

                if (!antennas.ContainsKey(freq))
                {
                    antennas[freq] = [];
                }

                antennas[freq].Add(new Vector2(x, y));
            }
        }

        return antennas;
    }

    static bool IsInBounds(char[][] map, Vector2 pos)
    {
        var cols = map[0].Length;
        var rows = map.Length;
        return 0 <= pos.X && pos.X < cols && 0 <= pos.Y && pos.Y < rows;
    }

    static void PrintMap(char[][] map, List<Vector2> antidotePositions)
    {
        var rows = map.Length;
        var cols = map[0].Length;

        for (var y = 0; y < rows; y++)
        {
            for (var x = 0; x < cols; x++)
            {
                var pos = new Vector2(x, y);
                if (map[y][x] == '.' && antidotePositions.Contains(pos))
                {
                    Console.Write('#');
                }
                else
                {
                    Console.Write(map[y][x]);
                }
            }

            Console.WriteLine();
        }
    }
}
