using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;

namespace Day17;

static class Registers
{
    public static string A = "A";
    public static string B = "B";
    public static string C = "C";
}

enum OpCode
{
    Adv = 0,
    BxL = 1,
    Bst = 2,
    Jnz = 3,
    BxC = 4,
    Out = 5,
    Bdv = 6,
    Cdv = 7,
}

internal class Program
{
    static void Main()
    {
        /**
         * https://adventofcode.com/2024/day/17
         *
         */

        var input = File.ReadAllText("Data/input.txt");

        Console.WriteLine(Part1(input));
        // Console.WriteLine(Part2(input));
    }

    static string Part1(string input)
    {
        var regA = ReadRegister(input, 'A');
        var regB = ReadRegister(input, 'B');
        var regC = ReadRegister(input, 'C');
        var program = ReadProgram(input);
        var computer = new Computer(regA, regB, regC, program);

        PrintState(computer);
        Console.WriteLine();

        while (computer.StepOnce())
        {
            PrintState(computer);
            Console.WriteLine();
        }

        PrintState(computer);
        Console.WriteLine();
        
        return string.Join(",", computer.Output);
    }

    static long Part2(string input)
    {
        /**
         * My program disassembles to:
         *
         *      while a != 0 {
         *          b = a % 8               (0: Bst 4)
         *          b = b ^ 2               (1: BxL 2)
         *          c = a >> b              (2: Cdv 5)
         *          b = b ^ 3               (3: BxL 3)
         *          b = b ^ c               (4: BxC 4)
         *          output.add(b % 8)       (5: Out 5)
         *          a = a >> 3              (6: Adv 3)
         *      }
         *
         * So 'a' is only modified once per iteration by a 3-bit right shift. This already tells
         * us that 'a' has exactly 3-bits per digit in the output. With the output/program 
         * having 16 digits, 'a' is 48-bits long. Brute forcing this is not feasible.
         * 
         * For all the work this program does to modify 'b' and 'c', they are only used as
         * temporary values. In reality the output can be expressed purely as a function
         * of 'a' from a previous iteration.:
         *
         *      while a != 0 {
         *          output.add(((((a % 8) ^ 2) ^ 3) ^ (a >> ((a % 8) ^ 2))) % 8)
         *          a = a >> 3
         *      }
         *
         * In other words, in a brute force approach, we only need to test for values of 'a',
         * not 'b' and 'c'.
         *
         * The next important observation is that 'a' is shifted by 3 bits at the end of each
         * iteration until it is zero. During the LAST iteration, whatever remains, 'a' is now
         * a value in the range [0, 7].
         *
         *      0b_0000_0000_0000_0000_0000_0000_0000_0???
         *
         * The output, based on the problem description, is the last digit in the program. For
         * this last iteration, at least, we can brute force 'a'.
         * 
         * By repeating this process, basically running the program in reverse, and brute
         * forcing the last 3-bits for each iteration we should have a working approach!
         * 
         *      0b_0000_0000_0000_0000_0000_0000_0011_1???
         * 
         * Here we have already brute-forced 3 bits, and are in progress of brute forcing
         * the next triple in order to match the second last digit in the output/program.
         *
         * There is one complication. Each triple might have multiple solutions. We need 
         * to bring these along as we calculate new triples.
         */

        long[] solutions = [0];
        long[] program = [2, 4, 1, 2, 7, 5, 1, 3, 4, 4, 5, 5, 0, 3, 3, 0];

        for (var i = 0; i < 16; i++)
        {
            long[] candidates = [];
            var expected = program[15 - i];

            foreach (var solution in solutions)
            {
                Console.WriteLine();
                Console.WriteLine($"Expected output: {expected}");
                Console.WriteLine($"---------------------------------");

                var regA = solution << 3;

                for (var x = 0; x < 8; x++)
                {
                    var candidate = regA + x;
                    var output = OutputForOneIteration(candidate);

                    Console.WriteLine($"{BinaryLiteral(candidate)}");
                    Console.WriteLine($"x: {x} => output: {output}");

                    if (output == expected)
                    {
                        Console.WriteLine($"Match found!");
                        candidates = [.. candidates, candidate];
                    }
                }
            }

            solutions = candidates;
        }

        static long OutputForOneIteration(long a)
        {
            return ((a % 8) ^ 2 ^ 3 ^ (a >> (int)((a % 8) ^ 2))) % 8;
        }

        return solutions.Min();
    }

