using System;
using System.Linq;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

namespace CSynth.Bench;
public class Program
{
    public static void Main(string[] args)
    {
        if (!args.Contains("--trace")) {
            var config = DefaultConfig.Instance;
            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args, config);
        }
        else {
            var benchMark = new RestructureComplexBench();
            benchMark.Setup();
            benchMark.RestructureComplex();
        }

    }
}
