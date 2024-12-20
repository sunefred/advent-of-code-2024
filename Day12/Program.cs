using System.Diagnostics.CodeAnalysis;

namespace Day12;

internal class Program
{
    record class Vector2(long X, long Y)
    {
        public static Vector2 operator +(Vector2 v, Vector2 u) => new(v.X + u.X, v.Y + u.Y);
        public static Vector2 operator -(Vector2 v, Vector2 u) => new(v.X - u.X, v.Y - u.Y);
        public static Vector2 operator *(Vector2 v, long s) => new(v.X * s, v.Y * s);
        public static Vector2 operator *(long s, Vector2 v) => new(v.X * s, v.Y * s);
        public static Vector2 operator /(Vector2 v, long s) => new(v.X / s, v.Y / s);
    }

    record class Node(Vector2 Position, Vector2[] Neighbors);

    enum CornerType
    {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight,
    }

    static readonly Vector2[] Directions =
    [
        new(0, -1),
        new(0, 1),
        new(-1, 0),
        new(1, 0),
    ];

    static void Main()
    {
        /**
         * https://adventofcode.com/2024/day/12
         * 
         */

        var lines = File.ReadAllLines("Data/sample3.txt");
        var map = lines.Select(line => line.ToCharArray()).ToArray();
        var positions = map.SelectMany((row, y) => row.Select((_, x) => new Vector2(x, y))).ToArray();
        PrintGarden(map, positions);
        Console.WriteLine();

        Console.WriteLine(Part1(map));
        Console.WriteLine(Part2(map));
    }

    static int Part1(char[][] map)
    {
        HashSet<Vector2> visited = [];
        var totalPrice = 0;

        while (true)
        {
            if (!TryGetFirstUnvisitedPosition(map, visited, out var pos))
            {
                break;
            }

            var plant = map[pos.Y][pos.X];
            var region = GetRegionWithSamePlant(map, pos);
            var area = CalculateAreaForRegion(region);
            var perimeter = CalculatePerimeterForRegion(region);
            var price = area * perimeter;

            PrintGarden(map, region);
            Console.WriteLine($"Plant: {plant}, Area: {area}, Perimeter: {perimeter}, Price: {price}");
            Console.WriteLine();

            foreach (var position in region)
            {
                visited.Add(position);
            }

            totalPrice += price;
        }

        return totalPrice;
    }

    static int Part2(char[][] map)
    {
        HashSet<Vector2> visited = [];
        var totalPrice = 0;

        while (true)
        {
            if (!TryGetFirstUnvisitedPosition(map, visited, out var pos))
            {
                break;
            }

            var plant = map[pos.Y][pos.X];
            var region = GetRegionWithSamePlant(map, pos);
            var area = CalculateAreaForRegion(region);
            var corners = CalculateCornersForRegion(region);
            var price = area * corners;

            PrintGarden(map, region);
            Console.WriteLine($"Plant: {plant}, Area: {area}, Corners/sides: {corners}, Price: {price}");
            Console.WriteLine();

            foreach (var position in region)
            {
                visited.Add(position);
            }

            totalPrice += price;
        }

        return totalPrice;
    }

    static int CalculateAreaForRegion(Vector2[] region)
    {
        return region.Length;
    }

    static int CalculatePerimeterForRegion(Vector2[] region)
    {
        /**
         * Basic algorithm:
         *
         * For each position in the region, check the neighbors. If a neighbor is
         * outside the region, increment the sides counter.
         *
         * This will include the sides for internal plots that are next to a hole.
         */

        int sides = 0;

        foreach (var position in region)
        {
            foreach (var direction in Directions)
            {
                var neighbor = position + direction;
                if (!region.Contains(neighbor))
                {
                    sides++;
                }
            }
        }

        return sides;
    }

