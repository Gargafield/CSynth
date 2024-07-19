namespace CSynth.Analysis.Transformation;

public class RestructureBranch
{
    private CFG cfg;
    private Block branch = default!;
    private Dictionary<Block, HashSet<Block>> dominance = new();
    private HashSet<Block> set = new();

    private RestructureBranch(CFG cfg) {
        this.cfg = cfg;
    }

    public static void Restructure(CFG cfg) {

        var restructure = new RestructureBranch(cfg);

        restructure.set = cfg.Blocks.ToHashSet();
        restructure.dominance = restructure.FindDominants();

        var stack = new Stack<Block>();
        stack.Push(cfg.Blocks.First());

        while (stack.Peek() != null) {
            var head = stack.Pop();
            var branch = restructure.FindBranch(head);
            if (branch == null)
                break;
            
            restructure.branch = branch;
            restructure.RestructureSingle();

            foreach (var successor in branch.Successors) {
                stack.Push(successor);
            }
        }
    }

    private void RestructureSingle() {
        var regions = ConstructBranchRegions(branch);
        var exit = ConstructSingleExit(regions);
        
        BranchRegion.Create(cfg, branch, exit, regions);
    }

    private Block ConstructSingleExit(List<BranchRegion.Region> regions) {
        var dominators = this.dominance[branch];
        var continuations = regions.SelectMany(region => region.SelectMany(b => b.Successors.Except(region))).Distinct().ToList();

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
                foreach (var exit in continuation.Predecessors.Intersect(exits)) {
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

    private List<BranchRegion.Region> ConstructBranchRegions(Block branch) {
        var regions = new List<BranchRegion.Region>();
        
        foreach (var successor in branch.Successors.ToList()) {
            HashSet<Block> blocks;
            Block head = successor;

            var dominators = dominance[successor];
            var preds = successor.Predecessors
                .Where(pred => !dominators.Contains(pred));
            if (preds.Count() > 0) {
                // Empty branch region, construct noop
                var noop = NoopBlock.Create(cfg);
                branch.ReplaceTarget(successor, noop);
                noop.AddTarget(successor);
                blocks = new HashSet<Block>() { noop };
                head = noop;
            }
            else {
                // Discover all blocks that have succesesor as dominator
                blocks = new HashSet<Block>();
                var stack = new Stack<Block>();
                stack.Push(successor);
                while (stack.Count > 0) {
                    var block = stack.Pop();
                    if (blocks.Contains(block))
                        continue;

                    blocks.Add(block);
                    foreach (var succ in block.Successors) {
                        if (dominance[succ].Contains(successor)) {
                            stack.Push(succ);
                        }
                    }
                }
            }
            regions.Add(new BranchRegion.Region(head, blocks));
        }

        return regions;
    }

    private Block? FindBranch(Block head) {
        while (head != null) {
            if (head.Successors.Count > 1 && set.Contains(head)) {
                return head;
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
