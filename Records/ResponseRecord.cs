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

        /// <summary>
        /// Optional field. Maps to the "Content-Type" WARC header.
        /// Only makes sense with a non-empty Content Block
        /// </summary>
        public string? ContentType { get; set; }


        /// <summary>
        /// Optional field. Maps to the "WARC-IP-Address" WARC header.
        /// The IP address address contacted to retrieve any included content.
        /// </summary>
        public string? IpAddress { get; set; }

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
            AppendHeaderIfExists(builder, WarcHeaders.TargetUri, TargetUri);
            AppendHeaderIfExists(builder, WarcHeaders.WarcInfoId, WarcInfoId);
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