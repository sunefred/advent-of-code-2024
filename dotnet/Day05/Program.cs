namespace Day05;

internal class Program
{
    static void Main()
    {
        /**
         * https://adventofcode.com/2024/day/5
         * 
         */

        var input = File.ReadAllText("Data/input.txt");
        var sections = input.Trim().Split(Environment.NewLine + Environment.NewLine);
        var rules = ParseRules(sections[0]);
        var updates = ParseUpdates(sections[1]);

        Console.WriteLine(Part1(rules, updates));
        Console.WriteLine(Part2(rules, updates));
    }

    static int Part1(List<(int X, int Y)> rules, List<List<int>> updates)
    {
        var validUpdates = updates.Where(update => IsValidUpdate(update, rules)).ToList();
        var middlePages = validUpdates.Select(update => update.ElementAt(update.Count / 2)).ToList();

        return middlePages.Sum();
    }

    static int Part2(List<(int X, int Y)> rules, List<List<int>> updates)
    {
        var invalidUpdates = updates.Where(update => !IsValidUpdate(update, rules)).ToList();
        var reorderedUpdates = invalidUpdates.Select(update => ReorderUpdate(update, rules)).ToList();
        var middlePages = reorderedUpdates.Select(update => update.ElementAt(update.Count / 2)).ToList();

        return middlePages.Sum();
    }

    static List<(int X, int Y)> ParseRules(string rulesSection)
    {
        return rulesSection
            .Split(Environment.NewLine)
            .Select(line => {
                var parts = line.Split('|').Select(int.Parse).ToArray();
                return (parts[0], parts[1]);
            })
            .ToList();
    }

    static List<List<int>> ParseUpdates(string updatesSection)
    {
        return updatesSection
            .Split(Environment.NewLine)
            .Select(line => line.Split(',').Select(int.Parse).ToList())
            .ToList();
    }

    static bool IsValidUpdate(List<int> update, List<(int X, int Y)> rules)
    {
        /*
         * For each rule, x|y, x must appear before y in the update
         */

        foreach (var (x, y) in rules)
        {
            if (update.Contains(x) && update.Contains(y))
            {
                if (update.IndexOf(x) > update.IndexOf(y))
                {
                    return false;
                }
            }
        }
        
        return true;
    }

    public class DagNode(int value)
    {
        public int Value { get; init; } = value;
        public List<DagNode> Parents { get; set; } = [];
        public List<DagNode> Children { get; set; } = [];
        public bool Visited { get; set; }
    }

    static List<int> ReorderUpdate(List<int> update, List<(int X, int Y)> rules)
    {
        List<int> sortedOrder = [];

        /**
         * Extract only the relevant rules, rules where both x and y are present in the update.
         *
         * Construct a DAG from the relevant rules and perform a topological sort to reorder
         * the update. A topological search is a traversal where a node is visited only after
         * all of its dependencies have been visited.
         *
         */

        var relevantRules = rules.Where(rule => update.Contains(rule.X) && update.Contains(rule.Y)).ToList();

        var dagGraph = update
            .Select(page => (page, new DagNode(page)))
            .ToDictionary(x => x.page, x => x.Item2);

        foreach (var node in dagGraph.Values)
        {
            node.Children = [.. relevantRules.Where(r => r.X == node.Value).Select(r => dagGraph[r.Y])];
            node.Parents = [.. relevantRules.Where(r => r.Y == node.Value).Select(r => dagGraph[r.X])];
        }

        var queue = new Queue<DagNode>(dagGraph.Values.Where(node => node.Parents.Count == 0));
        while (queue.Count > 0)
        {
            var node = queue.Dequeue();
            node.Visited = true;

            if (update.Contains(node.Value))
            {
                sortedOrder.Add(node.Value);
            }

            foreach (var child in node.Children)
            {
                if (child.Parents.All(x => x.Visited))
                {
                    queue.Enqueue(child);
                }
            }
        }

        return sortedOrder;
    }
}
