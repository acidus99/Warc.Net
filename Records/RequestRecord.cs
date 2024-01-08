namespace WarcDotNet;

using System;
using System.Text;

public class RequestRecord : WarcRecord
{
    /// <summary>
    /// Optional field. Maps to the "WARC-Concurrent-To" WARC field.
    ///  The WARC-Record-ID of any records created as part of the same capture event as the current record. Relates this record to one or more records.
    /// </summary>
    public List<Uri> ConcurrentTo = new List<Uri>();

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

    private string? ipAddress;
    /// <summary>
    /// Optional field. Maps to the "WARC-IP-Address" WARC field.
    /// The IP address address contacted to retrieve any included content.
    /// </summary>
    public string? IpAddress
    {
        get => ipAddress;
        set
        {
            ipAddress = ValidateFieldValue(value);
        }
    }

    private string? identifiedPayloadType;
    /// <summary>
    /// Optional. Maps to the "WARC-Identified-Payload-Type" field.
    /// The content type of the "meaningful" payload inside the content block (if any)
    /// </summary>
    public string? IdentifiedPayloadType
    {
        get => identifiedPayloadType;
        set
        {
            identifiedPayloadType = ValidateFieldValue(value);
        }
    }

    private string? payloadDigest;
    /// <summary>
    /// Optional. Maps to the "WARC-Payload-Digest" field.
    /// A digest of the "meaningful" payload inside the content block (if any)
    /// </summary>
    public string? PayloadDigest
    {
        get => payloadDigest;
        set
        {
            payloadDigest = ValidateFieldValue(value);
        }
    }

    /// <summary>
    /// Optional field. Maps to the "WARC-Target-URI" WARC field.
    /// The original URI whose capture gave rise to the information content in this record
    /// </summary>
    public Uri? TargetUri { get; set; }

    public override string Type => RecordType.Request;

    /// <summary>
    /// Optional field. Maps to the WARC-Warcinfo-ID" WARC field.
    /// When present, the WARC-Warcinfo-ID indicates the WARC-Record-ID of the associated ‘warcinfo’ record for this record.
    /// </summary>
    public Uri? WarcInfoId { get; set; }

    public RequestRecord() { }

    internal RequestRecord(RawRecord rawRecord)
        : base(rawRecord)
    { }

    protected override void AppendRecordFields(StringBuilder builder)
    {
        foreach(Uri uri in ConcurrentTo)
        {
            builder.Append(FormatField(WarcFields.ConcurrentTo, FormatUrl(uri)));
        }
        AppendFieldIfExists(builder, WarcFields.ContentType, ContentType);
        AppendFieldIfExists(builder, WarcFields.IpAddress, IpAddress);

        if(TargetUri != null)
        {
            builder.Append(FormatField(WarcFields.TargetUri, TargetUri.AbsoluteUri));
        }

        AppendFieldIfExists(builder, WarcFields.WarcInfoId, WarcInfoId);
        AppendFieldIfExists(builder, WarcFields.IdentifiedPayloadType, IdentifiedPayloadType);
        AppendFieldIfExists(builder, WarcFields.PayloadDigest, PayloadDigest);

    }

    protected override bool ParseRecordField(string name, string value)
    {
        switch (name)
        {
            case NormalizedWarcFields.ContentType:
                contentType = value;
                return true;

            case NormalizedWarcFields.ConcurrentTo:
                ConcurrentTo.Add(ParseUri(value));
                return true;

            case NormalizedWarcFields.IpAddress:
                ipAddress = value;
                return true;

            case NormalizedWarcFields.IdentifiedPayloadType:
                identifiedPayloadType = value;
                return true;

            case NormalizedWarcFields.PayloadDigest:
                payloadDigest = value;
                return true;

            case NormalizedWarcFields.TargetUri:
                TargetUri = ParseUri(value);
                return true;

            case NormalizedWarcFields.WarcInfoId:
                WarcInfoId = ParseUri(value);
                return true;
        }
        return false;
    }
}