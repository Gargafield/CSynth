
namespace CSynth.Core;

public class SCC {
    private BlockCollection nodes;
    private HashSet<int> dfs_visited = new();
    private HashSet<int> scc_visited = new();

    private Stack<int> stack = new();
    private Stack<List<int>> sccs = new();

    public int Count => sccs.Count;
    public List<int> Pop() => sccs.Pop();

    public SCC(BlockCollection nodes) {
        this.nodes = nodes;
    }

    public void FindSCCs(IEnumerable<int> blocks) {
        dfs_visited.Clear();
        scc_visited.Clear();
        stack.Clear();

        foreach (var block in blocks) {
            scc_visited.Add(block);
        }

        foreach (var block in blocks) {
            if (!dfs_visited.Contains(block)) {
                DFS(nodes, block);
            }
        }

        scc_visited.Clear();
        foreach (var block in stack) {
            if (!scc_visited.Contains(block)) {
                var scc = new List<int>();
                ConnectSCC(nodes, block, scc);
                sccs.Push(scc);
            }
        }
    }
    
    public void DFS(BlockCollection nodes, int node) {
        dfs_visited.Add(node);

        foreach (var successor in nodes.Successors(node)) {
            if (!dfs_visited.Contains(successor) && scc_visited.Contains(successor)) {
                DFS(nodes, successor);
            }
        }

        stack.Push(node);
    }

    public void ConnectSCC(BlockCollection nodes, int node, List<int> scc) {
        scc_visited.Add(node);
        scc.Add(node);

        foreach (var predecessor in nodes.Predecessors(node)) {
            if (!scc_visited.Contains(predecessor) && dfs_visited.Contains(predecessor)) {
                ConnectSCC(nodes, predecessor, scc);
            }
        }
    }
}
