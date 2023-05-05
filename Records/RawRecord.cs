namespace Warc;

using System;

/// <summary>
/// Represents a minimumly parsed record. This is only used internally by the parser.
///
/// - What type of record?
/// - What's the version?
/// - What's the content bytes and length (if any)
/// - List of headerlines 
/// </summary>
internal class RawRecord
{
    const string HeaderContentLength = "content-length:";
    const string HeaderType = "warc-type:";
    const string HeaderVersion = "warc/";

    public string? Version { get; private set; }

    public string? Type { get; private set; }

    public int? ContentLength { get; private set; }

    public byte[]? ContentBytes = null;

    public List<string> headers = new List<string>(16);

    public void AddHeaderLine(string headerLine)
    {
        if(Version == null)
        {
            if (string.Compare(headerLine, 0, HeaderVersion, 0, HeaderVersion.Length, true) == 0)
            {
                Version = headerLine.Substring(HeaderVersion.Length);
                return;
            }
        }

        if (Type == null)
        {
            if (string.Compare(headerLine, 0, HeaderType, 0, HeaderType.Length, true) == 0)
            {
                Type = headerLine.Substring(HeaderType.Length).TrimStart();
                return;
            }
        }

        if (ContentLength == null)
        {
            if (string.Compare(headerLine, 0, HeaderContentLength, 0, HeaderContentLength.Length, true) == 0)
            {
                ContentLength = Convert.ToInt32(headerLine.Substring(HeaderContentLength.Length));
                //known that we know the length, we can allocate exactly that size
                ContentBytes = new byte[ContentLength.Value];
                return;
            }
        }
        headers.Add(headerLine);
    }
}

