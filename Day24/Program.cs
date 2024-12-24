using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;

namespace Day24;

internal partial class Program
{
    enum Operator
    {
        AND,
        OR,
        XOR,
    }

    class Wire(string name)
    {
        public string Name { get; init; } = name;
        public bool? Value { get; set; }
    }

    class Gate(Wire left, Wire right, Wire output, Operator @operator)
    {
        public Wire Left { get; init; } = left;
        public Wire Right { get; init; } = right;
        public Wire Output { get; init; } = output;
        public Operator Operator { get; init; } = @operator;

        public bool Update()
        {
            if (Left.Value == null || Right.Value == null)
            {
                return false;
            }

            if (Output.Value != null)
            {
                return false;
            }

            Output.Value = Operator switch
            {
                Operator.AND => Left.Value & Right.Value,
                Operator.OR => Left.Value | Right.Value,
                Operator.XOR => Left.Value ^ Right.Value,
                _ => throw new NotImplementedException()
            };

            return true;
        }
    }

    static void Main()
    {
        /**
         * https://adventofcode.com/2024/day/24
         * 
         */

        var input = File.ReadAllText("Data/input.txt");
        var wires = ParseWires(input);
        var gates = ParseGates(input, wires);
        SetStartValues(input, wires);

        var sw1 = Stopwatch.StartNew();
        var part1 = Part1(wires, gates);
        sw1.Stop();
        Console.WriteLine($"Part 1: {part1} in {sw1.ElapsedMilliseconds}ms");

        Part2(wires, gates);
    }

    static long Part1(Wire[] wires, Gate[] gates)
    {
        while (true)
        {
            var updated = false;

            foreach (var gate in gates)
            {
                updated |= gate.Update();
            }

            if (!updated)
            {
                break;
            }
        }

        var wireValues = wires
            .Where(w => w.Name.StartsWith("z"))
            .Select(w => w.Value!.Value ? 1L : 0L);

        var number = wireValues
            .Select((v, i) => v << i)
            .Sum();

        return number;
    }

    static string Part2(Wire[] wires, Gate[] gates)
    {
        /**
         * Part 2 solved through visual inspection. This code generates a .dot file and marks
         * the output wires (z-wires) that are not connected to the correct gates.
         *
         * The correct connections are:
         * - z-wire should be connected to an XOR gate
         * - XOR should be connected to an OR gate as well as a XOR gate
         *
         * Graphviz extension in VS Code was then used to inspect the graph and find wires
         * to re-solder. The following switches were found:
         *
         *      tsw XOR wwm -> hdt
         *      rnk OR mkq -> z05
         *
         *      x09 AND y09 -> z09
         *      vkd XOR wqr -> gbf
         *
         *      y15 XOR x15 -> jgt
         *      y15 AND x15 -> mht
         *
         *      dpr AND nvv -> z30
         *      dpr XOR nvv -> nbf
         */

        using var file = new StreamWriter("Part2.dot");

        PrintHeader();

        foreach (var wire in wires.Where(w => w.Name.StartsWith('z')))
        {
            var parent = gates.FirstOrDefault(g => g.Output == wire && g.Operator == Operator.XOR);
            if (parent is null)
            {
                PrintError(wire);
                continue;
            }
            
            var orGate = gates.FirstOrDefault(g => g.Output == parent.Left && g.Operator == Operator.OR || g.Output == parent.Right && g.Operator == Operator.OR);
            if (orGate is null)
            {
                PrintError(wire);
                continue;
            }
            
            var xorGate = gates.FirstOrDefault(g => g.Output == parent.Left && g.Operator == Operator.XOR || g.Output == parent.Right && g.Operator == Operator.XOR);
            if (xorGate is null)
            {
                PrintError(wire);
                continue;
            }
        }

        foreach (var (gate, i) in gates.Select((g, i) => (g, i)))
        {
            PrintGateWithWires(gate, i);
        }

        PrintFooter();

        return "part2.dot";

        void PrintHeader()
        {
            file.WriteLine("digraph G {");
        }

        void PrintFooter()
        {
            file.WriteLine("}");
        }

        void PrintError(Wire wire)
        {
            Console.WriteLine($"Something wrong with {wire.Name}");
            file.WriteLine($"{wire.Name} [style=filled, color=red, fillcolor=yellow]");
        }

        void PrintGateWithWires(Gate gate, int index)
        {
            var name = $"{gate.Operator}_{index}";
            file.WriteLine($"{gate.Left.Name} -> {name}");
            file.WriteLine($"{gate.Right.Name} -> {name}");
            file.WriteLine($"{name} -> {gate.Output.Name}");
        }
    }

    static Wire[] ParseWires(string input)
    {
        var matches = Gates().Matches(input);

        var wires = matches
            .SelectMany(m =>
            {
                var left = m.Groups["left"].Value;
                var right = m.Groups["right"].Value;
                var output = m.Groups["output"].Value;
                return new[] { left, right, output };
            })
            .Distinct()
            .OrderBy(name => name)
            .Select(name => new Wire(name))
            .ToArray();

        return wires;
    }

    static Gate[] ParseGates(string input, Wire[] wires)
    {
        var wireDict = wires.ToDictionary(w => w.Name);
        var matches = Gates().Matches(input);

        var gates = matches
            .Select(m =>
            {
                var left = wireDict[m.Groups["left"].Value];
                var right = wireDict[m.Groups["right"].Value];
                var output = wireDict[m.Groups["output"].Value];
                var type = Enum.Parse<Operator>(m.Groups["gate"].Value);
                return new Gate(left, right, output, type);
            })
            .ToArray();

        return gates;
    }

    private static void SetStartValues(string input, Wire[] wires)
    {
        var wireDict = wires.ToDictionary(w => w.Name);
        var matches = StartValues().Matches(input);

        foreach (Match match in matches)
        {
            var wire = wireDict[match.Groups["wire"].Value];
            wire.Value = int.Parse(match.Groups["value"].Value) switch
            {
                0 => false,
                1 => true,
                _ => throw new NotImplementedException()
            };
        }
    }

    [GeneratedRegex(@"(?<left>\w+)\s(?<gate>\w+)\s(?<right>\w+)\s->\s(?<output>\w+)")]
    private static partial Regex Gates();

    [GeneratedRegex(@"(?<wire>\w+):\s(?<value>\d+)")]
    private static partial Regex StartValues();
}
