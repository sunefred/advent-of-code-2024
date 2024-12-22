using System.Diagnostics;

namespace Day22;

internal class Program
{
    static void Main()
    {
        /**
         * https://adventofcode.com/2024/day/22
         * 
         */

        var input = File.ReadAllLines("Data/input.txt");
        var sellerStarts = input.Select(long.Parse).ToArray();

        var sw1 = Stopwatch.StartNew();
        var part1 = Part1(sellerStarts);
        sw1.Stop();
        Console.WriteLine($"Part 1: {part1}, Elapsed: {sw1.ElapsedMilliseconds}ms");

        var sw2 = Stopwatch.StartNew();
        var part2 = Part2(sellerStarts);
        sw2.Stop();
        Console.WriteLine($"Part 2: {part2}, Elapsed: {sw2.ElapsedMilliseconds}ms");
    }

    static long Part1(long[] sellerStarts)
    {
        var totalSum = 0L;

        foreach (var start in sellerStarts)
        {
            var secret = NthSecret(start, 2000);
            totalSum += secret;
        }

        return totalSum;
    }

    static long Part2(long[] sellerStarts)
    {
        /**
         * We create a dictionary of the to store the price at the FIRST occurrence of each
         * sequence of differences, for each seller:
         *
         *      (Seller0, (D0, D1, D2, D3)) => #Bananas
         *      (Seller0, (D1, D2, D3, D4)) => #Bananas
         *      ...
         *      (Seller1, (D0, D1, D2, D3)) => #Bananas
         *      ...
         *
         * In order to get the best sequence, that optimizes the number of bananas, we group
         * by the sequence. This group contains all the sellers that have the same sequence,
         * and their respective number of bananas you would win.
         *
         * The sum for each group represents the total winnings and ordering by this sum, we can
         * get the best sequence.
         */

        const int N = 2000;
        var priceDict = new Dictionary<(int Seller, (int, int, int, int) Seq), int>();

        foreach (var (start, seller) in sellerStarts.Select((s, i) => (s, i)))
        {
            var secrets = SeqOfSecrets(start, N);
            var prices = secrets.Select(s => (int)(s % 10));
            var diffs = prices.Skip(1).Zip(prices).Select(tp => tp.First - tp.Second).ToArray();

            foreach (var (price, i) in prices.Skip(4).Select((p, i) => (p, i)))
            {
                var seq = diffs[i..(i + 4)];
                var key = (seller, (seq[0], seq[1], seq[2], seq[3]));

                if (!priceDict.ContainsKey(key))
                {
                    priceDict[key] = price;
                }
            }
        }

        var best = priceDict
            .GroupBy(kvp => kvp.Key.Seq)
            .Select(g => (Seq: g.Key, Bananas: g.Sum(kvp => kvp.Value)))
            .MaxBy(tp => tp.Bananas);

        return best.Bananas;
    }

    static long NthSecret(long start, int n)
    {
        return SeqOfSecrets(start, n).Last();
    }

    static IEnumerable<long> SeqOfSecrets(long start, int n)
    {
        /**
         * Returns start and the next 'n' secrets
         */

        var secret = start;
        yield return secret;

        for (var i = 0; i < n; i++)
        {
            secret = NexSecret(secret);
            yield return secret;
        }
    }

    static long NexSecret(long current)
    {
        // Paragraph 1
        var a = current * 64;
        var b = Mix(a, current);
        var c = Prune(b);

        // Paragraph 2
        var d = c / 32;
        var e = Mix(d, c);
        var f = Prune(e);

        // Paragraph 3
        var g = f * 2048;
        var h = Mix(g, f);
        var i = Prune(h);

        return i;
    }

    static long Mix(long mixed, long secret)
    {
        return mixed ^ secret;
    }

    static long Prune(long mixed)
    {
        return mixed % 16777216L;
    }
}
