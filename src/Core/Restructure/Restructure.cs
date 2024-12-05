
namespace CSynth.Core;

public static class Restructure {
    public static void RestructureCFG(CFG cfg) {
        var loopRegions = RestructureLoop.Restructure(cfg);
        foreach (var loop in loopRegions)
            cfg.Regions.RemoveBackedge(loop);
        
        RestructureBranch.Restructure(cfg);
        
        foreach (var loop in loopRegions)
            cfg.Regions.AddBackedge(loop);
    }
}