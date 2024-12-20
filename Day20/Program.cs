namespace Day20;

internal class Program
{
    record class Vector2(int X, int Y)
    {
        public static Vector2 operator +(Vector2 p, Vector2 v) => new(p.X + v.X, p.Y + v.Y);
        public static Vector2 operator -(Vector2 p, Vector2 v) => new(p.X - v.X, p.Y - v.Y);
        public static Vector2 operator *(Vector2 v, int s) => new(v.X * s, v.Y * s);
        public static Vector2 operator *(int s, Vector2 v) => new(v.X * s, v.Y * s);
        public static Vector2 operator /(Vector2 v, int s) => new(v.X / s, v.Y / s);
    }

    record class Edge(Vector2 From, Vector2 To, int Cost);

    static readonly Vector2[] Directions = [
        new(0, -1), // Up
        new(0, 1), // Down
        new(-1, 0), // Left
        new(1, 0), // Right
    ];

    static void Main()
    {
        /**
         * https://adventofcode.com/2024/day/20
         * 
         */

        /**
         * Settings for max cheat length and required savings
         *
         *              sample.txt  input.txt
         *              ----------  ----------
         * Part 1:       2/1         2/100
         * Part 2:      20/50       20/100
         */

        var maxCheatLength = 20;
        var requiredSavings = 50;

        var lines = File.ReadLines("Data/sample.txt");
        var grid = lines.Select(l => l.ToCharArray()).ToArray();

        Console.WriteLine(Part1And2(grid, maxCheatLength, requiredSavings));
    }

    static int Part1And2(char[][] grid, int maxCheatLength, int requiredSavings)
    {
        var start = FindStart(grid);
        var nodes = NodesFromGrid(grid);
        var outgoingEdges = GetOutgoingEdges(nodes);
        var costs = Dijkstra(nodes, outgoingEdges, start);
        var cheats = FindCheats(nodes, costs, maxCheatLength, requiredSavings);

        foreach (var group in cheats.GroupBy(c => c.Savings).OrderBy(g => g.Key))
        {
            Console.WriteLine($"There are {group.Count()} cheats with savings of {group.Key}");
        }

        PrintGridWithCosts(grid, costs);

        Console.WriteLine($"There are {cheats.Count} cheats, cheat length is {maxCheatLength}, required savings is {requiredSavings}");
        return cheats.Count;
    }

    static List<(Vector2 From, Vector2 To, int Savings)> FindCheats(HashSet<Vector2> nodes, Dictionary<Vector2, int> costs, int maxCheatLength, int requiredSavings)
    {
        var cheats = new List<(Vector2 From, Vector2 To, int Savings)>();

        foreach (var from in nodes)
        {
            foreach (var to in nodes)
            {
                var d = to - from;
                var manhattan = Math.Abs(d.X) + Math.Abs(d.Y);
                var savings = costs[to] - costs[from] - manhattan;

                if (manhattan <= maxCheatLength && savings >= requiredSavings)
                {
                    cheats.Add((from, to, savings));
                }
            }
        }

        return cheats;
    }

    static Dictionary<Vector2, int> Dijkstra(HashSet<Vector2> nodes, ILookup<Vector2, Edge> outgoingEdges, Vector2 start)
    {
        var costs = nodes.ToDictionary(n => n, n => int.MaxValue);
        costs[start] = 0;

        var unvisited = new PriorityQueue<Vector2, int>();
        unvisited.Enqueue(start, 0);

        while (unvisited.TryDequeue(out var current, out var costToCurrent))
        {
            var edgesFromCur = outgoingEdges[current];
            foreach (var edge in edgesFromCur)
            {
                var neighbor = edge.To;
                var costThroughCurrent = costs[current] + edge.Cost;
                var costToNeighbor = costs[neighbor];

                if (costThroughCurrent < costToNeighbor)
                {
                    unvisited.Enqueue(edge.To, costThroughCurrent);
                    costs[neighbor] = costThroughCurrent;
                }
            }
        }

        return costs;
    }

    static HashSet<Vector2> NodesFromGrid(char[][] grid)
    {
        var height = grid.Length;
        var width = grid[0].Length;
        var nodes = new HashSet<Vector2>();

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                if (grid[y][x] == '#')
                {
                    continue;
                }

                nodes.Add(new Vector2(x, y));
            }
        }

        return nodes;
    }

    static ILookup<Vector2, Edge> GetOutgoingEdges(HashSet<Vector2> nodes)
    {
        var edges = new List<Edge>();

        foreach (var node in nodes)
        {
            foreach (var direction in Directions)
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

    static Vector2 FindStart(char[][] grid)
    {
        for (int y = 0; y < grid.Length; y++)
        {
            for (int x = 0; x < grid[y].Length; x++)
            {
                if (grid[y][x] == 'S')
                {
                    return new Vector2(x, y);
                }
            }
        }

        throw new InvalidOperationException();
    }

    static void PrintGridWithCosts(char[][] grid, IDictionary<Vector2, int> costs)
    {
        var height = grid.Length;
        var width = grid[0].Length;

        Console.Write("    ");

        for (int x = 0; x < width; x++)
        {
            Console.Write($" {x:D2} ");
        }

        Console.WriteLine();

        for (int y = 0; y < height; y++)
        {
            Console.Write($" {y:D2} ");

            for (int x = 0; x < width; x++)
            {
                var pos = new Vector2(x, y);

                if (costs.TryGetValue(pos, out var cost))
                {
                    Console.Write($" {cost:D2} ");
                }
                else
                {
                    Console.Write(new string(grid[y][x], 4));
                }
            }

            Console.WriteLine();
        }
    }
}
