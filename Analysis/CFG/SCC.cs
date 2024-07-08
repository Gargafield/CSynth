namespace CSynth.Analysis;

public static class SCC {
    public static List<HashSet<Block>> ComputeSCC(CFG cfg) {
        // Make use of the fact that we know predecessors and successors
        var sccs = new List<HashSet<Block>>();
        var index = 0;
        var stack = new Stack<Block>();
        var indices = new Dictionary<Block, int>();
        var lowlinks = new Dictionary<Block, int>();
        var onStack = new HashSet<Block>();

        void StrongConnect(Block node) {
            indices[node] = index;
            lowlinks[node] = index;
            index++;
            stack.Push(node);
            onStack.Add(node);

            foreach (var successor in node.Successors) {
                if (!indices.ContainsKey(successor)) {
                    StrongConnect(successor);
                    lowlinks[node] = Math.Min(lowlinks[node], lowlinks[successor]);
                } else if (onStack.Contains(successor)) {
                    lowlinks[node] = Math.Min(lowlinks[node], indices[successor]);
                }
            }

            if (lowlinks[node] == indices[node]) {
                var scc = new HashSet<Block>();
                Block w;
                do {
                    w = stack.Pop();
                    onStack.Remove(w);
                    scc.Add(w);
                } while (w != node);
                sccs.Add(scc);
            }
        }

        foreach (var node in cfg.Blocks) {
            if (!indices.ContainsKey(node)) {
                StrongConnect(node);
            }
        }

        return sccs;
    }
}
