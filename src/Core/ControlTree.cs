﻿using System.Text;

namespace CSynth.Core;

public class ControlTree {
    private CFG cfg;
    private Stack<Structure> stack = new();
    private HashSet<int> visited = new();
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
        BuildBlocks(cfg.Blocks.First(), cfg.Blocks.GetEnumerableIds().ToHashSet());
    }
    
    private void BuildBlocks(int block, ICollection<int> blocks) {
        // Depth-first search
        var stack = new Stack<int>();
        stack.Push(block);
        while (stack.Count > 0) {
            var currentId = stack.Pop();
            var current = cfg.Blocks[currentId];

            // This block is the head of a loop
            var loop = cfg.Regions.OfType<LoopRegion>().FirstOrDefault(r => r.Header == current);

            if (!blocks.Contains(currentId)
            || visited.Contains(currentId)
            || (!cfg.Blocks.Predecessors(currentId).All(visited.Contains) && loop == null)) {
                continue;
            }
            visited.Add(currentId);

            if (loop != null) {
                this.stack.Push(new LoopStructure());
            }

            // This block is the head of a branch
            var branch = cfg.Regions.OfType<BranchRegion>().FirstOrDefault(r => r.Header == current);
            if (branch != null) {
                this.stack.Push(new BranchStructure(current));

                foreach (var region in branch.Regions) {
                    this.stack.Push(new LinearStructure());
                    BuildBlocks(region.Header.Id, region.Blocks);
                    var structure = this.stack.Pop();
                    (Structure as BranchStructure)!.AddCondition(structure, region.Condition);
                }

                var branchStruct = this.stack.Pop();
                Structure.Children.Add(branchStruct);

                stack.Push(branch.Exit.Id);
            }
            else {
                Structure.Children.Add(current);

                foreach (var succ in cfg.Blocks.Successors(current.Id)) {
                    if (blocks.Contains(succ) && !visited.Contains(succ)) {
                        stack.Push(succ);
                    }
                }
            }

            // This block is the control of a loop
            loop = cfg.Regions.OfType<LoopRegion>().FirstOrDefault(r => r.Control == current);
            if (loop != null) {
                var loopStruct = this.stack.Pop();
                if (loopStruct is not LoopStructure)
                    throw new InvalidOperationException("Expected loop structure");
                
                Structure.Children.Add(loopStruct);
            }
        }
    }

    public override string ToString() {
        return ToString(Structure).TrimEnd();
    }

    private string ToString(object obj, int indent = 0) {
        if (obj is Block block) {
            return $"{new string(' ', indent * 2)}{block}\n";
        }

        var builder = new StringBuilder();
        builder.Append(' ', indent * 2);
        builder.Append($"{obj}\n");

        if (obj is Structure structure) {
            foreach (var children in structure.Children) {
                builder.Append(ToString(children, indent + 1));
            }
        }

        return builder.ToString();
    }
}
