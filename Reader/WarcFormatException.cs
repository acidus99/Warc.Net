namespace WarcDotNet;

using System.Text;

/// <summary>
/// Hold information about malformed WARCs
/// </summary>
public class WarcFormatException : FormatException
{
    /// <summary>
    /// Which record in the WARC has the problem
    /// </summary>
    public int RecordNumber { get; private set; }

    /// <summary>
    /// What is the offset in the WARC file where this format issue occurs.
    /// For compressed WARC files (e.g. example.warc.gz) this will be null;
    /// </summary>
    public long? RecordOffset { get; private set; }

    public WarcFormatException(string message, int  recordNumber, long? offset)
        : base(message)
    {
        RecordNumber = recordNumber;
        RecordOffset = offset;
    }

    internal WarcFormatException(string message, RawRecord record)
        : this(message, record.RecordNumber, record.Offset)
    {}

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.Append($"Warc Format Exception. Record {RecordNumber}");
        if(RecordOffset.HasValue)
        {
            sb.Append($" Offset: {RecordOffset}");
        }
        sb.Append($": {Message}");
        return sb.ToString();
    }
}