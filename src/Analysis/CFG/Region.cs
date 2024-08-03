using System.Collections;

namespace CSynth.Analysis;

public enum RegionId : uint { }

public abstract class Region {
    public RegionId Id { get; set; }
    public HashSet<Block> Blocks { get; set; } = new();

    protected Region(RegionId id) {
        Id = id;
    }
}

public class RegionCollection : AbstractCollection<Region> {
    public RegionCollection() { }

    public Region this[RegionId id] => _items[(int)id];

    public T Add<T>(Func<RegionId, T> factory)
        where T : Region
    {
        var region = factory((RegionId)getId());
        _items.Add(region);
        return region;
    }
}

public class LoopRegion : Region {
    public Block Header { get; set; }
    public Block Control { get; set; }

    private LoopRegion(RegionId id, HashSet<Block> blocks, Block header, Block control) : base(id) {
        Blocks = blocks;
        Header = header;
        Control = control;
    }

    public static LoopRegion Create(CFG cfg, HashSet<Block> blocks, Block header, Block control) {
        var region = cfg.Regions.Add(id => new LoopRegion(id, blocks, header, control));
        return region;
    }

    public void RemoveBackedge() {
        Control.RemoveTarget(Header);
    }

    public void AddBackedge() {
        Control.AddTarget(Header);
    }
}

public class BranchRegion : Region {
    public Block Header { get; set; }
    public Block Exit { get; set; }
    public List<Region> Regions { get; set; } = new();

    public class Region : IEnumerable<Block> {
        public Block Header;
        public HashSet<Block> Blocks;

        public Region(Block header, HashSet<Block> blocks) {
            Header = header;
            Blocks = blocks;
        }

        public IEnumerator<Block> GetEnumerator() => Blocks.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    private BranchRegion(RegionId id, HashSet<Block> blocks, Block header, Block exit, List<Region> regions) : base(id) {
        Blocks = blocks;
        Header = header;
        Exit = exit;
        Regions = regions;
    }

    public static BranchRegion Create(CFG cfg, Block header, Block exit, List<Region> regions) {
        var blocks = regions.SelectMany(region => region).ToHashSet();
        blocks.Add(header);
        blocks.Add(exit);
        
        var region = cfg.Regions.Add(id => new BranchRegion(id, blocks, header, exit, regions));
        return region;
    }

    public void RemoveExit() {
        Header.RemoveTarget(Exit);
    }
}