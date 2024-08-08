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

        var split = text.Split(Environment.NewLine);
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
            counter = 0;
            foreach (var targetId in targetIds) {
                var target = cfg.Blocks[targetId];
                if (block is BranchBlock branchBlock) {
                    branchBlock.AddBranch(counter++, target);
                }
                else {
                    block.AddTarget(target);
                }
            }
        }

        return cfg;
    }

    public IEnumerator<Block> GetEnumerator() {
        return Blocks.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }

    public void Print()
    {
        foreach (var block in Blocks) {
            Console.WriteLine($"Block {block.Id}");

            foreach (var target in block.Successors) {
                Console.WriteLine($"  -> {target}");
            }

            if (block is BasicBlock basicBlock) {
                foreach (var instruction in basicBlock.Statements) {
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

        foreach (var block in Blocks) {
            var label = block switch {
                BasicBlock basicBlock => string.Join(Environment.NewLine, basicBlock.Statements),
                BranchBlock branchBlock => $"Branch {branchBlock.Variable}",
                _ => ""
            };
            var regionId = ((List<Region>)Regions).LastOrDefault(r => r.Blocks.Contains(block.Id))?.Id;
            var group = regionId == null ? "" : $"{regionId}";

            
            builder.AppendLine($"  N{block.Id} [xlabel = {block.Id}, label = \"{label.Replace('"', '\'')}\", group = \"{group}\"];");
            
            foreach (var target in block.Successors) {
                builder.AppendLine($"  N{block.Id} -> N{target};");
            }
        }

        builder.AppendLine("}");

        return builder.ToString();
    }

    public string ToMermaid() {
        var builder = new StringBuilder();
        builder.AppendLine("graph TD");
        foreach (var block in Blocks) {
            foreach (var target in block.Successors) {
                builder.AppendLine($"  {block.Id} --> {target}");
            }
        }
        return builder.ToString();
    }

    public string ToEquality() {
        var builder = new StringBuilder();
        foreach (var block in ((List<Block>)Blocks).OrderBy(b => b.Id)) {
            var targets = block.Successors.Order();
            builder.AppendLine($"{block.Id}[{string.Join(',', targets)}]");
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
