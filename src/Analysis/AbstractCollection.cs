using System.Collections;

namespace CSynth.Analysis;

public abstract class AbstractCollection<T> : IEnumerable<T> where T : class {
    private uint _idCounter = 0;
    protected List<T> _items = new();

    public AbstractCollection() { }

    protected uint getId() {
        return _idCounter++;
    }

    public IEnumerator<T> GetEnumerator() => _items.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public T this[uint id] => _items[(int)id];
}
