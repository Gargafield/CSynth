namespace CSynth.Analysis;

public enum VariableId : int { }

public class Variable
{
    public VariableId Id { get; set; }

    protected Variable(VariableId id)
    {
        Id = id;
    }

    public static Variable Create(CFG cfg) {
        return cfg.Variables.Add(id => new Variable(id));
    }

    public override string ToString() => Id.ToString();
}

public class VariableCollection : AbstractCollection<Variable>
{
    public VariableCollection() { }

    public Variable this[VariableId id] => _items[(int)id];

    public T Add<T>(Func<VariableId, T> factory)
        where T : Variable
    {
        var node = factory((VariableId)getId());
        _items.Add(node);
        return node;
    }
}