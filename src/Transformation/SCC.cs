using CSynth.AST;

namespace CSynth.Transformation;

public static class SCC {

    public static void DFS(BlockCollection nodes, int node, Stack<int> stack, HashSet<int> visited) {
        visited.Add(node);

        foreach (var successor in nodes.Successors(node)) {
            if (!visited.Contains(successor)) {
                DFS(nodes, successor, stack, visited);
            }
        }

        stack.Push(node);
    }

    public static void ConnectSCC(BlockCollection nodes, int node, List<int> scc, HashSet<int> visited) {
        visited.Add(node);
        scc.Add(node);

        foreach (var predecessor in nodes.Predecessors(node)) {
            if (!visited.Contains(predecessor)) {
                ConnectSCC(nodes, predecessor, scc, visited);
            }
        }
    }

    public static List<List<int>> ComputeSCC(BlockCollection nodes) {
        
        var visited = new HashSet<int>();
        var stack = new Stack<int>();

        foreach (var node in nodes.GetEnumerableIds()) {
            if (!visited.Contains(node))
                DFS(nodes, node, stack, visited);
        }

        visited.Clear();
        var result = new List<List<int>>();

        while (stack.Count > 0) {
            var node = stack.Pop();
            if (visited.Contains(node))
                continue;

            var scc = new List<int>();
            ConnectSCC(nodes, node, scc, visited);

            result.Add(scc);
        }

        return result;
    }
}
