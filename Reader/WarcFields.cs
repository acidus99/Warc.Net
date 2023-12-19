namespace WarcDotNet;

using System;

/// <summary>
/// Constants of properly formatted WARC field names
/// </summary>
public static class WarcFields
{
    public const string BlockDigest = "WARC-Block-Digest";
    public const string ConcurrentTo = "WARC-Concurrent-To";
    public const string ContentType = "Content-Type";
    public const string Date = "WARC-Date";
    public const string Filename = "WARC-Filename";
    public const string IdentifiedPayloadType = "WARC-Identified-Payload-Type";
    public const string IpAddress = "WARC-IP-Address";
    public const string PayloadDigest = "WARC-Payload-Digest";
    public const string RecordId = "WARC-Record-ID";
    public const string RefersTo = "WARC-Refers-To";
    public const string SegmentNumber = "WARC-Segment-Number";
    public const string TargetUri = "WARC-Target-URI";
    public const string Truncated = "WARC-Truncated";
    public const string WarcInfoId = "WARC-Warcinfo-ID";
}

/// <summary>
/// case-insentivie versions of the WARC field names used for faster parsing / matching
/// </summary>
internal static class NormalizedWarcFields
{
    public static readonly string BlockDigest = WarcFields.BlockDigest.ToLower();
    public static readonly string ConcurrentTo = WarcFields.ConcurrentTo.ToLower();
    public static readonly string ContentType = WarcFields.ContentType.ToLower();
    public static readonly string Date = WarcFields.Date.ToLower();
    public static readonly string Filename = WarcFields.Filename.ToLower();
    public static readonly string IdentifiedPayloadType = WarcFields.IdentifiedPayloadType.ToLower();
    public static readonly string IpAddress = WarcFields.IpAddress.ToLower();
    public static readonly string PayloadDigest = WarcFields.PayloadDigest.ToLower();
    public static readonly string RecordId = WarcFields.RecordId.ToLower();
    public static readonly string RefersTo = WarcFields.RefersTo.ToLower();
    public static readonly string SegmentNumber = WarcFields.SegmentNumber.ToLower();
    public static readonly string TargetUri = WarcFields.TargetUri.ToLower();
    public static readonly string Truncated = WarcFields.Truncated.ToLower();
    public static readonly string WarcInfoId = WarcFields.WarcInfoId.ToLower();
}