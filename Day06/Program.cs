namespace Day06;

internal class Program
{
    record class Position(int X, int Y);
    record class Direction(int DX, int DY);
    enum Action { Start, Forward, Turn, Cycle, Outside }

    static void Main()
    {
        /**
         * https://adventofcode.com/2024/day/6
         * 
         */

        var input = File.ReadAllLines("Data/sample.txt");
        var floor = input.Select(line => line.ToCharArray()).ToArray();

        Console.WriteLine(Part1(floor, false));
        Console.WriteLine(Part2(floor, false));
    }

    static int Part1(char[][] floor, bool showFloor)
    {
        var (startPos, startDir) = GetStartPosition(floor);
        var visited = MoveAlongGuardPath(floor, startPos, startDir, showFloor);

        foreach (var (pos, dir, action) in visited)
        {
            PrintFloor(floor, pos, dir);
            Console.WriteLine();
        }

        var unique = visited
            .Where(x => x.Action == Action.Start || x.Action == Action.Forward)
            .Select(x => x.Position)
            .Distinct();

        return unique.Count();
    }

    static int Part2(char[][] floor, bool showFloor)
    {
        var result = 0;
        var (startPos, startDir) = GetStartPosition(floor);

        foreach (var candidate in GetCandidateFloors(floor))
        {
            var visited = MoveAlongGuardPath(candidate, startPos, startDir, showFloor);
            if (visited.Last().Action == Action.Cycle)
            {
                result++;
            }
        }

        return result;
    }

    static (Position Pos, Direction Dir) GetStartPosition(char[][] floor)
    {
        var rows = floor.Length;
        var cols = floor[0].Length;

        var directions = new Dictionary<char, Direction>
        {
            { '^', new Direction(0, -1) },
            { '>', new Direction(1, 0) },
            { 'v', new Direction(0, 1) },
            { '<', new Direction(-1, 0) }
        };

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                if (directions.TryGetValue(floor[y][x], out Direction? value))
                {
                    return (new Position(x, y), value);
                }
            }
        }

        throw new Exception("Guard's initial position not found!");
    }

    static IEnumerable<(Position Position, Direction Direction, Action Action)> MoveAlongGuardPath(char[][] floor, Position startPos, Direction startDir, bool showFloor)
    {
        Position pos = startPos;
        Direction dir = startDir;
        HashSet<(Position, Direction)> visited = [(pos, dir)];

        yield return (pos, dir, Action.Start);

        while (true)
        {
            if (showFloor)
            {
                PrintFloor(floor, pos, dir);
            }

            var candidate = new Position(pos.X + dir.DX, pos.Y + dir.DY);
            if (IsOutside(floor, candidate))
            {
                yield return (candidate, dir, Action.Outside);
                break;
            }

            if (IsObstructed(floor, candidate))
            {
                dir = new Direction(-dir.DY, dir.DX);
                if (visited.Contains((pos, dir)))
                {
                    yield return (pos, dir, Action.Cycle);
                    break;
                }
                else
                {
                    visited.Add((pos, dir));
                    yield return (pos, dir, Action.Turn);
                }
            }
            else
            {
                pos = candidate;
                if (visited.Contains((pos, dir)))
                {
                    yield return (candidate, dir, Action.Cycle);
                    break;
                }
                else
                {
                    visited.Add((pos, dir));
                    yield return (pos, dir, Action.Forward);
                }
            }
        }
    }

    static IEnumerable<char[][]> GetCandidateFloors(char[][] floor)
    {
        var rows = floor.Length;
        var cols = floor[0].Length;

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                if (CanBeObstructed(floor, new Position(x, y)))
                {
                    var candidate = floor.Select(row => row.ToArray()).ToArray();
                    candidate[y][x] = 'O';
                    yield return candidate;
                }
            }
        }
    }

    static bool IsObstructed(char[][] floor, Position pos)
    {
        return floor[pos.Y][pos.X] == '#' || floor[pos.Y][pos.X] == 'O';
    }

    static bool CanBeObstructed(char[][] floor, Position pos)
    {
        return !(floor[pos.Y][pos.X] == '#' || floor[pos.Y][pos.X] == 'O' || floor[pos.Y][pos.X] == '^');
    }

    static bool IsOutside(char[][] floor, Position pos)
    {
        var rows = floor.Length;
        var cols = floor[0].Length;

        return !(0 <= pos.X && pos.X < cols && 0 <= pos.Y && pos.Y < rows);
    }

    static void PrintFloor(char[][] floor, Position pos, Direction dir)
    {
        int rows = floor.Length;
        int cols = floor[0].Length;

        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                if (pos.X == x && pos.Y == y)
                {
                    Console.Write(dir switch
                    {
                        { DX: 0, DY: -1 } => '^',
                        { DX: 1, DY: 0 } => '>',
                        { DX: 0, DY: 1 } => 'v',
                        { DX: -1, DY: 0 } => '<',
                        _ => throw new Exception("Invalid direction")
                    });
                    Console.ResetColor();
                }
                else
                {
                    Console.Write(floor[y][x]);
                }
            }
            Console.WriteLine();
        }
    }
}
