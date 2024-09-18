using System.Collections;
using System.Text;

namespace CSynth.AST;

public class CFG : IEnumerable<Block> {
    public BlockCollection Blocks { get; set; }
    public RegionCollection Regions { get; set; }

    public CFG() {
        Blocks = new BlockCollection();
        Regions = new RegionCollection(Blocks);
    }
    
    public static CFG FromEquality(string text) {
        /*
        Input:
        0[1]
        1[2]
        2[]
        */

        var cfg = new CFG();

        var split = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        var blocks = new Dictionary<Block, List<int>>();

        var counter = 0;
        foreach (var line in split) {
            var _split = line.Split('[');
            var id = int.Parse(_split[0]);
            var targetStr = _split[1].Substring(0, _split[1].Length - 1);

            var targets = targetStr.Length == 0
                ? Enumerable.Empty<int>().ToList()
                : targetStr.Split(',').Select(s => int.Parse(s)).ToList();

            if (id != counter++)
                throw new ArgumentException("Id not sorted", nameof(text));
            
            if (targets.Count > 1) {
                var block = BranchBlock.Create(cfg.Blocks, "x");
                blocks.Add(block, targets.ToList());
                continue;
            }
            else {
                var block = NoopBlock.Create(cfg.Blocks);
                blocks.Add(block, targets.ToList());
            }
        }

        foreach (var (block, targetIds) in blocks) {
            foreach (var targetId in targetIds) {
                var target = cfg.Blocks[targetId];
                if (block is BranchBlock branchBlock) {
                    cfg.Blocks.AddEdge(branchBlock.Id, target.Id);
                }
                else {
                    cfg.Blocks.AddEdge(block.Id, target.Id);
                }
            }
        }

        return cfg;
    }

    public IEnumerator<Block> GetEnumerator() {
        return Blocks.Blocks.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }

    public void Print()
    {
        foreach (var blockId in Blocks.GetEnumerableIds()) {
            Console.WriteLine($"Block {blockId}");

            foreach (var target in Blocks.Successors(blockId)) {
                Console.WriteLine($"  -> {target}");
            }

            var block = Blocks[blockId];
            if (block is BasicBlock basicBlock) {
                foreach (var instruction in basicBlock.Instructions) {
                    Console.WriteLine(instruction);
                }
            }
            
            Console.WriteLine();
        }
    }

    public string ToDot() {
        // https://github.com/Rerumu/Flow-Structurer/blob/trunk/fuzz/fuzz_targets/sample/list.rs#L53
        var builder = new StringBuilder();

        builder.AppendLine("digraph G {");
        builder.AppendLine("  node [shape = box, style = filled, ordering = out];");

        foreach (var blockId in Blocks.GetEnumerableIds()) {
            var block = Blocks[blockId];

            var label = block switch {
                BasicBlock basicBlock => string.Join(Environment.NewLine, basicBlock.Instructions),
                BranchBlock branchBlock => $"Branch {branchBlock.Variable}",
                AssignmentBlock assignmentBlock => string.Join(Environment.NewLine, assignmentBlock.Instructions.Select(i => $"{i.Key} = {i.Value}")),
                _ => ""
            };
            var regionId = ((List<Region>)Regions).LastOrDefault(r => r.Blocks.Contains(block.Id))?.Id;
            var group = regionId == null ? "" : $"{regionId}";

            
            builder.AppendLine($"  N{block.Id} [xlabel = {block.Id}, label = \"{label.Replace('"', '\'')}\", group = \"{group}\"];");
            
            foreach (var target in Blocks.Successors(blockId)) {
                builder.AppendLine($"  N{block.Id} -> N{target};");
            }
        }

        builder.AppendLine("}");

        return builder.ToString();
    }

    public string ToMermaid() {
        var builder = new StringBuilder();
        builder.AppendLine("graph TD");
        foreach (var blockId in Blocks.GetEnumerableIds()) {
            foreach (var target in Blocks.Successors(blockId)) {
                builder.AppendLine($"  {blockId} --> {target}");
            }
        }
        return builder.ToString();
    }

    public string ToEquality() {
        var builder = new StringBuilder();
        foreach (var block in Blocks.GetEnumerableIds()) {
            var targets = Blocks.Successors(block).Order();
            builder.AppendLine($"{block}[{string.Join(',', targets)}]");
        }

        return builder.ToString().TrimEnd();
    }

    public void PrintMermaid()
    {
        Console.WriteLine(ToMermaid());
    }

    public override string ToString() {
        return ToMermaid();
    }
}
