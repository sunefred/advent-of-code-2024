namespace Day16;

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

    record class Node(Vector2 Position, Vector2 Direction);
    record class Edge(Node From, Node To, int Cost);
    record class Optimum(int Cost, Edge[][] Paths);

    static readonly Vector2[] Directions = [
        new(0, -1), // North
        new(1, 0), // East
        new(0, 1), // South
        new(-1, 0), // West
    ];

    static void Main()
    {
        /**
         * https://adventofcode.com/2024/day/16
         * 
         */

        var lines = File.ReadLines("Data/sample2.txt");
        var grid = lines.Select(l => l.ToCharArray()).ToArray();

        Console.WriteLine(Part1(grid));
        Console.WriteLine();
        Console.WriteLine(Part2(grid));
    }

    static long Part1(char[][] grid)
    {
        var startPos = FindTheThing(grid, 'S');
        var endPos = FindTheThing(grid, 'E');
        var start = new Node(startPos, Directions[1]);

        var nodes = NodesFromGrid(grid);
        var outgoingEdges = GetOutgoingEdges(nodes);
        var costs = Dijkstra(nodes, outgoingEdges, start);

        var pathToEnd = costs
            .Where(kvp => kvp.Key.Position == endPos)
            .MinBy(kvp => kvp.Value.Cost)
            .Value.Paths.First();

        var costPerPos = pathToEnd
            .Select(e => e.From)
            .DistinctBy(n => n.Position)
            .ToDictionary(n => n.Position, n => costs[n].Cost);

        PrintGridWithCosts(grid, costPerPos);

        // There can be more than one because last position is
        // represented by four nodes (one for each direction)
        return costs
            .Where(kvp => kvp.Key.Position == endPos)
            .Select(kvp => kvp.Value.Cost)
            .Min();
    }

    static long Part2(char[][] grid)
    {
        var startPos = FindTheThing(grid, 'S');
        var endPos = FindTheThing(grid, 'E');
        var start = new Node(startPos, Directions[1]);

        var nodes = NodesFromGrid(grid);
        var outgoingEdges = GetOutgoingEdges(nodes);
        var costs = Dijkstra(nodes, outgoingEdges, start);

        var pathsToEnd = costs
            .Where(kvp => kvp.Key.Position == endPos)
            .MinBy(kvp => kvp.Value.Cost)
            .Value.Paths;

        var costPerPos = pathsToEnd
            .SelectMany(e => e)
            .Select(e => e.From)
            .DistinctBy(n => n.Position)
            .ToDictionary(n => n.Position, n => costs[n].Cost);

        PrintGridWithCosts(grid, costPerPos);
        return costPerPos.Count + 1;
    }

    static Dictionary<Node, Optimum> Dijkstra(HashSet<Node> nodes, ILookup<Node, Edge> outgoingEdges, Node start)
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
         *
         * Fork
         * ~~~~
         * - A path is "forked" if there are multiple paths to a node with the same cost.
         * - Fork means that all of the paths for the current node are added to the neighbor.
         */

        var costs = nodes.ToDictionary(n => n, n => new Optimum(int.MaxValue, []));
        costs[start] = new Optimum(0, [[]]);

        var unvisited = new PriorityQueue<Node, int>();
        unvisited.Enqueue(start, 0);

        while (unvisited.TryDequeue(out var current, out var costToCurrent))
        {
            var edgesFromCur = outgoingEdges[current];
            foreach (var edge in edgesFromCur)
            {
                var neighbor = edge.To;
                var costThroughCurrent = costs[current].Cost + edge.Cost;
                var pathsThroughCurrent = costs[current].Paths.Select(p => p.Append(edge).ToArray());
                var costToNeighbor = costs[neighbor].Cost;

                // No Fork: Update cost of neighbor, update its paths as well
                if (costThroughCurrent < costToNeighbor)
                {
                    unvisited.Enqueue(edge.To, costThroughCurrent);
                    costs[neighbor] = new Optimum(costThroughCurrent, [.. pathsThroughCurrent]);
                }

                // Yes Fork: Append paths going through current node, to paths in neighbor
                else if (costThroughCurrent == costToNeighbor)
                {
                    costs[neighbor] = new Optimum(costThroughCurrent, [.. costs[neighbor].Paths, .. pathsThroughCurrent]);
                }
            }
        }
        return costs;
    }

    static HashSet<Node> NodesFromGrid(char[][] grid)
    {
        /**
         * Creates four nodes for each cell (not wall), one node for each
         * direction to represent all possible states of the player.
         *
         */

        var height = grid.Length;
        var width = grid[0].Length;
        var nodes = new HashSet<Node>();

        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                if (grid[y][x] == '#')
                {
                    continue;
                }

                foreach (var direction in Directions)
                {
                    nodes.Add(new Node(new Vector2(x, y), direction));
                }
            }
        }
        return nodes;
    }

    static ILookup<Node, Edge> GetOutgoingEdges(HashSet<Node> nodes)
    {
        /**
         * Only moves allowed are:
         * - Move forward (cost 1)
         * - Turn clockwise (cost 1000)
         * - Turn counterclockwise (cost 1000)
         */

        var edges = new List<Edge>();

        foreach (var node in nodes)
        {
            var pos = node.Position;
            var dir = node.Direction;

            // Turn CCW
            var ccwPos = pos;
            var ccwDir = new Vector2(-dir.Y, dir.X);
            if (nodes.Contains(new Node(ccwPos, ccwDir)))
            {
                edges.Add(new Edge(node, new Node(ccwPos, ccwDir), 1000));
            }

            // Turn CW
            var cwPos = pos;
            var cwDir = new Vector2(dir.Y, -dir.X);
            if (nodes.Contains(new Node(cwPos, cwDir)))
            {
                edges.Add(new Edge(node, new Node(cwPos, cwDir), 1000));
            }

            // Move forward
            var fwdPos = pos + dir;
            var fwdDir = dir;
            if (nodes.Contains(new Node(fwdPos, fwdDir)))
            {
                edges.Add(new Edge(node, new Node(fwdPos, fwdDir), 1));
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

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var pos = new Vector2(x, y);

                if (costs.TryGetValue(pos, out var cost))
                {
                    Console.Write($" {cost:D5} ");
                }
                else
                {
                    Console.Write(new string(grid[y][x], 7));
                }
            }

            Console.WriteLine();
        }
    }
}
