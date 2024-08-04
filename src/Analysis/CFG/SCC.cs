namespace CSynth.Analysis;

public static class SCC {

    public static void DFS(Block node, Stack<Block> stack, HashSet<Block> visited) {
        visited.Add(node);

        foreach (var successor in node.Successors) {
            if (!visited.Contains(successor)) {
                DFS(successor, stack, visited);
            }
        }

        stack.Push(node);
    }

    public static void ConnectSCC(Block node, HashSet<Block> scc, HashSet<Block> visited) {
        visited.Add(node);
        scc.Add(node);

        foreach (var predecessor in node.Predecessors) {
            if (!visited.Contains(predecessor)) {
                ConnectSCC(predecessor, scc, visited);
            }
        }
    }

    public static List<HashSet<Block>> ComputeSCC(CFG cfg) {
        
        var visited = new HashSet<Block>();
        var stack = new Stack<Block>();

        foreach (var block in cfg.Blocks) {
            if (!visited.Contains(block))
                DFS(block, stack, visited);
        }

        visited.Clear();
        var result = new List<HashSet<Block>>();

        while (stack.Count > 0) {
            var node = stack.Pop();
            if (visited.Contains(node))
                continue;
            

            var scc = new HashSet<Block>();
            ConnectSCC(node, scc, visited);

            result.Add(scc);
        }

        return result;
    }
}
