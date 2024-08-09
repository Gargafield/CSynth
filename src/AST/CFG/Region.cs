using System.Collections;

namespace CSynth.AST;

public enum RegionId : uint { }

public abstract class Region {
    public ICollection<int> Blocks { get; set; }
    public int Id { get; set; }

    public Region(ICollection<int> blocks) {
        Blocks = blocks;
    }
}

public class RegionCollection : List<Region>, IEnumerable<int> {
    public BlockCollection Blocks { get; set; }

    public RegionCollection(BlockCollection blocks) {
        Blocks = blocks;
    }

    public int AddBranchRegion(int branch, int exit) {
        return BranchRegion.Create(this, Blocks[branch], Blocks[exit]).Id;
    }
    public void AddRegionToBranch(int region, ICollection<int> nodes, int header, int condition) {
        var branch = (BranchRegion)this[region];
        branch.Regions.Add(new BranchRegion.Region(Blocks[header], nodes, condition));
    }
    public int AddLoopRegion(ICollection<int> blocks, int header, int control) {
        return LoopRegion.Create(this, blocks, Blocks[header], Blocks[control]).Id;
    }

    public void RemoveBackedge(int loop) {
        var region = (LoopRegion)this[loop];
        Blocks.RemoveEdge(region.Control.Id, region.Header.Id);
    }

    public void AddBackedge(int loop) {
        var region = (LoopRegion)this[loop];
        Blocks.AddEdge(region.Control.Id, region.Header.Id);
    }

    public T Add<T>(T region)
    where T: Region
    {
        region.Id = Count;
        base.Add(region);
        return region;
    }

    IEnumerator<int> IEnumerable<int>.GetEnumerator() => Enumerable.Range(0, Count).GetEnumerator();
}

public class LoopRegion : Region {
    public Block Header { get; set; }
    public Block Control { get; set; }

    private LoopRegion(ICollection<int> blocks, Block header, Block control) : base(blocks) {
        Header = header;
        Control = control;
        Blocks = blocks;
    }

    public static LoopRegion Create(RegionCollection regions, ICollection<int> blocks, Block header, Block control) {
        return regions.Add(new LoopRegion(blocks, header, control));
    }
}

public class BranchRegion : Region {
    public Block Header { get; set; }
    public Block Exit { get; set; }
    public List<Region> Regions { get; set; } = new();

    public class Region : IEnumerable<int> {
        public Block Header;
        public ICollection<int> Blocks;
        public int Condition { get; set; }

        public Region(Block header, ICollection<int> blocks, int condition) {
            Header = header;
            Blocks = blocks;
            Condition = condition;
        }

        public IEnumerator<int> GetEnumerator() => Blocks.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    private BranchRegion(ICollection<int> blocks, Block header, Block exit) : base(blocks) {
        Header = header;
        Exit = exit;
        Regions = new List<Region>();
        Blocks = blocks;
    }

    public static BranchRegion Create(RegionCollection regions, Block header, Block exit) {
        var blocks = new List<int>() { header.Id, exit.Id };

        var region = regions.Add(new BranchRegion(blocks, header, exit));
        return region;
    }

    public void AddRegion(Region region) {
        Regions.Add(region);
        foreach (var block in region) {
            Blocks.Add(block);
        }
    }
}