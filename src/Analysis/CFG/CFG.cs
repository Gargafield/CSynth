﻿using System.Collections;
using System.Text;

namespace CSynth.Analysis;

public class CFG : IEnumerable<Block> {
    public BlockCollection Blocks { get; set; } = new();
    public RegionCollection Regions { get; set; } = new();

    public CFG() { }

    public static CFG FromEquality(string text) {
        /*
        Input:
        0[1]
        1[2]
        2[]
        */

        var cfg = new CFG();

        var split = text.Split(Environment.NewLine);
        var blocks = new Dictionary<Block, List<BlockId>>();

        var counter = 0;
        foreach (var line in split) {
            var _split = line.Split('[');
            var id = int.Parse(_split[0]);
            var targetStr = _split[1].Substring(0, _split[1].Length - 1);

            var targets = targetStr.Length == 0
                ? Enumerable.Empty<BlockId>()
                : targetStr.Split(',').Select(s => (BlockId)int.Parse(s));

            if (id != counter++)
                throw new ArgumentException("Id not sorted", nameof(text));
            
            var block = BasicBlock.Create(cfg);
            blocks.Add(block, targets.ToList());
        }

        foreach (var (block, targetIds) in blocks) {
            foreach (var targetId in targetIds) {
                var target = cfg.Blocks[targetId];
                block.AddTarget(target);
            }
        }

        return cfg;
    }

    public Block this[BlockId index] {
        get => Blocks[index];
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
                Console.WriteLine($"  -> {target.Id}");
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
            var regionId = Regions.LastOrDefault(r => r.Blocks.Contains(block))?.Id;
            var group = regionId == null ? "" : $"{regionId}";

            
            builder.AppendLine($"  N{block.Id} [xlabel = {block.Id}, label = \"{label.Replace('"', '\'')}\", group = \"{group}\"];");
            
            foreach (var target in block.Successors) {
                builder.AppendLine($"  N{block.Id} -> N{target.Id};");
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
                builder.AppendLine($"  {block.Id} --> {target.Id}");
            }
        }
        return builder.ToString();
    }

    public string ToEquality() {
        var builder = new StringBuilder();
        foreach (var block in Blocks.OrderBy(b => b.Id)) {
            var targets = block.Successors.OrderBy(b => b.Id);
            builder.AppendLine($"{block.Id}[{string.Join(',', targets.Select(t => (int)t.Id))}]");
        }

        return builder.ToString().TrimEnd();
    }

    public void PrintMermaid()
    {
        Console.WriteLine(ToMermaid());
    }
}
