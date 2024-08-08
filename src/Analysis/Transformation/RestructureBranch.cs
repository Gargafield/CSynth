
namespace CSynth.Analysis.Transformation;

public class RestructureBranch
{
    private CFG cfg;
    private BranchBlock branch = default!;
    private HashSet<Block> set = new();

    private RestructureBranch(CFG cfg) {
        this.cfg = cfg;
    }

    public static void Restructure(CFG cfg) {
        var restructure = new RestructureBranch(cfg);

        restructure.set = cfg.Blocks.ToHashSet();

        var stack = new Stack<Block>();
        stack.Push(cfg.Blocks.First());

        while (stack.Count > 0) {
            var head = stack.Pop();
            var branch = restructure.FindBranch(head);
            if (branch == null)
                continue;

            restructure.branch = branch;
            restructure.RestructureSingle();

            foreach (var successor in branch.Successors) {
                stack.Push(successor);
            }
        }
    }

    private void RestructureSingle() {
        var (regions, continuations) = ConstructBranchRegions(branch);

        Block exit;
        if (continuations.Count > 1)
            exit = ConstructSingleExit(regions, continuations.ToList());
        else
            exit = continuations.First();
        
        BranchRegion.Create(cfg, branch, exit, regions);
    }

    private Block ConstructSingleExit(List<BranchRegion.Region> regions, List<Block> continuations) {
        if (continuations.Count <= 1) {
            return continuations.FirstOrDefault()!;
        }

        var variable = BlockVariable.BranchControl;
        var control = BranchBlock.Create(cfg, variable);
        
        for (int i = 0; i < continuations.Count; i++) {
            control.AddBranch(i, continuations[i]);
        }

        foreach (var region in regions) {
            var exits = region.Where(b => b.Successors.Intersect(continuations).Any()).ToHashSet();

            Block target = control;
            if (exits.Count > 1) {
                target = NoopBlock.Create(cfg);
                target.AddTarget(control);
                region.Blocks.Add(target);
            }

            foreach (var continuation in continuations) {
                foreach (var exit in continuation.Predecessors.Intersect(exits).ToArray()) {
                    var assigment = AssignmentBlock.Create(cfg);
                    assigment.AddVariable(variable, continuations.IndexOf(continuation));
                    exit.ReplaceTarget(continuation, assigment);
                    assigment.AddTarget(target);

                    region.Blocks.Add(assigment);
                }
            }
        }

        return control;
    }

    private Tuple<List<BranchRegion.Region>, HashSet<Block>> ConstructBranchRegions(BranchBlock branch) {
        var regions = new List<BranchRegion.Region>();
        var continuations = new HashSet<Block>();
        
        foreach (var (successor, condition) in branch.Branches.ToList()) {
            HashSet<Block> blocks;
            Block head = successor;

            if (successor.Predecessors.Count > 1) {
                // Empty branch region, construct noop
                var noop = NoopBlock.Create(cfg);
                branch.ReplaceTarget(successor, noop);
                noop.AddTarget(successor);
                blocks = new HashSet<Block>() { noop };
                head = noop;

                continuations.Add(successor);
            }
            else {
                // Discover all blocks that have succesesor as dominator
                blocks = new HashSet<Block>() { successor };
                var stack = new Queue<Block>();
                stack.Enqueue(successor);

                while (stack.Count > 0) {
                    var block = stack.Dequeue();

                    blocks.Add(block);
                    foreach (var succ in block.Successors) {
                        if (!blocks.Contains(succ) && succ.Predecessors.All(blocks.Contains))
                            stack.Enqueue(succ);
                    }
                }

                 // Find continuations in blocks
                foreach (var block in blocks) {
                    foreach (var succ in block.Successors) {
                        if (!blocks.Contains(succ))
                            continuations.Add(succ);
                    }
                }
            }

            regions.Add(new BranchRegion.Region(head, blocks, condition));
        }

        return Tuple.Create(regions, continuations);
    }

    private BranchBlock? FindBranch(Block head) {
        while (head != null) {
            if (head.Successors.Count > 1 && set.Contains(head)) {
                return head as BranchBlock;
            }

            set.Remove(head);
            head = head.Successors.FirstOrDefault()!;
        }

        return null;
    }

    public Dictionary<Block, HashSet<Block>> FindDominants()
    {
        // Initialize dominants: each node dominates all others initially
        var dominants = cfg.Blocks.ToDictionary(node => node, node => new HashSet<Block>(cfg.Blocks.Where(n => n != node)));

        bool changed;
        do
        {
            changed = false;
            foreach (var node in cfg.Blocks)
            {
                var currentDominants = new HashSet<Block>(dominants[node]);
                var predDominants = node.Predecessors.Any() ? cfg.Blocks.ToHashSet() : new HashSet<Block>();

                foreach (var pred in node.Predecessors)
                {
                    if (predDominants.SetEquals(cfg.Blocks)) // First iteration
                    {
                        predDominants = new HashSet<Block>(dominants[pred]);
                    }
                    else
                    {
                        predDominants.IntersectWith(dominants[pred]);
                    }        
                }

                predDominants.Add(node); // A node always dominates itself
                dominants[node] = predDominants;

                // Check if dominants set changed
                if (!currentDominants.SetEquals(predDominants))
                {
                    changed = true;
                }
            }
        } while (changed); // Repeat until no changes

        return dominants;
    }

}
