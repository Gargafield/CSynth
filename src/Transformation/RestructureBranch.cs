
using System.Buffers;
using CSynth.AST;

namespace CSynth.Transformation;

public class RestructureBranch
{
    public class Region {
        public required HashSet<int> Blocks { get; set; }
        public int Header { get; set; }
        public int Condition { get; set; }
    }

    private CFG cfg;
    private BlockCollection blocks;
    private RegionCollection regions;

    private int branch = default!;
    private HashSet<int> set = new();

    private RestructureBranch(CFG cfg) {
        this.cfg = cfg;

        blocks = cfg.Blocks;
        regions = cfg.Regions;
    }

    public static void Restructure(CFG cfg) {
        var restructure = new RestructureBranch(cfg);

        restructure.set = cfg.Blocks.GetEnumerableIds().ToHashSet();

        var stack = new Stack<int>();
        stack.Push(cfg.Blocks.First());

        while (stack.Count > 0) {
            var head = stack.Pop();

            var branch = restructure.FindBranch(head);
            if (branch == null)
                continue;

            restructure.branch = branch.Value;
            restructure.RestructureSingle();

            foreach (var successor in cfg.Blocks.Successors(branch.Value)) {
                stack.Push(successor);
            }
        }
    }

    private void RestructureSingle() {
        var (regionsArray, numRegions, continuations) = ConstructBranchRegions(branch);
        var regions = regionsArray.AsSpan(0, numRegions);

        int exit;
        if (continuations.Count > 1)
            exit = ConstructSingleExit(regions, continuations.ToList());
        else
            exit = continuations.First();
        
        var branchRegion = cfg.Regions.AddBranchRegion(branch, exit);
        foreach (var region in regions) {
            this.regions.AddRegionToBranch(branchRegion, region.Blocks, region.Header, region.Condition);
        }
        ArrayPool<Region>.Shared.Return(regionsArray);
    }

    private int ConstructSingleExit(Span<Region> regions, List<int> continuations) {
        if (continuations.Count <= 1) {
            return continuations.FirstOrDefault()!;
        }

        var variable = BranchVariable.BranchControl;
        var control = blocks.AddBranch(variable);
        
        for (int i = 0; i < continuations.Count; i++) {
            blocks.AddEdge(control, continuations[i]);
        }

        foreach (var region in regions) {
            var exits = region.Blocks.Where(b => blocks.Successors(b).Intersect(continuations).Any()).ToArray();

            int target = control;
            if (exits.Length > 1) {
                target = blocks.AddNoop();
                blocks.AddEdge(target, control);
                region.Blocks.Add(target);
            }

            for (int i = 0; i < continuations.Count; i++) {
                var continuation = continuations[i];

                foreach (var exit in blocks.Predecessors(continuation).Intersect(exits).ToArray()) {
                    var assigment = blocks.AddAssignment(variable, i);
                    blocks.AddEdge(assigment, target);
                    blocks.ReplaceEdge(exit, continuation, assigment);

                    region.Blocks.Add(assigment);
                }
            }
        }

        return control;
    }

    private Tuple<Region[], int, HashSet<int>> ConstructBranchRegions(int branch) {
        var succesesor = blocks.Successors(branch);
        var regions = ArrayPool<Region>.Shared.Rent(succesesor.Count);
        var continuations = new HashSet<int>();
        
        for (int i = 0; i < succesesor.Count; i++) {
            var successor = succesesor[i];
            HashSet<int> blocks;
            int head = successor;

            if (this.blocks.Predecessors(successor).Count > 1) {
                // Empty branch region, construct noop
                var noop = this.blocks.AddNoop();
                this.blocks.AddEdge(noop, successor);
                this.blocks.ReplaceEdge(branch, successor, noop);

                blocks = new HashSet<int>() { noop };
                head = noop;

                continuations.Add(successor);
            }
            else {
                // Discover all blocks that have succesesor as dominator
                blocks = new HashSet<int>();
                var stack = new Queue<int>();
                stack.Enqueue(successor);

                while (stack.Count > 0) {
                    var block = stack.Dequeue();

                    blocks.Add(block);
                    foreach (var succ in this.blocks.Successors(block)) {
                        var predessors = this.blocks.Predecessors(succ);
                        if (!blocks.Contains(succ) && (predessors.Count == 1 || predessors.All(blocks.Contains)))
                            stack.Enqueue(succ);
                    }
                }

                 // Find continuations in blocks
                foreach (var block in blocks) {
                    foreach (var succ in this.blocks.Successors(block)) {
                        if (!blocks.Contains(succ))
                            continuations.Add(succ);
                    }
                }
            }

            regions[i] = new Region {
                Blocks = blocks,
                Header = head,
                Condition = i
            };
        }

        return Tuple.Create(regions, succesesor.Count, continuations);
    }

    private int? FindBranch(int head) {
        var succesesor = blocks.Successors(head);
        while (succesesor.Count > 0) {
            if (succesesor.Count > 1 && set.Contains(head)) {
                return head;
            }

            set.Remove(head);
            head = succesesor[0];

            succesesor = blocks.Successors(head);
        }

        return null;
    }
}
