using System.Collections;

namespace WarcDotNet;

public class WarcRecordEnumerator : IEnumerator<WarcRecord>
{
    WarcReader _reader;

    public WarcRecordEnumerator(WarcReader reader)
    {
        _reader = reader;
        Current = null!;
    }

    public WarcRecord Current { get; private set; }

    object IEnumerator.Current => Current;

    public void Dispose()
    { }

    public bool MoveNext()
    {
        Current = _reader.GetNextRecord()!;

        return (Current != null);
    }

    public void Reset()
    {
        throw new NotImplementedException();
    }
}