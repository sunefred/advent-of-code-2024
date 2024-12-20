using System.Text.RegularExpressions;

namespace Day14;

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

    record class Robot
    {
        public Robot(Vector2 position, Vector2 velocity)
        {
            Position = position;
            Velocity = velocity;
        }

        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; }
    }

    static void Main()
    {
        /**
         * https://adventofcode.com/2024/day/14
         * 
         */

        //int width = 11;
        //int height = 7;
        //var input = File.ReadAllText("Data/sample.txt");

        int width = 101;
        int height = 103;
        var input = File.ReadAllText("Data/input.txt");

        var matches = Regex.Matches(input, @"p=(?<px>-?\d+),(?<py>-?\d+)\s+v=(?<vx>-?\d+),(?<vy>-?\d+)");

        var robots = matches.Select(match =>
        {
            var px = int.Parse(match.Groups["px"].Value);
            var py = int.Parse(match.Groups["py"].Value);
            var vx = int.Parse(match.Groups["vx"].Value);
            var vy = int.Parse(match.Groups["vy"].Value);
            return new Robot(new Vector2(px, py), new Vector2(vx, vy));
        })
        .ToList();

        Console.WriteLine(Part1(width, height, robots));
        Console.WriteLine(Part2(width, height, robots));
    }

    static long Part1(int width, int height, List<Robot> robots)
    {
        for (int i = 0; i < 100; i++)
        {
            foreach (var robot in robots)
            {
                MoveRobotOnce(width, height, robot);
            }
        }

        PrintArea(width, height, robots);

        var topLeftCount = robots.Where(r => r.Position.X < width / 2 && r.Position.Y < height / 2).Count();
        var topRightCount = robots.Where(r => r.Position.X > width / 2 && r.Position.Y < height / 2).Count();
        var bottomLeftCount = robots.Where(r => r.Position.X < width / 2 && r.Position.Y > height / 2).Count();
        var bottomRightCount = robots.Where(r => r.Position.X > width / 2 && r.Position.Y > height / 2).Count();

        return topLeftCount * topRightCount * bottomLeftCount * bottomRightCount;
    }

    static long Part2(int width, int height, List<Robot> robots)
    {
        for (int i = 0; i < int.MaxValue; i++)
        {
            foreach (var robot in robots)
            {
                MoveRobotOnce(width, height, robot);
            }

            var robotCounts = robots.GroupBy(r => r.Position).Select(g => g.Count());
            if (robotCounts.Max() == 1)
            {
                PrintArea(width, height, robots);
                return i + 1;
            }
        }

        return 0;
    }

    static void MoveRobotOnce(int width, int height, Robot robot)
    {
        var newX = (robot.Position.X + robot.Velocity.X + width) % width;
        var newY = (robot.Position.Y + robot.Velocity.Y + height) % height;
        robot.Position = new Vector2(newX, newY);
    }

    static void PrintArea(int width, int height, List<Robot> robots)
    {
        for (var y = 0; y < height; y++)
        {
            Console.Write($"{y:D3} ");

            for (var x = 0; x < width; x++)
            {
                var position = new Vector2(x, y);
                var robotCount = robots.Count(robot => robot.Position == position);

                if (robotCount > 0)
                {
                    Console.Write(robotCount);
                }
                else
                {
                    Console.Write(".");
                }
            }

            Console.WriteLine();
        }

        Console.WriteLine();
    }
}
