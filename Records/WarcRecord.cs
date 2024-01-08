namespace WarcDotNet;

using System;
using System.Collections.Generic;
using System.Text;

/// <summary>
/// Common abstract base class for all WARC Records
/// </summary>
public abstract class WarcRecord
{

    private string? blockDigest;
    /// <summary>
    /// Optional. Maps to the "WARC-Block-Digest" field.
    /// A digest of the Records' content block
    /// </summary>
    public string? BlockDigest
    {
        get => blockDigest;
        set
        {
            blockDigest = ValidateFieldValue(value);
        }
    }

    /// <summary>
    /// Optional. Maps to the body of the record
    /// The bytes that make up the recode. null mean an empty record
    /// </summary>
    public byte[]? ContentBlock { get; set; }

    /// <summary>
    /// Required. Maps to "Content-Length" field.
    /// Represents size of record content block
    /// </summary>
    public int ContentLength
        => ContentBlock?.Length ?? 0;

    /// <summary>
    /// Any custom WARC fields
    /// </summary>
    public FieldCollection CustomFields = new FieldCollection();

    /// <summary>
    /// Required. Maps to the "WARC-Date" field.
    /// The date/time associated with the record
    /// </summary>
    public DateTime Date { get; set; } = DateTime.Now;

    /// <summary>
    /// Required. Maps to the "WARC-Record-ID" field
    /// Unique identifier for this record.
    /// </summary>
    public Uri Id { get; set; } = CreateId();

    /// <summary>
    /// Optional. Maps to the "WARC-Segment-Number" field.
    /// This record’s relative ordering in a sequence of segmented records.
    /// </summary>
    public int? Segment { get; set; }

    private string? truncated;
    /// <summary>
    /// Optional. Maps to the "WARC-Truncated" field.
    /// A reason why the full contents of something wasn't stored in a record
    /// </summary>
    public string? Truncated
    {
        get => truncated;
        set
        {
            truncated = ValidateFieldValue(value);
        }
    }

    /// <summary>
    /// Required. Maps to the "WARC-Type" field.
    /// The type of record this is
    /// </summary>
    public abstract string Type { get; }

    /// <summary>
    /// Required. Maps to the initial "WARC/" Version.
    /// The version of the WARC format this record is using
    /// </summary>
    public string Version { get; set; } = "1.1";

    public WarcRecord()
    { }

    internal WarcRecord(RawRecord rawRecord)
    {
        ContentBlock = rawRecord.ContentBytes;
        //version is validated as not null earlier in the parser
        Version = rawRecord.Version!;

        ParseFields(rawRecord);
    }

    /// <summary>
    /// Parses the fields collected by RawRecord
    /// </summary>
    /// <param name="rawRecord"></param>
    /// <exception cref="WarcFormatException"></exception>
    private void ParseFields(RawRecord rawRecord)
    {
        int fieldNumber = 0;
        foreach (var fieldLine in rawRecord.FieldsLines)
        {
            fieldNumber++;

            int index = fieldLine.IndexOf(':');
            if (index > 0 && index + 1 < fieldLine.Length)
            {
                var name = fieldLine.Substring(0, index).ToLower();
                var value = fieldLine.Substring(index + 1).Trim();

                //first see if it's a common field
                if (ParseCommonField(name, value))
                {
                    continue;
                }
                //check for record-specific field
                if (ParseRecordField(name, value))
                {
                    continue;
                }
                //unknonwn field, so added to list of custom field
                //duplicates are supported
                CustomFields.Add(name, value);
            }
            else
            {
                throw new WarcFormatException($"Malformed WARC field. Missing ':' delimiter in line {fieldNumber}.", rawRecord);
            }
        }
    }

    /// <summary>
    /// Sets the Date if the supplied value is non-null.Otherwise current value persists
    /// </summary>
    /// <param name="dateTime"></param>
    public void SetDate(DateTime? dateTime)
    {
        if(dateTime != null)
        {
            Date = dateTime.Value;
        }
    }

