using System;
using System.Text;

namespace Warc
{
	public class WarcInfoRecord : WarcRecord
	{
        /// <summary>
        /// Optional field. Maps to the "Content-Type" WARC header.
        /// Only makes sense with a non-empty Content Block
        /// </summary>
        public string? ContentType { get; set; }

        /// <summary>
        /// Optional field. Maps to the "WARC-Filename" WARC header.
        /// The filename containing this warcinfo record.
        /// </summary>
        public string? Filename { get; set; }

        public override string Type => RecordType.WarcInfo;

        internal WarcInfoRecord(RawRecord rawRecord)
            : base(rawRecord)
        { }		

        protected override bool ParseRecordHeader(string name, string value)
        {
            switch (name)
            {
                case NormalizedWarcHeaders.ContentType:
                    ContentType = value;
                    return true;

                case NormalizedWarcHeaders.Filename:
                    Filename = value;
                    return true;
            }
            return false;
        }

        protected override void AppendRecordHeaders(StringBuilder builder)
        {
            AppendHeaderIfExists(builder, WarcHeaders.Filename, Filename);
            AppendHeaderIfExists(builder, WarcHeaders.ContentType, ContentType);
        }
    }
}

