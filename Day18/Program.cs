using System.Diagnostics;
using System.Text.RegularExpressions;

namespace Day18;

internal class Program
{
    record class Vector2(int X, int Y)
    {
        public static Vector2 Up => new(0, -1);
        public static Vector2 Down => new(0, 1);
        public static Vector2 Left => new(-1, 0);
        public static Vector2 Right => new(1, 0);
        public static Vector2 operator +(Vector2 u, Vector2 v) => new(u.X + v.X, u.Y + v.Y);
        public static Vector2 operator -(Vector2 u, Vector2 v) => new(u.X - v.X, u.Y - v.Y);
        public static Vector2 operator *(Vector2 v, int s) => new(v.X * s, v.Y * s);
        public static Vector2 operator *(int s, Vector2 v) => new(v.X * s, v.Y * s);
        public static Vector2 operator /(Vector2 v, int s) => new(v.X / s, v.Y / s);
    }

    record class Edge(Vector2 From, Vector2 To, int Cost);

    static void Main()
    {
        /**
         * https://adventofcode.com/2024/day/18
         * 
         *                      Sample  Input
         *                      -----   ------ 
         * Grid size:           7x7     71x71 (0..70)
         * take/startAt:        12      1024
         *
         */

        int width = 71;
        int height = 71;
        int take = 1024;

        var input = File.ReadAllText("Data/input.txt");
        var corrupted = ReadCorruptedCoords(input);

        var sw1 = Stopwatch.StartNew();
        var part1 = Part1(width, height, [.. corrupted.Take(take)]);
        sw1.Stop();
        Console.WriteLine($"Part 1: {part1}, Elapsed: {sw1.ElapsedMilliseconds}ms");

        var sw2 = Stopwatch.StartNew();
        var part2 = Part2(width, height, corrupted, take);
        sw2.Stop();
        Console.WriteLine($"Part 2: {part2}, Elapsed: {sw2.ElapsedMilliseconds}ms");
    }

    static int Part1(int width, int height, Vector2[] corrupted)
    {
        var start = new Vector2(0, 0);
        var exit = new Vector2(width - 1, height - 1);

        var path = PathToExit(start, exit, corrupted);
        var nodes = path.Select(e => e.To).ToArray();

        PrintMemory(width, height, [.. corrupted], nodes);

        return path.Length;
    }

    static Vector2 Part2(int width, int height, Vector2[] corrupted, int startAt)
    {
        /**
         * Finds the first "falling" byte in the corrupts the memory such
         * that no path from the start to the exit exists.
         *
         * We do this simply by increasing the number of corrupted bytes
         * for each iteration and check if a path exists.
         *
         * This is very slow. A better approach would be to use a binary
         * search algorithm to find the first corrupted byte that breaks
         * the path.
         */

        var start = new Vector2(0, 0);
        var exit = new Vector2(width - 1, height - 1);

        for (int i = startAt; i < corrupted.Length; i++)
        {
            var corrupted1 = corrupted.Take(i).ToArray();
            var path = PathToExit(start, exit, corrupted1);
            var nodes = path.Select(e => e.To).ToArray();

            Console.WriteLine($"Byte: {i}, Length: {path.Length}, Corrupted: {corrupted1.Last()}");

            if (path.Length == 0)
            {
                return corrupted1.Last();
            }
        }

        throw new InvalidOperationException("No solution found");
    }

    static Edge[] PathToExit(Vector2 start, Vector2 exit, Vector2[] corrupted)
    {
        var nodes = NodesFromGrid(exit.X + 1, exit.Y +1, corrupted);
        var outgoingEdges = OutgoingEdges(nodes);

        var best = Dijkstra(nodes, outgoingEdges, start);
        var bestForExit = best[exit];

        return bestForExit.Path;
    }

    static Dictionary<Vector2, (int Cost, Edge[] Path)> Dijkstra(HashSet<Vector2> nodes, ILookup<Vector2, Edge> outgoingEdges, Vector2 start)
    {
        var best = nodes.ToDictionary(n => n, n => (Cost: int.MaxValue, Path: Array.Empty<Edge>()));
        best[start] = (0, []);

        var unvisited = new PriorityQueue<Vector2, int>();
        unvisited.Enqueue(start, 0);

        while (unvisited.TryDequeue(out var current, out var costToCurrent))
        {
            var edgesFromCur = outgoingEdges[current];
            foreach (var edge in edgesFromCur.Reverse())
            {
                var neighbor = edge.To;
                var costThroughCurrent = best[current].Cost + edge.Cost;
                Edge[] pathThroughCurrent = [.. best[current].Path, edge];
                var costToNeighbor = best[neighbor].Cost;

                if (costThroughCurrent < costToNeighbor)
                {
                    unvisited.Enqueue(edge.To, costThroughCurrent);
                    best[neighbor] = (costThroughCurrent, pathThroughCurrent);
                }
            }
        }

        return best;
    }

    static HashSet<Vector2> NodesFromGrid(int width, int height, Vector2[] corrupted)
    {
        var nodes = new HashSet<Vector2>();

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                if (corrupted.Contains(new Vector2(x, y)))
                {
                    continue;
                }

                nodes.Add(new Vector2(x, y));
            }
        }

        return nodes;
    }

    static ILookup<Vector2, Edge> OutgoingEdges(HashSet<Vector2> nodes)
    {
        Vector2[] directions = [Vector2.Up, Vector2.Down, Vector2.Left, Vector2.Right];
        var edges = new List<Edge>();

        foreach (var node in nodes)
        {
            foreach (var direction in directions)
            {
                var test = node + direction;
                if (nodes.TryGetValue(test, out var neighbor))
                {
                    edges.Add(new Edge(node, neighbor, 1));
                }
            }
        }

        return edges.ToLookup(e => e.From);
    }

    static Vector2[] ReadCorruptedCoords(string input)
    {
        var matches = Regex.Matches(input, @"(\d+),(\d+)");
        var coords = matches
            .Select(m => new Vector2(int.Parse(m.Groups[1].Value), int.Parse(m.Groups[2].Value)))
            .ToArray();

        return coords;
    }

    static void PrintMemory(int width, int height, Vector2[] corrupted, Vector2[] path)
    {
        for (var y = 0; y < height; y++)
        {
            Console.Write($"{y:D2} ");
            for (var x = 0; x < width; x++)
            {
                var pos = new Vector2(x, y);

                if (corrupted.Contains(pos))
                {
                    Console.Write("#");
                }
                else if (path.Contains(pos))
                {
                    Console.Write("O");
                }
                else
                {
                    Console.Write(".");
                }
            }
            Console.WriteLine();
        }
    }
}
