using System.Collections;

namespace CSynth.Analysis;

public enum RegionId : uint { }

public abstract class Region {
    public int Id { get; set; }
    public List<Block> Blocks { get; set; } = new();
}

public class RegionCollection : List<Region> {
    public RegionCollection() { }

    public T Add<T>(T region)
    where T: Region
    {
        region.Id = Count;
        base.Add(region);
        return region;
    }
}

public class LoopRegion : Region {
    public Block Header { get; set; }
    public Block Control { get; set; }

    private LoopRegion(List<Block> blocks, Block header, Block control) {
        Blocks = blocks;
        Header = header;
        Control = control;
    }

    public static LoopRegion Create(CFG cfg, List<Block> blocks, Block header, Block control) {
        return cfg.Regions.Add(new LoopRegion(blocks, header, control));
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
        public List<Block> Blocks;
        public int Condition { get; set; }

        public Region(Block header, List<Block> blocks, int condition) {
            Header = header;
            Blocks = blocks;
            Condition = condition;
        }

        public IEnumerator<Block> GetEnumerator() => Blocks.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    private BranchRegion(List<Block> blocks, Block header, Block exit, List<Region> regions) {
        Blocks = blocks;
        Header = header;
        Exit = exit;
        Regions = regions;
    }

    public static BranchRegion Create(CFG cfg, Block header, Block exit, List<Region> regions) {
        var blocks = regions.SelectMany(region => region).ToList();
        blocks.Add(header);
        blocks.Add(exit);
        
        var region = cfg.Regions.Add(new BranchRegion(blocks, header, exit, regions));
        return region;
    }

    public void RemoveExit() {
        Header.RemoveTarget(Exit);
    }
}