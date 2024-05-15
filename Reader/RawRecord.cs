namespace WarcDotNet;

/// <summary>
/// Represents a minimumly parsed record. This is only used internally by the parser.
///
/// - What type of record?
/// - What's the version?
/// - What's the content bytes and length (if any)
/// - List of field lines
/// </summary>
internal class RawRecord
{
    const string ContentLengthField = "content-length:";
    const string TypeField = "warc-type:";
    const string VersionField = "warc/";

    public string? Version { get; private set; }

    public string? Type { get; private set; }

    public int? ContentLength { get; private set; }

    public byte[]? ContentBytes = null;

    public List<string> FieldsLines = new List<string>(16);

    /// <summary>
    /// The byte offset of beginning of this WARC record in the file
    /// </summary>
    public readonly long? Offset;

    public readonly int RecordNumber;

    public bool IsEmpty { get; private set; } = true;

    internal RawRecord(int record, long? offset)
    {
        RecordNumber = record;
        Offset = offset;
    }

    public void AddFieldLine(string? fieldLine)
    {
        if (fieldLine == null)
        {
            return;
        }

        IsEmpty = false;

        if (Version == null)
        {
            if (string.Compare(fieldLine, 0, VersionField, 0, VersionField.Length, true) == 0)
            {
                Version = fieldLine.Substring(VersionField.Length);
                return;
            }
        }

        if (Type == null)
        {
            if (string.Compare(fieldLine, 0, TypeField, 0, TypeField.Length, true) == 0)
            {
                Type = fieldLine.Substring(TypeField.Length).TrimStart();
                return;
            }
        }

        if (ContentLength == null)
        {
            if (string.Compare(fieldLine, 0, ContentLengthField, 0, ContentLengthField.Length, true) == 0)
            {
                ContentLength = Convert.ToInt32(fieldLine.Substring(ContentLengthField.Length));
                ContentBytes = new byte[ContentLength.Value];
                return;
            }
        }
        FieldsLines.Add(fieldLine);
    }
}
