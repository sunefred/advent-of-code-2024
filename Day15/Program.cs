using System.Numerics;

namespace Day15;

internal class Program
{

    record class Vector2(long X, long Y)
    {
        public static Vector2 operator +(Vector2 p, Vector2 v) => new(p.X + v.X, p.Y + v.Y);
        public static Vector2 operator -(Vector2 p, Vector2 v) => new(p.X - v.X, p.Y - v.Y);
        public static Vector2 operator *(Vector2 v, long s) => new(v.X * s, v.Y * s);
        public static Vector2 operator *(long s, Vector2 v) => new(v.X * s, v.Y * s);
        public static Vector2 operator /(Vector2 v, long s) => new(v.X / s, v.Y / s);
    }

    record class BoundingBox
    {
        public Vector2 Position { get; private set; }
        public Vector2 Size { get; private set; }

        public BoundingBox(Vector2 position, Vector2 size)
        {
            Position = position;
            Size = size;
        }

        public bool Intersects(BoundingBox other)
        {
            return Intersects(other.Position, other.Size);
        }

        public bool Intersects(Vector2 position, Vector2 size)
        {
            var a = Position;
            var b = Position + Size;
            var c = position;
            var d = position + size;

            return a.X < d.X && b.X > c.X && a.Y < d.Y && b.Y > c.Y;
        }

        public bool IsInside(Vector2 position)
        {
            var l = Position;
            var r = Position + Size;
            var p = position;
            
            return l.X <= p.X && p.X < r.X && l.Y <= p.Y && p.Y < r.Y;
        }

        public bool IsAtLeft(Vector2 position) => Position.X == position.X && Position.Y == position.Y;

        public bool IsAtRight(Vector2 position) => Position.X + Size.X - 1 == position.X && Position.Y == position.Y;

        public void Move(Vector2 direction) => Position += direction;
    }

    static readonly Dictionary<char, Vector2> DirectionForSymbol = new()
    {
        ['^'] = new Vector2(0, -1),
        ['v'] = new Vector2(0, 1),
        ['<'] = new Vector2(-1, 0),
        ['>'] = new Vector2(1, 0),
    };

    static readonly Dictionary<Vector2, char> SymbolForDirection =
        DirectionForSymbol.ToDictionary(kv => kv.Value, kv => kv.Key);

    static void Main()
    {
        /**
         * https://adventofcode.com/2024/day/15
         */

        // Part 1
        Console.WriteLine("Part 1");
        Console.WriteLine("------");
        Console.WriteLine(Part1And2("Data/sample1.txt", 1, false));
        Console.WriteLine(Part1And2("Data/sample2.txt", 1, false));
        Console.WriteLine(Part1And2("Data/sample3.txt", 1, false));
        Console.WriteLine(Part1And2("Data/input.txt", 1, false));
        Console.WriteLine();

        // Part 2
        Console.WriteLine("Part 2");
        Console.WriteLine("------");
        Console.WriteLine(Part1And2("Data/sample1.txt", 2, false));
        Console.WriteLine(Part1And2("Data/sample2.txt", 2, false));
        Console.WriteLine(Part1And2("Data/sample3.txt", 2, false));
        Console.WriteLine(Part1And2("Data/input.txt", 2, false));
        Console.WriteLine();
    }

    static long Part1And2(string inputPath, int scaleX, bool printProgress = false)
    {
        var input = File.ReadAllText(inputPath);
        var (size, walls, boxes, start, movements) = ParseMap(input, scaleX);
        var robot = start;

        foreach (var (movement, i) in movements.Select((m, i) => (m, i)))
        {
            TryMoveRobot(walls, boxes, robot, movement);

            if (printProgress)
            {
                Console.WriteLine($"{i + 1}/{movements.Length}: {SymbolForDirection[movement]}");
                PrintMap(size, walls, boxes, start);
                if (i < movements.Length - 1)
                {
                    Console.WriteLine($"Next: {SymbolForDirection[movements[i + 1]]}");
                }
                Thread.Sleep(200);
                Console.WriteLine();
            }
        }

        var sumOfCoordinates = boxes.Sum(b => 100 * b.Position.Y + b.Position.X);
        return sumOfCoordinates;
    }

