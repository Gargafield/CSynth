using System.Linq;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

namespace CSynth.Bench;
public class Program
{
    public static void Main(string[] args)
    {
        var config = DefaultConfig.Instance;
        // var summary = BenchmarkRunner.Run<RestrucureBench>(config, args);

        // Use this to select benchmarks from the console:
        if (!args.Contains("--trace"))
            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, config);
        else {
            var benchMark = new RestructureBench();
            benchMark.Setup();
            benchMark.RestructureLinear();
        }

    }
}
