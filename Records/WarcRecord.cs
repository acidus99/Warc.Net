﻿using System;
using System.Collections.Specialized;
using System.Reflection;
using System.Text;

namespace Warc
{
    /// <summary>
    /// Common abstract base class for all WARC Records
    /// </summary>
	public abstract class WarcRecord
	{
        /// <summary>
        /// Optional. Maps to the "WARC-Block-Digest" header.
        /// A digest of the Records' content block
        /// </summary>
        public string? BlockDigest { get; set; }

        /// <summary>
        /// Optional. Maps to the body of the record
        /// The bytes that make up the recode. null mean an empty record
        /// </summary>
        public byte[]? ContentBlock { get; set; }

        /// <summary>
        /// Required. Maps to "Content-Length" header.
        /// Represents size of record content block
        /// </summary>
        public int ContentLength
            => ContentBlock?.Length ?? 0;

        /// <summary>
        /// Any custom WARC headers
        /// </summary>
        public IDictionary<string, string> CustomHeaders = new Dictionary<string, string>();
 
        /// <summary>
        /// Required. Maps to the "WARC-Date" header.
        /// The date/time associated with the record
        /// </summary>
        public DateTime Date { get; set; } = DateTime.Now;

        /// <summary>
        /// Required. Maps to the "WARC-Record-ID" header
        /// Unique identifier for this record.
        /// </summary>
        public Uri Id { get; set; } = CreateId();

        /// <summary>
        /// Optional. Maps to the "WARC-Identified-Payload-Type".
        /// The content type of the "meaningful" payload inside the content block (if any)
        /// </summary>
        public string? IdentifiedPayloadType { get; set; }

        /// <summary>
        /// Optional. Maps to the "WARC-Payload-Digest" header.
        /// A digest of the "meaningful" payload inside the content block (if any)
        /// </summary>
        public string? PayloadDigest { get; set; }

        /// <summary>
        /// Optional. Maps to the "WARC-Segment-Number" header.
        /// This record’s relative ordering in a sequence of segmented records.
        /// </summary>
        public int? Segment { get; set; }

        /// <summary>
        /// Optional. Maps to the "WARC-Truncated" header.
        /// A reason why the full contents of something wasn't stored in a record
        /// </summary>
        public string? Truncated { get; set; }

        /// <summary>
        /// Required. Maps to the "WARC-Type" header.
        /// The type of record this is
        /// </summary>
        public abstract string Type { get; }

        /// <summary>
        /// Required. Maps to the initial "WARC/" header.
        /// The version of the WARC format this record is using
        /// </summary>
        public string Version { get; set; } = "1.1";

        public WarcRecord()
        { }

        internal WarcRecord(RawRecord rawRecord)
        {
            ContentBlock = rawRecord.ContentBytes;
            Version = rawRecord.Version!;

            ParseHeaders(rawRecord.headers);
        }

        /// <summary>
        /// Parses the headers collectd by RawRecord
        /// </summary>
        /// <param name="headers"></param>
        private void ParseHeaders(List<string> headers)
        {
            foreach (var headerLine in headers)
            {
                int index = headerLine.IndexOf(':');
                if (index > 0 && index + 1 < headerLine.Length)
                {
                    var name = headerLine.Substring(0, index).ToLower();
                    var value = headerLine.Substring(index+1).Trim();

                    //first see if its a common header
                    if(ParseCommonHeader(name, value))
                    {
                        continue;
                    }
                    //check for record-specific headers
                    if(ParseRecordHeader(name, value))
                    {
                        continue;
                    }
                    //unknonwn header, so added to list of custom headers
                    //if duplicates, last value wins
                    CustomHeaders[name] = value;
                }
            }
        }

        /// <summary>
        /// Parses the few headers that are common to all record types
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        private bool ParseCommonHeader(string name, string value)
        {
            switch (name)
            {
                case NormalizedWarcHeaders.BlockDigest:
                    BlockDigest = value;
                    return true;

                case NormalizedWarcHeaders.Date:
                    Date = DateTime.Parse(value);
                    return true;

                case NormalizedWarcHeaders.IdentifiedPayloadType:
                    IdentifiedPayloadType = value;
                    return true;

                case NormalizedWarcHeaders.PayloadDigest:
                    PayloadDigest = value;
                    return true;

                case NormalizedWarcHeaders.RecordId:
                    Id = ParseUri(value);
                    return true;

                case NormalizedWarcHeaders.SegmentNumber:
                    Segment = Convert.ToInt32(value);
                    return true;

                case NormalizedWarcHeaders.Truncated:
                    Truncated = value;
                    return true;
            }
            return false;
        }


        /// <summary>
        /// Calls the record-specific header parsing logic
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        protected abstract bool ParseRecordHeader(string name, string value);

        protected abstract void AppendRecordHeaders(StringBuilder builder);

        public static Uri CreateId()
        {
            var uri = new Uri($"urn:uuid:{Guid.NewGuid()}");
            return uri;
        }

        protected Uri ParseUri(string uri)
        {
            if (uri[0] == '<')
            {
                uri = uri.Substring(1, uri.Length - 2);
            }
            return new Uri(uri);
        }

        public string GetHeaders()
        {
            var sb = new StringBuilder();
            // required headers first
            sb.Append($"WARC/{Version}\r\n");
            sb.Append(FormatHeader("WARC-Type", Type));
            sb.Append(FormatHeader(WarcHeaders.Date, FormatDate(Date)));
            sb.Append(FormatHeader(WarcHeaders.RecordId, FormatUrl(Id)));
            sb.Append(FormatHeader("Content-Length", ContentLength.ToString()));

            //add common, optional headers next
            AppendHeaderIfExists(sb, WarcHeaders.BlockDigest, BlockDigest);
            AppendHeaderIfExists(sb, WarcHeaders.IdentifiedPayloadType, IdentifiedPayloadType);
            AppendHeaderIfExists(sb, WarcHeaders.PayloadDigest, PayloadDigest);
            AppendHeaderIfExists(sb, WarcHeaders.SegmentNumber, Segment);
            AppendHeaderIfExists(sb, WarcHeaders.Truncated, Truncated);

            //add record-specific headers
            AppendRecordHeaders(sb);

            //add custom headers
            foreach(var nvp in CustomHeaders)
            {
                sb.Append(FormatHeader(nvp.Key, nvp.Value));
            }

            return sb.ToString();
        }
        protected void AppendHeaderIfExists(StringBuilder builder, string name, Uri? url)
            => AppendHeaderIfExists(builder, name, FormatOptionalUrl(url));

        protected void AppendHeaderIfExists(StringBuilder builder, string name, int? num)
            => AppendHeaderIfExists(builder, name, num?.ToString());

        protected void AppendHeaderIfExists(StringBuilder builder, string name, string? value)
        {
            if (value != null)
            {
                builder.Append(FormatHeader(name, value));
            }
        }

        protected string FormatHeader(string name, string value)
            => $"{name}: {value}\r\n";

        protected string FormatUrl(Uri url)
            => $"<{url.AbsoluteUri}>";

        protected string? FormatOptionalUrl(Uri? url)
            => (url != null) ?
                "<{url.AbsoluteUri}>" :
                null;

        protected string FormatDate(DateTime date)
            => date.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ");
    }
}
