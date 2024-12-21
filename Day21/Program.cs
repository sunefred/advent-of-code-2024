using System.Diagnostics;

namespace Day21;

internal class Program
{
    record class Vector2(int X, int Y)
    {
        public static Vector2 Up => new(0, -1);
        public static Vector2 Down => new(0, 1);
        public static Vector2 Left => new(-1, 0);
        public static Vector2 Right => new(1, 0);
        public static Vector2 operator +(Vector2 u, Vector2 v) => new(u.X + v.X, u.Y + v.Y);
        public static Vector2 operator -(Vector2 u, Vector2 v) => new(u.X - v.X, u.Y - v.Y);
        public static Vector2 operator *(Vector2 v, int s) => new(v.X * s, v.Y * s);
        public static Vector2 operator *(int s, Vector2 v) => new(v.X * s, v.Y * s);
        public static Vector2 operator /(Vector2 v, int s) => new(v.X / s, v.Y / s);
    }

    readonly static Dictionary<char, Vector2> NumpadPositions = new()
    {
        ['7'] = new(0, 0),
        ['8'] = new(1, 0),
        ['9'] = new(2, 0),
        ['4'] = new(0, 1),
        ['5'] = new(1, 1),
        ['6'] = new(2, 1),
        ['1'] = new(0, 2),
        ['2'] = new(1, 2),
        ['3'] = new(2, 2),
        ['0'] = new(1, 3),
        ['A'] = new(2, 3),
    };

    readonly static Dictionary<char, Vector2> DirpadPositions = new()
    {
        ['^'] = new(1, 0),
        ['A'] = new(2, 0),
        ['<'] = new(0, 1),
        ['v'] = new(1, 1),
        ['>'] = new(2, 1),
    };

    static void Main()
    {
        /**
         * https://adventofcode.com/2024/day/21
         */

        var lines = File.ReadAllLines("Data/input.txt");

        /**
         *              Depth
         *              -----
         *  Part 1      3
         *  Part 2      26
         */

        var sw = Stopwatch.StartNew();
        var result = Part1And2(lines, 26);
        sw.Stop();

        Console.WriteLine($"Result: {result}, Elapsed: {sw.ElapsedMilliseconds}ms");
    }

    static long Part1And2(string[] sequences, int depth)
    {
        var totalComplexity = 0L;
        var memo = new Dictionary<(char, char, int), long>();

        foreach (var sequence in sequences)
        {
            var length = MinSequenceLength(sequence, 0, depth, memo);
            var number = int.Parse(new string([.. sequence.Where(char.IsDigit)]));
            var complexity = number * length;
            totalComplexity += complexity;

            Console.WriteLine($"Number: {number}, Length: {length}, Complexity: {complexity}");
        }

        return totalComplexity;
    }

    static long MinSequenceLength(string sequence, int layer, int depth, Dictionary<(char, char, int), long> memo)
    {
        /**
         * For a given sequence 'seq', its length is the sum of the length for each 'pair' 
         * in the sequence.
         * 
         * A pair is a start and end position on the key-pad (numpad or dirpad). The length
         * of a 'pair' is the length of the shortest sequence that describes moving from
         * 'start' to 'end'.
         * 
         * 
         *      LAYER X                 .---seq---. 
         *                             /           \
         *      LAYER X            pair1            pair2
         *                         /   \            /   \
         *      LAYER X+1    seq1.1   seq1.2    seq2.1  seq2.2
         * 
         *
         *      len_pair1 = min(len_seq1.1 + len_seq1.2) 
         *      len_seq = sum(pair1, pair2, ...)
         *
         *  
         * There can be arbitrary number of layers, so this formula is applied recursively.
         */

        if (layer == depth)
        {
            return sequence.Length;
        }

        var result = 0L;
        var sequence1 = "A" + sequence;
        var pairs = sequence1.Zip(sequence1.Skip(1)).ToArray();

        foreach (var (start, end) in pairs)
        {
            var length = MinSequenceLengthForPair(start, end, layer, depth, memo);
            result += length;
        }

        return result;
    }

    static long MinSequenceLengthForPair(char start, char end, int layer, int depth, Dictionary<(char, char, int), long> memo)
    {
        /**
         * Its better to cache pairs than sequences. We will get more hits.
         */

        if (memo.TryGetValue((start, end, layer), out var value))
        {
            return value;
        }

        var sequences = layer == 0
            ? GenerateNumpadSequences(NumpadPositions[start], NumpadPositions[end])
            : GenerateDirpadSequences(DirpadPositions[start], DirpadPositions[end]);

        var length = sequences.Select(s => MinSequenceLength(s, layer + 1, depth, memo)).Min();

        memo[(start, end, layer)] = length;
        return length;
    }

    static List<string> GenerateNumpadSequences(Vector2 start, Vector2 end) =>
        GenerateSequences(start, end, NumpadPositions.ContainsValue);

    static List<string> GenerateDirpadSequences(Vector2 start, Vector2 end) =>
        GenerateSequences(start, end, DirpadPositions.ContainsValue);

    static List<string> GenerateSequences(Vector2 start, Vector2 end, Func<Vector2, bool> inBounds)
    {
        if (!inBounds(start))
        {
            return [];
        }

        if (start == end)
        {
            return ["A"];
        }

        var result = new List<string>();

        // Go up ^, negative Y
        if (start.Y > end.Y)
        {
            foreach (string str in GenerateSequences(start + Vector2.Up, end, inBounds))
            {
                result.Add("^" + str);
            }
        }

        // Go down v, positive Y
        if (start.Y < end.Y)
        {
            foreach (string str in GenerateSequences(start + Vector2.Down, end, inBounds))
            {
                result.Add("v" + str);
            }
        }

        // Go left <, negative X
        if (start.X > end.X)
        {
            foreach (string str in GenerateSequences(start + Vector2.Left, end, inBounds))
            {
                result.Add("<" + str);
            }
        }

        // Go right >, positive X
        if (start.X < end.X)
        {
            foreach (string str in GenerateSequences(start + Vector2.Right, end, inBounds))
            {
                result.Add(">" + str);
            }
        }

        return result;
    }
}
