using CSynth.AST;

namespace CSynth.Transformation;

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