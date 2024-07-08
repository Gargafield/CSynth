using System.Collections;

namespace CSynth.Analysis;

public class CFG : IEnumerable<Block> {
    public BlockCollection Blocks { get; set; } = new();
    public VariableCollection Variables { get; set; } = new();

    public CFG() { }

    public Block this[BlockId index] {
        get => Blocks[index];
    }

    public IEnumerator<Block> GetEnumerator() {
        return Blocks.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }

    public void Print()
    {
        foreach (var block in Blocks) {
            Console.WriteLine($"Block {block.Id}");

            foreach (var target in block.Successors) {
                Console.WriteLine($"  -> {target.Id}");
            }

            if (block is BasicBlock basicBlock) {
                foreach (var instruction in basicBlock.Instructions) {
                    Console.WriteLine(instruction);
                }
            }
            
            Console.WriteLine();
        }
    }

    public void PrintMermaid()
    {
        Console.WriteLine("graph TD");

        foreach (var block in Blocks) {
            foreach (var target in block.Successors) {
                Console.WriteLine($"  {block.Id} --> {target.Id}");
            }
        }
    }
}
