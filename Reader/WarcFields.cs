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
/// case-insentivie versions of the WARC field names
/// </summary>
internal static class NormalizedWarcFields
{
    public const string BlockDigest = "warc-block-digest";
    public const string ConcurrentTo = "warc-concurrent-to";
    public const string ContentType = "content-type";
    public const string Date = "warc-date";
    public const string Filename = "warc-filename";
    public const string IdentifiedPayloadType = "warc-identified-payload-type";
    public const string IpAddress = "warc-ip-address";
    public const string PayloadDigest = "warc-payload-digest";
    public const string RecordId = "warc-record-id";
    public const string RefersTo = "warc-refers-to";
    public const string SegmentNumber = "warc-segment-number";
    public const string TargetUri = "warc-target-uri";
    public const string Truncated = "warc-truncated";
    public const string WarcInfoId = "warc-warcinfo-id";
}