namespace WarcDotNet;

/// <summary>
/// All known WARC record types
/// </summary>
public static class RecordType
{
	public const string Continuation = "continuation";
    public const string Conversion = "conversion";
    public const string Metadata = "metadata";
    public const string Request = "request";
    public const string Resource = "resource";
    public const string Response = "response";
    public const string Revisit = "revisit";
    public const string WarcInfo = "warcinfo";
    public const string Unknown = "unknown";
}