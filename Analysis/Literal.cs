namespace CSynth.Analysis;

public abstract class Literal : Expression { }

public class IntLiteral : Literal
{
    public int Value { get; set; }

    public override string ToString()
    {
        return Value.ToString();
    }
}

public class StringLiteral : Literal
{
    public string Value { get; set; } = string.Empty;

    public override string ToString()
    {
        return Value;
    }
}

public class BoolLiteral : Literal
{
    public bool Value { get; set; }

    public override string ToString()
    {
        return Value.ToString();
    }
}

public class NullLiteral : Literal
{
    public override string ToString()
    {
        return "null";
    }
}
