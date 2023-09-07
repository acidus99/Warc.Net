using System;
using System.Net.Mime;
using System.Text;

namespace Warc
{
	public class ResponseRecord : WarcRecord
	{

        /// <summary>
        /// Optional field. Maps to the "WARC-Concurrent-To" WARC header.
        ///  The WARC-Record-ID of any records created as part of the same capture event as the current record. Relates this record to one or more records.
        /// </summary>
        public List<Uri> ConcurrentTo = new List<Uri>();

        private string? contentType;
        /// <summary>
        /// Optional field. Maps to the "Content-Type" WARC header.
        /// Only makes sense with a non-empty Content Block
        /// </summary>
        public string? ContentType
        {
            get => contentType;
            set
            {
                contentType = ValidateLegalFieldCharacters(value);
            }
        }

        private string? ipAddress;
        /// <summary>
        /// Optional field. Maps to the "WARC-IP-Address" WARC header.
        /// The IP address address contacted to retrieve any included content.
        /// </summary>
        public string? IpAddress
        {
            get => ipAddress;
            set
            {
                ipAddress = ValidateLegalFieldCharacters(value);
            }
        }

        private string? identifiedPayloadType;
        /// <summary>
        /// Optional. Maps to the "WARC-Identified-Payload-Type".
        /// The content type of the "meaningful" payload inside the content block (if any)
        /// </summary>
        public string? IdentifiedPayloadType
        {
            get => identifiedPayloadType;
            set
            {
                identifiedPayloadType = ValidateLegalFieldCharacters(value);
            }
        }

        /// <summary>
        /// Optional. Maps to the "WARC-Payload-Digest" header.
        /// A digest of the "meaningful" payload inside the content block (if any)
        /// </summary>
        public string? PayloadDigest { get; set; }

        /// <summary>
        /// Optional field. Maps to the "WARC-Target-URI" WARC header.
        /// The original URI whose capture gave rise to the information content in this record
        /// </summary>
        public Uri? TargetUri { get; set; }

        public override string Type => RecordType.Response;

        /// <summary>
        /// Optional field. Maps to the WARC-Warcinfo-ID" WARC header.
        /// When present, the WARC-Warcinfo-ID indicates the WARC-Record-ID of the associated ‘warcinfo’ record for this record.
        /// </summary>
        public Uri? WarcInfoId { get; set; }

        public ResponseRecord() { }

        internal ResponseRecord(RawRecord rawRecord)
            : base(rawRecord)
        { }

        protected override void AppendRecordHeaders(StringBuilder builder)
        {
            foreach (Uri uri in ConcurrentTo)
            {
                builder.Append(FormatHeader(WarcHeaders.ConcurrentTo, FormatUrl(uri)));
            }
            AppendHeaderIfExists(builder, WarcHeaders.ContentType, ContentType);
            AppendHeaderIfExists(builder, WarcHeaders.IpAddress, IpAddress);
            if (TargetUri != null)
            {
                builder.Append(FormatHeader(WarcHeaders.TargetUri, TargetUri.AbsoluteUri));
            }
            AppendHeaderIfExists(builder, WarcHeaders.WarcInfoId, WarcInfoId);
            AppendHeaderIfExists(builder, WarcHeaders.IdentifiedPayloadType, IdentifiedPayloadType);
            AppendHeaderIfExists(builder, WarcHeaders.PayloadDigest, PayloadDigest);

        }

        protected override bool ParseRecordHeader(string name, string value)
        {
            switch (name)
            {
                case NormalizedWarcHeaders.ContentType:
                    ContentType = value;
                    return true;

                case NormalizedWarcHeaders.ConcurrentTo:
                    ConcurrentTo.Add(ParseUri(value));
                    return true;

                case NormalizedWarcHeaders.IpAddress:
                    IpAddress = value;
                    return true;

                case NormalizedWarcHeaders.IdentifiedPayloadType:
                    IdentifiedPayloadType = value;
                    return true;

                case NormalizedWarcHeaders.PayloadDigest:
                    PayloadDigest = value;
                    return true;

                case NormalizedWarcHeaders.TargetUri:
                    TargetUri = ParseUri(value);
                    return true;

                case NormalizedWarcHeaders.WarcInfoId:
                    WarcInfoId = ParseUri(value);
                    return true;
            }
            return false;
        }
    }
}