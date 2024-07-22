
namespace CSynth.Analysis.Transformation;

public static class Restructure {
    public static void RestructureCFG(CFG cfg) {
        RestructureLoop.Restructure(cfg);
        foreach (var loop in cfg.Regions.OfType<LoopRegion>())
            loop.RemoveBackedge();
        
        RestructureBranch.Restructure(cfg);
        
        foreach (var loop in cfg.Regions.OfType<LoopRegion>())
            loop.AddBackedge();
    }
}