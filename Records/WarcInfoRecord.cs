using System.Text;

namespace WarcDotNet;

public class WarcInfoRecord : WarcRecord
{
    /// <summary>
    /// Helper property, lets use set/get UTF-8 string for the ContentBlock for this record.
    /// If you perfer, you can just set the ContentBlock directly
    /// </summary>
    public string? ContentText
    {
        get
        {
            return ContentLength > 0 ?
                Encoding.UTF8.GetString(ContentBlock!) :
                null;
        }

        set
        {
            if (value == null)
            {
                ContentBlock = null;
            }
            else
            {
                ContentBlock = Encoding.UTF8.GetBytes(value);
            }
        }
    }

    private string? contentType;
    /// <summary>
    /// Optional field. Maps to the "Content-Type" WARC field.
    /// Only makes sense with a non-empty Content Block
    /// </summary>
    public string? ContentType
    {
        get => contentType;
        set
        {
            contentType = ValidateFieldValue(value);
        }
    }

    private string? filename;
    /// <summary>
    /// Optional field. Maps to the "WARC-Filename" WARC field.
    /// The filename containing this warcinfo record.
    /// </summary>
    public string? Filename
    {
        get => filename;
        set
        {
            filename = ValidateFieldValue(value);
        }
    }

    public override string Type => RecordType.WarcInfo;

    public WarcInfoRecord() { }

    internal WarcInfoRecord(RawRecord rawRecord)
        : base(rawRecord)
    { }

    protected override bool ParseRecordField(string name, string value)
    {
        switch (name)
        {
            case NormalizedWarcFields.ContentType:
                contentType = value;
                return true;

            case NormalizedWarcFields.Filename:
                filename = value;
                return true;
        }
        return false;
    }

    protected override void AppendRecordFields(StringBuilder builder)
    {
        AppendFieldIfExists(builder, WarcFields.Filename, Filename);
        AppendFieldIfExists(builder, WarcFields.ContentType, ContentType);
    }
}