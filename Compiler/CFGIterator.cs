using CSynth.Analysis;

namespace CSynth.Compiler;

public static class CFGIterator {
    public static IEnumerable<Block> Iterate(CFG cfg) {
        var visited = new HashSet<Block>();
        var stack = new Stack<Block>();
        stack.Push(cfg.Blocks.First());
        while (stack.Count > 0) {
            var block = stack.Pop();
            if (visited.Contains(block))
                continue;
            visited.Add(block);
            yield return block;
            foreach (var successor in block.Successors)
                stack.Push(successor);
        }
    }
}
