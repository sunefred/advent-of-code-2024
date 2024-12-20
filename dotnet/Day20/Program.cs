using System.ComponentModel.DataAnnotations;

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
        var lines = File.ReadLines("Data/sample.txt");
        var grid = lines.Select(l => l.ToCharArray()).ToArray();

        // Console.WriteLine(Part1(grid));
        Console.WriteLine(Part2(grid));
    }

    static int Part1(char[][] grid)
    {
        var start = FindTheThing(grid, 'S');
        var end = FindTheThing(grid, 'E');
        var nodes = NodesFromGrid(grid);
        var outgoingEdges = GetOutgoingEdges(nodes);   
        var costs = Dijkstra(nodes, outgoingEdges, start);

        List<(Vector2 From, Vector2 To, int Savings)> cheats = [];
        var requiredSavings = 1;

        foreach (var from in nodes)
        {
            foreach (var to in nodes)
            {
                var d = to - from;
                var savings = costs[to] - costs[from] - 2;
                var manhattan = Math.Abs(d.X) + Math.Abs(d.Y);

                if (manhattan == 2 && savings >= requiredSavings)
                {
                    cheats.Add((from, to, savings));
                }
            }
        }

        foreach (var group in cheats.GroupBy(c => c.Savings).OrderBy(g => g.Key))
        {
            Console.WriteLine($"There are {group.Count()} cheats with savings of {group.Key}");
        }

        PrintGridWithCosts(grid, costs);

        return cheats.Count;
    }

    static int Part2(char[][] grid)
    {
        var start = FindTheThing(grid, 'S');
        var end = FindTheThing(grid, 'E');
        var nodes = NodesFromGrid(grid);
        var outgoingEdges = GetOutgoingEdges(nodes);   
        var costs = Dijkstra(nodes, outgoingEdges, start);

        List<(Vector2 From, Vector2 To, int Savings)> cheats = [];
        var requiredSavings = 10;

        var from1 = new Vector2(7, 1);
        var to1 = new Vector2(1, 3);
        var savings1 = costs[from1] - costs[to1] - 2;

        foreach (var from in nodes)
        {
            foreach (var to in nodes)
            {
                var d = to - from;
                var savings = costs[to] - costs[from] - 2;
                var manhattan = Math.Abs(d.X) + Math.Abs(d.Y);

                if (manhattan <= 20 && savings >= requiredSavings)
                {
                    cheats.Add((from, to, savings));
                }
            }
        }
        
        foreach (var cheat in cheats.Where(c => c.Savings == 10))
        {
            Console.WriteLine($"{cheat.From} -> {cheat.To} ({cheat.Savings})");
        }   

        foreach (var group in cheats.GroupBy(c => c.Savings).OrderBy(g => g.Key))
        {
            Console.WriteLine($"There are {group.Count()} cheats with savings of {group.Key}");
        }

        PrintGridWithCosts(grid, costs);

        return cheats.Count;
    }

    static Dictionary<Vector2, int> Dijkstra(HashSet<Vector2> nodes, ILookup<Vector2, Edge> outgoingEdges, Vector2 start)
    {
        /**
         * This is Dijkstra's algorithm. Given a starting point, find the lowest "cost" to reach each
         * node in the graph.
         *
         * Initialize:
         * ~~~~~~~~~~~~
         * - Create a "cost" table to hold the optimum for each node
         * - Init the cost of the start node to 0, others to infinity
         * - Create a priority queue "unvisited" to hold a working set of nodes to visit
         * - Init the queue with the start node
         *
         * Loop:
         * ~~~~~
         * - Pop the node with the smallest cost from the unvisited set
         * - For each neighbor of the current node:
         *   - Calculate cost through the current node
         *   - Update cost of neighbor if it is smaller than current cost
         *   - Add neighbor to unvisited set
         * - Repeat until unvisited set is empty
         *
         * The algorithm has been modified such that it also keeps track the paths to each node.
         * that is associated with the cost. There can be multiple paths to a node with the
         * same cost - all are kept.
         */

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

    static Vector2 FindTheThing(char[][] grid, char thing)
    {
        for (int y = 0; y < grid.Length; y++)
        {
            for (int x = 0; x < grid[y].Length; x++)
            {
                if (grid[y][x] == thing)
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