    static bool TryMoveRobot(BoundingBox[] walls, BoundingBox[] boxes, BoundingBox robot, Vector2 direction)
    {
        /**
         * This will actually move the robot if it succeeds, i.e. is not blocked by 
         * a wall or an unmovable box.
         *
         * Moving a box can be complicated because they can be connected to more than one box.
         * This is an example with boxes [], and wall segments #. Here, NO boxes should be moved
         * because the right-most box is blocked by a wall.
         *
         *      |      []
         *      |     [][]
         *      v    []  []
         *                ###
         *
         * We achieve this by first checking the tree to see that all boxes can be moved, 
         * and if movable, we move them all.
         */

        var newPosition = robot.Position + direction;
        var adjacentWalls = walls.Where(w => w.Intersects(newPosition, robot.Size));
        var adjacentBoxes = boxes.Where(b => b.Intersects(newPosition, robot.Size));

        // Robot can move freely
        if (!adjacentWalls.Any() && !adjacentBoxes.Any())
        {
            robot.Move(direction);
            return true;
        }

        // Robot can only move if boxes are also movable
        if (adjacentBoxes.Any())
        {
            var movableBoxes = adjacentBoxes
                .SelectMany(b => GetMovableBoxes(walls, boxes, b, direction))
                .ToArray();

            if (movableBoxes.Length != 0)
            {
                foreach (var box in movableBoxes)
                {
                    box.Move(direction);
                }

                robot.Move(direction);
                return true;
            }
        }

        return false;
    }

    static BoundingBox[] GetMovableBoxes(BoundingBox[] walls, BoundingBox[] boxes, BoundingBox box, Vector2 direction)
    {
        /**
         * For a given box, this method will return all adjacent boxes, and the box itself,
         * that can be moved in the given direction.
         *
         * Most importantly, if a sub-tree of boxes cannot be moved this method will return
         * an empty array.
         */

        var newPosition = box.Position + new Vector2(direction.X, direction.Y);
        var adjacentWalls = walls.Where(w => w.Intersects(newPosition, box.Size));
        var adjacentBoxes = boxes.Except([box]).Where(b => b.Intersects(newPosition, box.Size));

        if (adjacentWalls.Any())
        {
            return [];
        }

        List<BoundingBox> result = [box];

        foreach (var adjacentBox in adjacentBoxes)
        {
            var movableBoxes = GetMovableBoxes(walls, boxes, adjacentBox, direction);
            if (movableBoxes.Length == 0)
            {
                return [];
            }
            else
            {
                result.AddRange(movableBoxes);
            }
        }

        return [.. result];
    }

    static (Vector2 Size, BoundingBox[] Walls, BoundingBox[] Boxes, BoundingBox Robot, Vector2[] Movements) ParseMap(string input, int scaleX)
    {
        var rawMap = input.Split($"{Environment.NewLine}{Environment.NewLine}")[0];
        var rawMovements = input.Split($"{Environment.NewLine}{Environment.NewLine}")[1];

        var size = new Vector2(
            rawMap.Split(Environment.NewLine).First().Length * scaleX,
            rawMap.Split(Environment.NewLine).Length
        );

        var grid = rawMap
            .Split(Environment.NewLine)
            .Select((line, y) => (line, y))
            .SelectMany(t => t.line.Select((c, x) => (Symbol: c, X: x, Y: t.y)));

        var walls = grid
            .Where(g => g.Symbol == '#')
            .Select(g => new BoundingBox(new Vector2(g.X * scaleX, g.Y), new Vector2(scaleX, 1)))
            .ToArray();

        var boxes = grid
            .Where(g => g.Symbol == 'O')
            .Select(g => new BoundingBox(new Vector2(g.X * scaleX, g.Y), new Vector2(scaleX, 1)))
            .ToArray();

        var robot = grid
            .Where(g => g.Symbol == '@')
            .Select(g => new BoundingBox(new Vector2(g.X * scaleX, g.Y), new Vector2(1, 1)))
            .First();

        var movements = rawMovements
            .Split(Environment.NewLine)
            .SelectMany(line => line.Select(c => DirectionForSymbol[c]).ToArray())
            .ToArray();

        return (size, walls, boxes, robot, movements);
    }

    static void PrintMap(Vector2 size, BoundingBox[] walls, BoundingBox[] boxes, BoundingBox player)
    {
        for (int y = 0; y < size.Y; y++)
        {
            Console.Write($"{y:D3} ");
            for (int x = 0; x < size.X; x++)
            {
                var pos = new Vector2(x, y);

                if (player.IsInside(pos))
                {
                    Console.Write('@');
                }
                else if (walls.Any(w => w.IsInside(pos)))
                {
                    Console.Write('#');
                }
                else if (boxes.Any(b => b.IsAtLeft(pos)) && boxes.Any(b => b.IsAtRight(pos)) )
                {
                    Console.Write('O');
                }
                else if (boxes.Any(b => b.IsAtLeft(pos)))
                {
                    Console.Write('[');
                }
                else if (boxes.Any(b => b.IsAtRight(pos)))
                {
                    Console.Write(']');
                }
                else
                {
                    Console.Write('.');
                }
            }
            Console.WriteLine();
        }
    }
}
