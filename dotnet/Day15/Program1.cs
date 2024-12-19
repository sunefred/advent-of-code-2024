using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.VisualBasic;

class Program1
{
    record class Vector2(long X, long Y)
    {
        public static Vector2 operator +(Vector2 p, Vector2 v) => new(p.X + v.X, p.Y + v.Y);
        public static Vector2 operator -(Vector2 p, Vector2 v) => new(p.X - v.X, p.Y - v.Y);
        public static Vector2 operator *(Vector2 v, long s) => new(v.X * s, v.Y * s);
        public static Vector2 operator *(long s, Vector2 v) => new(v.X * s, v.Y * s);
        public static Vector2 operator /(Vector2 v, long s) => new(v.X / s, v.Y / s);
    }

    enum Obstruction
    {
        None,
        Box,
        Wall,
    }

    static readonly Dictionary<string, Obstruction> SymbolToObstruction = new()
    {
        ["."] = Obstruction.None,
        ["O"] = Obstruction.Box,
        ["#"] = Obstruction.Wall,
        ["@"] = Obstruction.None,
    };

    static readonly Dictionary<char, Vector2> SymbolToDirection = new()
    {
        ['^'] = new Vector2(0, -1),
        ['v'] = new Vector2(0, 1),
        ['<'] = new Vector2(-1, 0),
        ['>'] = new Vector2(1, 0),
    };

    static readonly Dictionary<Vector2, char> DirectionToSymbol 
        = SymbolToDirection.ToDictionary(kvp => kvp.Value, kvp => kvp.Key);

    static void Main()
    {
        var input = File.ReadAllText("Data/sample1.txt");

        Console.WriteLine(Part1(input));
        // Console.WriteLine(Part2(input));
    }

    static long Part1(string input)
    {
        var (start, map, movements) = ParseMap(input);
        PrintMap(map, start, null);
        Console.WriteLine();

        var position = start;
        foreach (var (movement, i) in movements.Select((m, i) => (m, i)))
        {
            position = MoveRobot(map, position, movement);

            Console.WriteLine($"{i+1}/{movements.Length}: {DirectionToSymbol[movement]}");
            PrintMap(map, position, movement);
            Console.WriteLine();
            Thread.Sleep(200);
        }

        var sumOfCoordinates = 0;
        for (int y = 0; y < map.Length; y++)
        {
            for (int x = 0; x < map[y].Length; x++)
            {
                if (map[y][x] == Obstruction.Box)
                {
                    var coordinate = 100 * y + x;
                    sumOfCoordinates += coordinate;
                }
            }
        }

        return sumOfCoordinates;
    }

    static Vector2 MoveRobot(Obstruction[][] map, Vector2 position, Vector2 movement)
    {
        var position1 = position + movement;
        var content1 = map[position1.Y][position1.X];

        // Wall => do nothing
        if (content1 == Obstruction.Wall)
        {
            return position;
        }

        // Nothing => free to move
        if (content1 == Obstruction.None)
        {
            return position1;
        }

        var noneFree = Enumerable.Range(0, int.MaxValue)
            .Select(i => position1 + movement * i)
            .TakeWhile(p => InBounds(map, p))
            .Select(p => map[p.Y][p.X])
            .TakeWhile(c => c != Obstruction.None)
            .ToArray();

        // Row of boxes with wall behind => do nothing
        if (content1 == Obstruction.Box && noneFree.Contains(Obstruction.Wall))
        {
            return position;
        }

        // Boxes with one free space behind => move all boxes
        foreach (var (_, i) in noneFree.Select((c, i) => (c, i)))
        {
            var position2 = position1 + (i + 1) * movement;
            map[position2.Y][position2.X] = Obstruction.Box;
        }

        map[position1.Y][position1.X] = Obstruction.None;
        return position1;
    }

    static bool InBounds(Obstruction[][] map, Vector2 position)
    {
        var width = map[0].Length;
        var height = map.Length;

        return position.X >= 0 && position.X < width && position.Y >= 0 && position.Y < height;
    }

    static long Part2(string input)
    {
        return 2;
    }

    static (Vector2 start, Obstruction[][] Map, Vector2[] Movements) ParseMap(string input)
    {
        var rawMap = input.Split($"{Environment.NewLine}{Environment.NewLine}")[0];
        var rawMovements = input.Split($"{Environment.NewLine}{Environment.NewLine}")[1];

        Vector2 start = rawMap
            .Split(Environment.NewLine)
            .Select((line, y) => (line, y))
            .SelectMany(t => t.line.Select((c, x) => (c, x, t.y)))
            .Where(t => t.c == '@')
            .Select(t => new Vector2(t.x, t.y))
            .First();

        var map = rawMap
            .Split(Environment.NewLine)
            .Select(line => line.Select(c => c switch
            {
                '.' => Obstruction.None,
                'O' => Obstruction.Box,
                '#' => Obstruction.Wall,
                '@' => Obstruction.None,
                _ => throw new InvalidOperationException()
            }).ToArray())
            .ToArray();

        var movements = rawMovements
            .Split(Environment.NewLine)
            .SelectMany(line => line.Select(c => c switch
            {
                '^' => new Vector2(0, -1),
                'v' => new Vector2(0, 1),
                '<' => new Vector2(-1, 0),
                '>' => new Vector2(1, 0),
                _ => throw new InvalidOperationException()
            }).ToArray())
            .ToArray();

        return (start, map, movements);
    }

    static void PrintMap(Obstruction[][] map, Vector2 position, Vector2? movement)
    {
        var height = map.Length;
        var width = map[0].Length;

        for (int y = 0; y < height; y++)
        {
            Console.Write($"{y:D3} ");

            for (int x = 0; x < width; x++)
            {
                if (position.X == x && position.Y == y)
                {
                    Console.Write('@');
                }
                else
                {
                    Console.Write(map[y][x] switch
                    {
                        Obstruction.None => '.',
                        Obstruction.Box => 'O',
                        Obstruction.Wall => '#',
                        _ => throw new InvalidOperationException()
                    });
                }
            }
            Console.WriteLine();
        }
    }
}