    /// <summary>
    /// Parses the few fields that are common to all record types
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    private bool ParseCommonField(string name, string value)
    {
        switch (name)
        {
            case NormalizedWarcFields.BlockDigest:
                BlockDigest = value;
                return true;

            case NormalizedWarcFields.Date:
                Date = DateTime.Parse(value);
                return true;

            case NormalizedWarcFields.RecordId:
                Id = ParseUri(value);
                return true;

            case NormalizedWarcFields.SegmentNumber:
                Segment = Convert.ToInt32(value);
                return true;

            case NormalizedWarcFields.Truncated:
                Truncated = value;
                return true;
        }
        return false;
    }

    /// <summary>
    /// Checks if a string contains allowed characters for a field value.
    /// </summary>
    /// <param name="fieldValue">field value to use</param>
    /// <returns>field value if it is allowed</returns>
    /// <exception cref="FormatException">if fieldValue conttains illegal characters </exception>
    protected string? ValidateFieldValue(string? fieldValue)
    {
        if (fieldValue != null)
        {
            if (!FieldValidator.IsAllowedValue(fieldValue))
            {
                throw new FormatException($"Field value '{fieldValue}' contains illegal character");
            }
        }
        return fieldValue;
    }

    /// <summary>
    /// Calls the record-specific field parsing logic. Strings will already have been checked for illegal characters
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    protected abstract bool ParseRecordField(string name, string value);

    /// <summary>
    /// Calls the record-specific code to add record-specific field to the record field
    /// being built.
    /// </summary>
    /// <param name="builder"></param>
    protected abstract void AppendRecordFields(StringBuilder builder);

    public static Uri CreateId()
    {
        var uri = new Uri($"urn:uuid:{Guid.NewGuid()}");
        return uri;
    }

    protected Uri ParseUri(string uri)
    {
        if(uri.Length < 3)
        {
            throw new ArgumentException("Invalid URI: provided URI too short", nameof(uri));
        }

        //strip < > around URI per WARC spec, if they exist
        if (uri[0] == '<' && uri[uri.Length-1] == '>')
        {
            uri = uri.Substring(1, uri.Length - 2);
        }
        return new Uri(uri);
    }

    /// <summary>
    /// Get the header for the WARC Record
    /// </summary>
    /// <returns></returns>
    public string GetHeader()
    {
        var sb = new StringBuilder();
        // required fields first
        sb.Append($"WARC/{Version}\r\n");
        sb.Append(FormatField("WARC-Type", Type));
        sb.Append(FormatField(WarcFields.Date, FormatDate(Date)));
        sb.Append(FormatField(WarcFields.RecordId, FormatUrl(Id)));
        sb.Append(FormatField("Content-Length", ContentLength.ToString()));

        //add common, optional fields next
        AppendFieldIfExists(sb, WarcFields.BlockDigest, BlockDigest);
        AppendFieldIfExists(sb, WarcFields.SegmentNumber, Segment);
        AppendFieldIfExists(sb, WarcFields.Truncated, Truncated);

        //add record-specific fields
        AppendRecordFields(sb);

        //add custom fields
        foreach(string fieldName in CustomFields.Fields)
        {
            foreach (string value in CustomFields[fieldName])
            {
                sb.Append(FormatField(fieldName, value));
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Helper, appends a field if a URL exists
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="name"></param>
    /// <param name="url"></param>
    protected void AppendFieldIfExists(StringBuilder builder, string name, Uri? url)
        => AppendFieldIfExists(builder, name, FormatOptionalUrl(url));

    /// <summary>
    /// Helper, appends a field with a number value, if it exists
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="name"></param>
    /// <param name="num"></param>
    protected void AppendFieldIfExists(StringBuilder builder, string name, int? num)
        => AppendFieldIfExists(builder, name, num?.ToString());

    /// <summary>
    /// Helpoer, appends a field only if a value exists
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="name"></param>
    /// <param name="value"></param>
    protected void AppendFieldIfExists(StringBuilder builder, string name, string? value)
    {
        if (value != null)
        {
            builder.Append(FormatField(name, value));
        }
    }

    protected string FormatField(string name, string value)
        => $"{name}: {value}\r\n";

    protected string FormatUrl(Uri url)
        => $"<{url.AbsoluteUri}>";

    protected string? FormatOptionalUrl(Uri? url)
        => (url != null) ?
            $"<{url.AbsoluteUri}>" :
            null;

    protected string FormatDate(DateTime date)
        => date.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");
}
