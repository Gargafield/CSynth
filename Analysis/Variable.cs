namespace CSynth.Analysis;

public abstract class Variable : Expression
{ }

public class LocalVariable : Variable
{
    public int Offset { get; set; }

    public override string ToString() => $"local_{Offset}";
}