    static int CalculateCornersForRegion(Vector2[] region)
    {
        /**
         * Basic algorithm:
         *
         * Calculate corner positions for each plot (a 1x1 area of land) in the region.
         * Some corners will be shared between plots, some wont. Counting the number of times
         * a corner is shared determine if the corner is an outside corner or not.
         *
         *      x--------
         *      |###|###           Shared 1 time => +1 corner
         *      |---|----
         *      |###|###
         *
         *       ---x---
         *       ###|###           Shared 2 times => not a corner
         *       ---|---
         *       ###|###
         *
         *     ¤¤###|#####           
         *     -|---|---|-
         *     #|   |###|#         Shared 2 times on diagonal => +2 corners
         *     -|---x---|-
         *     #|###|   |#
         *     -|---|---|-
         *     #####|##### 
         *
         *       --------
         *       ###|###           Shared 3 times => +1 corner
         *       ---x----
         *          |###
         *
         *
         *       ###|###           
         *       ---x----          Shared 4 times => not a corner
         *       ###|###
         */

        var plotCorners = region
            .SelectMany(plot =>
            {
                var topLeft = (plot, CornerType.TopLeft);
                var topRight = (plot + new Vector2(1, 0), CornerType.TopRight);
                var bottomLeft = (plot + new Vector2(0, 1), CornerType.BottomLeft);
                var bottomRight = (plot + new Vector2(1, 1), CornerType.BottomRight);
                return new List<(Vector2 Position, CornerType Type)>() { topLeft, topRight, bottomLeft, bottomRight };
            });

        var cornerCount = plotCorners
            .GroupBy(corner => corner.Position)
            .Select(group =>
            {
                // Shared 1 time => +1 corner
                if (group.Count() == 1)
                {
                    return 1;
                }

                // Shared 2 times on diagonal => +2 corners
                else if (group.Count() == 2)
                {
                    var type1 = group.First().Type;
                    var type2 = group.Last().Type;

                    if (type1 == CornerType.TopLeft && type2 == CornerType.BottomRight ||
                        type1 == CornerType.BottomRight && type2 == CornerType.TopLeft ||
                        type1 == CornerType.TopRight && type2 == CornerType.BottomLeft ||
                        type1 == CornerType.BottomLeft && type2 == CornerType.TopRight)
                    {
                        return 2;
                    }
                }

                // Shared 3 times => +1 corner
                else if (group.Count() == 3)
                {
                    return 1;
                }

                return 0;
            })
            .Sum();

        return cornerCount;
    }

    static Vector2[] GetRegionWithSamePlant(char[][] map, Vector2 pos)
    {
        var plant = map[pos.Y][pos.X];
        var visited = new List<Vector2>();
        var stack = new Stack<Vector2>();
        stack.Push(pos);

        while (stack.Count != 0)
        {
            var current = stack.Pop();
            visited.Add(current);

            foreach (var direction in Directions)
            {
                var neighbor = current + direction;

                if (!InBounds(map, neighbor))
                {
                    continue;
                }

                if (visited.Contains(neighbor))
                {
                    continue;
                }

                if (map[neighbor.Y][neighbor.X] != plant)
                {
                    continue;
                }

                stack.Push(neighbor);
            }
        }

        return [.. visited.Distinct()];
    }

    static bool InBounds(char[][] map, Vector2 pos)
    {
        var rows = map.Length;
        var cols = map[0].Length;
        return pos.X >= 0 && pos.X < cols && pos.Y >= 0 && pos.Y < rows;
    }


    static bool TryGetFirstUnvisitedPosition(char[][] map, HashSet<Vector2> visited, [NotNullWhen(true)] out Vector2? result)
    {
        result = null;

        for (int y = 0; y < map.Length; y++)
        {
            for (int x = 0; x < map[y].Length; x++)
            {
                var candidate = new Vector2(x, y);
                if (visited.Contains(candidate))
                {
                    continue;
                }

                result = candidate;
                return true;
            }
        }

        return false;
    }

    static void PrintGarden(char[][] map, Vector2[] area)
    {
        for (int y = 0; y < map.Length; y++)
        {
            Console.Write($"{y:D3} ");

            for (int x = 0; x < map[y].Length; x++)
            {
                var pos = new Vector2(x, y);
                if (area.Contains(pos))
                {
                    Console.Write(map[y][x]);
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
