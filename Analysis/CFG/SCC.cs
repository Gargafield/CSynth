namespace CSynth.Analysis;

public static class SCC {
    // Tarjan's strongly connected components algorithm
    public static List<List<Block>> ComputeSCC(CFG cfg) {
        var index = 0;
        var stack = new Stack<Block>();
        var indices = new Dictionary<Block, int>();
        var lowlinks = new Dictionary<Block, int>();
        var onStack = new HashSet<Block>();
        var scc = new List<List<Block>>();

        void StrongConnect(Block v) {
            indices[v] = index;
            lowlinks[v] = index;
            index++;
            stack.Push(v);
            onStack.Add(v);

            foreach (var w in v.Targets) {
                if (!indices.ContainsKey(w)) {
                    StrongConnect(w);
                    lowlinks[v] = Math.Min(lowlinks[v], lowlinks[w]);
                } else if (onStack.Contains(w)) {
                    lowlinks[v] = Math.Min(lowlinks[v], indices[w]);
                }
            }

            if (lowlinks[v] == indices[v]) {
                var component = new List<Block>();
                Block w;
                do {
                    w = stack.Pop();
                    onStack.Remove(w);
                    component.Add(w);
                } while (w != v);
                scc.Add(component);
            }
        }

        foreach (var v in cfg) {
            if (!indices.ContainsKey(v))
                StrongConnect(v);
        }

        return scc;
    }
}