    static long ReadRegister(string input, char register)
    {
        var match = Regex.Match(input, @$"Register\s{register}:\s(?<r>\d+)");
        var reg = long.Parse(match.Groups["r"].Value);

        return reg;
    }

    static (OpCode OpCode, long Operand)[] ReadProgram(string input)
    {
        var matches = Regex.Matches(input, @"(?<c>\d),(?<n>\d)");

        var program = matches.Select(m =>
        {
            var opCode = Enum.Parse<OpCode>(m.Groups["c"].Value);
            var operand = long.Parse(m.Groups["n"].Value);
            return (opCode, operand);
        })
        .ToArray();

        return program;
    }

    static void PrintState(Computer computer)
    {
        var regA = computer.RegA;
        var regB = computer.RegB;
        var regC = computer.RegC;
        var ptr = computer.Ptr;
        var program = computer.Program;
        var output = computer.Output;
        Console.WriteLine($"Register A: {regA} {BinaryLiteral(regA)}");
        Console.WriteLine($"Register B: {regB}");
        Console.WriteLine($"Register C: {regC}");
        Console.WriteLine($"Pointer: {ptr}");
        Console.WriteLine($"Program: {string.Join(", ", program.Select((p, i) => $"({i}: {p.OpCode} {p.Operand})"))}");
        Console.WriteLine($"Output: {string.Join(", ", output)}");
    }

    static string BinaryLiteral(long value)
    {
        var bits = Convert.ToString(value, 2).PadLeft(64, '0');
        var literals = string.Join("_", Enumerable.Range(0, 16).Select(i => bits.Substring(i * 4, 4)));
        return $"0b_{literals}";
    }
}

internal class Computer(long regA, long regB, long regC, (OpCode OpCode, long Operand)[] program)
{
    /**
     * 3-bit for instructions
     * 3-bit for operands
     * 64?-bit for registers
     */

    public long RegA { get; private set; } = regA;
    public long RegB { get; private set; } = regB;
    public long RegC { get; private set; } = regC;
    public (OpCode OpCode, long Operand)[] Program { get; init; } = program;
    public long Ptr { get; private set; }
    public long[] Output { get; private set; } = [];

    public void RunToCompletion()
    {
        while (StepOnce()) ;
    }

    public bool StepOnce()
    {
        if (Ptr >= Program.Length)
        {
            return false;
        }

        var (opCode, operand) = Program[Ptr];
        switch (opCode)
        {
            case OpCode.Adv:
                RegA = RegA / (1 << (int)Combo(RegA, RegB, RegC, operand));
                break;
            case OpCode.BxL:
                RegB = RegB ^ Literal(operand);
                break;
            case OpCode.Bst:
                RegB = Combo(RegA, RegB, RegC, operand) % 8;
                break;
            case OpCode.Jnz:
                Ptr = (RegA == 0) ? Ptr + 1 : Literal(operand) >> 1;
                return true;
            case OpCode.BxC:
                RegB = RegB ^ RegC;
                break;
            case OpCode.Out:
                var value = Combo(RegA, RegB, RegC, operand) % 8;
                Output = [.. Output, value];
                break;
            case OpCode.Bdv:
                RegB = RegA / (1 << (int)Combo(RegA, RegB, RegC, operand));
                break;
            case OpCode.Cdv:
                RegC = RegA / (1 << (int)Combo(RegA, RegB, RegC, operand));
                break;
        }

        Ptr++;
        return true;
    }

    private static long Literal(long operand)
    {
        return operand;
    }

    private static long Combo(long regA, long regB, long regC, long operand)
    {
        return operand switch
        {
            0 => 0,
            1 => 1,
            2 => 2,
            3 => 3,
            4 => regA,
            5 => regB,
            6 => regC,
            _ => throw new ArgumentOutOfRangeException(nameof(operand))
        };
    }
}