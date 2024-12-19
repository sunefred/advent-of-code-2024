// using System.Globalization;
// using System.Text.RegularExpressions;
// using System.Xml.Serialization;
// using Microsoft.VisualBasic;



// class Program2
// {
//     record class Vector2(int X, int Y)
//     {
//         public static Vector2 operator +(Vector2 p, Vector2 v) => new(p.X + v.X, p.Y + v.Y);
//         public static Vector2 operator -(Vector2 p, Vector2 v) => new(p.X - v.X, p.Y - v.Y);
//         public static Vector2 operator *(Vector2 v, int s) => new(v.X * s, v.Y * s);
//         public static Vector2 operator *(int s, Vector2 v) => new(v.X * s, v.Y * s);
//         public static Vector2 operator /(Vector2 v, int s) => new(v.X / s, v.Y / s);
//     }

//     enum Obstruction
//     {
//         None,
//         BoxLeft,
//         BoxRight,
//         Wall,
//     }

//     static void Main()
//     {
//         var input = File.ReadAllText("Data/sample3.txt");

//         Console.WriteLine(Part1(input));
//         Console.WriteLine(Part2(input));
//     }

//     static long Part1(string input)
//     {
//         var (start, map, movements) = ParseMap(input);
//         PrintMap(map, start, null);
//         Console.WriteLine();

//         var position = start;
//         foreach (var movement in movements)
//         {
//             position = MoveRobot(map, position, movement);
//             PrintMap(map, position, movement);
//             Console.WriteLine();
//         }

//         // var sumOfCoordinates = 0;
//         // for (int y = 0; y < map.Length; y++)
//         // {
//         //     for (int x = 0; x < map[y].Length; x++)
//         //     {
//         //         if (map[y][x] == Obstruction.Box)
//         //         {
//         //             var coordinate = 100 * y + x;
//         //             sumOfCoordinates += coordinate;
//         //         }
//         //     }
//         // }

//         // return sumOfCoordinates;

//         return 0;
//     }

//     static Vector2 MoveRobot(Obstruction[][] map, Vector2 position, Vector2 direction)
//     {
//         var position1 = position + direction;
//         var content1 = map[position1.Y][position1.X];

//         // Wall => do nothing
//         if (content1 == Obstruction.Wall)
//         {
//             return position;
//         }

//         // Nothing => free to move
//         if (content1 == Obstruction.None)
//         {
//             return position1;
//         }

//         var noneFree = Enumerable.Range(0, int.MaxValue)
//             .Select(i => position1 + direction * i)
//             .TakeWhile(p => InBounds(map, p))
//             .Select(p => map[p.Y][p.X])
//             .TakeWhile(c => c != Obstruction.None)
//             .ToArray();

//         // Row of boxes with wall behind => do nothing
//         if ((content1 == Obstruction.BoxLeft || content1 == Obstruction.BoxRight) && noneFree.Contains(Obstruction.Wall))
//         {
//             return position;
//         }

//         // Boxes with one free space behind => move all boxes
//         var firstBoxLeft = content1 switch
//         {
//             Obstruction.BoxLeft => position1,
//             Obstruction.BoxRight => position1 - new Vector2(1, 0),
//             _ => throw new InvalidOperationException()
//         };
//         var firstBoxRight = firstBoxLeft + new Vector2(1, 0);

//         var skip = direction.X != 0 ? 2 : 1;

//         for (int i = 1; i < noneFree.Length; i+=skip)
//         {
//             var curBoxLeft = firstBoxLeft + i * direction;
//             var curBoxRight = firstBoxRight + i * direction;
//             map[curBoxLeft.Y][curBoxLeft.X] = Obstruction.BoxLeft;
//             map[curBoxRight.Y][curBoxRight.X] = Obstruction.BoxRight;
//         }

//         map[position1.Y][position1.X] = Obstruction.None;
//         return position1;
//     }

//     static bool InBounds(Obstruction[][] map, Vector2 position)
//     {
//         var width = map[0].Length;
//         var height = map.Length;

//         return position.X >= 0 && position.X < width && position.Y >= 0 && position.Y < height;
//     }

//     static long Part2(string input)
//     {
//         return 2;
//     }

//     static (Vector2 start, Obstruction[][], Vector2[] Movements) ParseMap(string input)
//     {
//         var rawMap = input.Split($"{Environment.NewLine}{Environment.NewLine}")[0];
//         var rawMovements = input.Split($"{Environment.NewLine}{Environment.NewLine}")[1];

//         Vector2 start = rawMap
//             .Split(Environment.NewLine)
//             .Select((line, y) => (line, y))
//             .SelectMany(t => t.line.Select((c, x) => (c, x, t.y)))
//             .Where(t => t.c == '@')
//             .Select(t => new Vector2(2 * t.x, t.y))
//             .First();

//         var map = rawMap
//             .Split(Environment.NewLine)
//             .Select(line => line.SelectMany<char, Obstruction>(c => c switch
//             {
//                 '.' => [Obstruction.None, Obstruction.None],
//                 'O' => [Obstruction.BoxLeft, Obstruction.BoxRight],
//                 '#' => [Obstruction.Wall, Obstruction.Wall],
//                 '@' => [Obstruction.None, Obstruction.None],
//                 _ => throw new InvalidOperationException()
//             }).ToArray())
//             .ToArray();

//         var movements = rawMovements
//             .Split(Environment.NewLine)
//             .SelectMany(line => line.Select(c => c switch
//             {
//                 '^' => new Vector2(0, -1),
//                 'v' => new Vector2(0, 1),
//                 '<' => new Vector2(-1, 0),
//                 '>' => new Vector2(1, 0),
//                 _ => throw new InvalidOperationException()
//             }).ToArray())
//             .ToArray();

//         return (start, map, movements);
//     }

//     static void PrintMap(Obstruction[][] map, Vector2 position, Vector2? movement)
//     {
//         var height = map.Length;
//         var width = map[0].Length;

//         if (movement is not null)
//         {
//             var symbol = movement switch
//             {
//                 { X: 0, Y: -1 } => '^',
//                 { X: 0, Y: 1 } => 'v',
//                 { X: -1, Y: 0 } => '<',
//                 { X: 1, Y: 0 } => '>',
//                 _ => throw new InvalidOperationException()
//             };

//             Console.WriteLine($"Move: {symbol}");
//         }

//         Console.Write("  ");
//         for (int x = 0; x < width; x++)
//         {
//             Console.Write($"{x%10:D1}");
//         }
//         Console.WriteLine();

//         for (int y = 0; y < height; y++)
//         {
//             Console.Write($"{y%10:D1} ");

//             for (int x = 0; x < width; x++)
//             {
//                 if (position.X == x && position.Y == y)
//                 {
//                     Console.Write('@');
//                 }
//                 else
//                 {
//                     Console.Write(map[y][x] switch
//                     {
//                         Obstruction.None => '.',
//                         Obstruction.BoxLeft => '[',
//                         Obstruction.BoxRight => ']',
//                         Obstruction.Wall => '#',
//                         _ => throw new InvalidOperationException()
//                     });
//                 }
//             }
//             Console.WriteLine();
//         }
//     }
// }