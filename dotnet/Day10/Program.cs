namespace Day10;

record class Coordinate(int X, int Y);
record class Node(Coordinate Coordinate, int Height, List<Node> Neighbors);

internal class Program
{
    static void Main()
    {
        /**
         * https://adventofcode.com/2024/day/10
         *
         */

        var lines = File.ReadAllLines("Data/sample.txt");
        var heightMap = lines.Select(line => line.Select(c => c - '0').ToArray()).ToArray();

        Console.WriteLine(Part1(heightMap));
        Console.WriteLine(Part2(heightMap));
    }

    static int Part1(int[][] heightMap)
    {
        var totalScore = 0;

        var graph = GraphFromHeightMap(heightMap);
        var trailHeads = graph.Values.Where(node => node.Height == 0).ToList();

        foreach (var trailHead in trailHeads)
        {
            var endNodes = GetReachableNineHeightNodes(trailHead);
            var score = endNodes.Distinct().Count();
            totalScore += score;
        }

        return totalScore;
    }

    static int Part2(int[][] heightMap)
    {
        var totalRating = 0;

        var graph = GraphFromHeightMap(heightMap);
        var trailHeads = graph.Values.Where(node => node.Height == 0).ToList();

        foreach (var trailHead in trailHeads)
        {
            var endNodes = GetReachableNineHeightNodes(trailHead);
            totalRating += endNodes.Count();
        }

        return totalRating;
    }

    static Dictionary<Coordinate, Node> GraphFromHeightMap(int[][] heightMap)
    {
        var graph = new Dictionary<Coordinate, Node>();
        var directions = new[] { (-1, 0), (1, 0), (0, -1), (0, 1) };
        var rows = heightMap.Length;
        var cols = heightMap[0].Length;

        // Add each node
        for (var y = 0; y < rows; y++)
        {
            for (var x = 0; x < cols; x++)
            {
                var height = heightMap[y][x];
                var coordinate = new Coordinate(x, y);
                var node = new Node(coordinate, height, []);

                graph[coordinate] = node;
            }
        }

        // Connect each node to its neighbors
        foreach (var node in graph.Values)
        {
            foreach (var (dx, dy) in directions)
            {
                var (x, y) = node.Coordinate;
                var nx = x + dx;
                var ny = y + dy;

                if (nx < 0 || nx >= cols || ny < 0 || ny >= rows)
                {
                    continue;
                }

                var coordinate = new Coordinate(nx, ny);
                var neighbor = graph[coordinate];
                
                if (neighbor.Height == node.Height + 1)
                {
                    node.Neighbors.Add(neighbor);
                }
            }
        }

        return graph;
    }

    static IEnumerable<Node> GetReachableNineHeightNodes(Node root)
    {
        var stack = new Stack<Node>();

        stack.Push(root);

        while (stack.Count > 0)
        {
            var currentNode = stack.Pop();

            if (currentNode.Height == 9)
            {
                yield return currentNode;
            }

            foreach (var neighbor in currentNode.Neighbors)
            {
                stack.Push(neighbor);
            }
        }
    }
}
