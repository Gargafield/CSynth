﻿using System.Globalization;
using System.Text;

namespace CSynth.Analysis;

public class ControlTree {
    private CFG cfg;
    private Stack<Structure> stack = new();
    private HashSet<Block> visited = new();
    public Structure Structure => stack.Peek();

    private ControlTree(CFG cfg) {
        this.cfg = cfg;
        stack.Push(new LinearStructure());
    }

    public static ControlTree From(CFG cfg) {
        var tree = new ControlTree(cfg);
        tree.Build();
        return tree;
    }

    private void Build() {
        BuildBlocks(cfg.Blocks.First(), cfg.Blocks.ToHashSet());
    }
    
    private void BuildBlocks(Block block, HashSet<Block> blocks) {
        // Depth-first search
        var stack = new Stack<Block>();
        stack.Push(block);
        while (stack.Count > 0) {
            var current = stack.Pop();

            if (!blocks.Contains(current) || visited.Contains(current)) {
                continue;
            }
            visited.Add(current);

            // This block is the head of a loop
            var loop = cfg.Regions.OfType<LoopRegion>().FirstOrDefault(r => r.Header == current);
            if (loop != null) {
                this.stack.Push(new LoopStructure());
            }

            // This block is the head of a branch
            var branch = cfg.Regions.OfType<BranchRegion>().FirstOrDefault(r => r.Header == current);
            if (branch != null) {
                this.stack.Push(new BranchStructure(current));

                foreach (var region in branch.Regions) {
                    this.stack.Push(new LinearStructure());
                    BuildBlocks(region.Header, region.Blocks);
                    var structure = this.stack.Pop();
                    Structure.Children.Add(structure);
                }

                var branchStruct = this.stack.Pop();
                Structure.Children.Add(branchStruct);

                stack.Push(branch.Exit);
            }
            else {
                Structure.Children.Add(new BlockStructure(current));

                foreach (var succ in current.Successors) {
                    if (blocks.Contains(succ) && !visited.Contains(succ)) {
                        stack.Push(succ);
                    }
                }
            }

            // This block is the control of a loop
            loop = cfg.Regions.OfType<LoopRegion>().FirstOrDefault(r => r.Control == current);
            if (loop != null) {
                var loopStruct = this.stack.Pop();
                Structure.Children.Add(loopStruct);
            }
        }
    }

    public override string ToString() {
        return ToString(Structure);
    }

    private string ToString(Structure structure, int indent = 0) {
        var builder = new StringBuilder();
        builder.Append(' ', indent * 2);
        builder.Append(structure.ToString());
        builder.Append('\n');

        foreach (var children in structure.Children) {
            builder.Append(ToString(children, indent + 1));
        }
        return builder.ToString();
    }
}