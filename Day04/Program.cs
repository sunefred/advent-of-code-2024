namespace Day04;

internal class Program
{
    static void Main()
    {
        /**
         * https://adventofcode.com/2024/day/4
         * 
         */

        var lines = File.ReadAllLines("Data/sample.txt");
        char[][] grid = [.. lines.Select(line => line.ToCharArray())];

        Console.WriteLine(Part1(grid));
        Console.WriteLine(Part2(grid));
    }

    static int Part1(char[][] grid)
    {
        int count = 0;
        var word = "XMAS";
        int rows = grid.Length;
        int cols = grid[0].Length;

        // Define all 8 directions
        int[][] directions = [
            [0, 1],
            [0, -1],
            [1, 0],
            [-1, 0],
            [1, 1],
            [1, -1],
            [-1, 1],
            [-1, -1]
        ];

        // Iterate over all cells and directions
        for (int x = 0; x < rows; x++)
        {
            for (int y = 0; y < cols; y++)
            {
                foreach (var direction in directions)
                {
                    int dx = direction[0];
                    int dy = direction[1];

                    var chars = word.Select((_, i) => (x: x + i * dx, y: y + i * dy))
                        .Where(p => IsInBounds(p.x, p.y))
                        .Select(p => grid[p.x][p.y]);

                    if (string.Concat(chars) == word)
                    {
                        count++;
                    }
                }
            }
        }

        return count;

        // Helper functions
        bool IsInBounds(int x, int y)
        {
            return 0 <= x && x < rows && 0 <= y && y < cols;
        }
    }

    static int Part2(char[][] grid)
    {
        int count = 0;
        int rows = grid.Length;
        int cols = grid[0].Length;

        // Check all possible centers for the "X-MAS" cross
        for (int x = 1; x <= rows; x++)
        {
            for (int y = 1; y <= cols; y++)
            {
                if (CheckForXMas(x, y))
                {
                    count++;
                }
            }
        }

        return count;

        // Helper functions
        bool CheckForXMas(int x, int y)
        {
            if (!IsXInBounds(x, y))
            {
                return false;
            }

            return IsMasOrSam(x - 1, y - 1, x, y, x + 1, y + 1)
                && IsMasOrSam(x + 1, y - 1, x, y, x - 1, y + 1);
        }

        bool IsMasOrSam(int x0, int y0, int x1, int y1, int x2, int y2)
        {
            var word = $"{grid[x0][y0]}{grid[x1][y1]}{grid[x2][y2]}";
            return word == "MAS" || word == "SAM";
        }

        bool IsXInBounds(int x, int y)
        {
            return 0 < x && x + 1 < rows
                && 0 < y && y + 1 < cols;
        }
    }
}
