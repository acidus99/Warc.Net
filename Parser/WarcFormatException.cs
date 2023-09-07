namespace Warc;

	public class WarcFormatException : FormatException
	{
    public long RecordOffset { get; private set; }

    public WarcFormatException(long recordOffset, string? message)
        : base(message)
    {
        RecordOffset = recordOffset;
    }

    public override string ToString()
        => $"Warc Format Exception. Offset {RecordOffset}. {Message}";
}

