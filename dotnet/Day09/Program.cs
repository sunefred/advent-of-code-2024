record class FileSpan(int FileId, int Length);

internal class Program
{
    static void Main()
    {
        /**
         * https://adventofcode.com/2024/day/9
         * 
         */

        var input = File.ReadLines("Data/sample.txt").First();
        var fileSpanLengths = input.ToArray().Select(c => c  - '0').ToArray();

        Console.WriteLine(Part1(input));
        Console.WriteLine(Part2(input));
    }

    static long Part1(string input)
    {
        var diskMap = ParseDiskMap(input);
        PrintDiskMap(diskMap);

        CompactDiskMap1(diskMap);
        PrintDiskMap(diskMap);

        return CalculateChecksum(diskMap);
    }

    static long Part2(string input)
    {
        var diskMap = ParseDiskMap(input);
        PrintDiskMap(diskMap);
        
        CompactDiskMap2(diskMap);
        PrintDiskMap(diskMap);

        return CalculateChecksum(diskMap);
    }

    static void CompactDiskMap1(List<int> diskMap)
    {
        while (true)
        {
            int freeIndex = diskMap.IndexOf(-1);

            int fileId = diskMap.Last(v => v != -1);
            int fileIndex = diskMap.LastIndexOf(fileId);

            if (freeIndex > fileIndex)
            {
                break;
            }

            diskMap[freeIndex] = fileId;
            diskMap[fileIndex] = -1;
        }
    }

    static void CompactDiskMap2(List<int> diskMap)
    {
        var maxFileId = diskMap.Max();

        for (int fileId = maxFileId; fileId >= 0; fileId--)
        {
            int fileIndex = diskMap.IndexOf(fileId);
            int fileLength = diskMap.Skip(fileIndex).TakeWhile(id => id == fileId).Count();

            var freeSpans = GetFreeSpans(diskMap);
            var (freeIndex, freeLength) = freeSpans.FirstOrDefault(span => span.Length >= fileLength);

            if (freeLength == 0 || freeIndex > fileIndex)
            {
                continue;
            }

            for (int i = 0; i < fileLength; i++)
            {
                diskMap[freeIndex + i] = fileId;
                diskMap[fileIndex + i] = -1;
            }
        }
    }

    static List<int> ParseDiskMap(string input)
    {
        var diskMap = new List<int>();

        for (int i = 0; i < input.Length; i++)
        {
            if (i % 2 == 0)
            {
                int fileLength = input[i] - '0';
                var extent = Enumerable.Repeat(i / 2, fileLength);
                diskMap.AddRange(extent);
            }
            else
            {
                int freeLength = input[i] - '0';
                var extent = Enumerable.Repeat(-1, freeLength);
                diskMap.AddRange(extent);
            }
        }

        return [.. diskMap];
    }

    static IEnumerable<(int Index, int Length)> GetFreeSpans(List<int> diskMap)
    {
        var result = new List<(int Index, int Length)>();

        for (int i = 0; i < diskMap.Count; )
        {
            if (diskMap[i] == -1)
            {
                int length = diskMap.Skip(i).TakeWhile(id => id == -1).Count();
                result.Add((i, length));
                i += length;
            }
            else
            {
                i++;
            }
        }

        return result;
    }

    static long CalculateChecksum(List<int> diskMap)
    {
        return diskMap
            .Select((fileId, index) => (FileId: fileId, Index: index))
            .Where(pair => pair.FileId != -1)
            .Sum(pair => (long)pair.FileId * pair.Index);
    }

    static void PrintDiskMap(List<int> diskMap)
    {
        foreach (var fileId in diskMap)
        {
            Console.Write(fileId != -1 ? fileId : ".");
        }

        Console.WriteLine();
    }
}