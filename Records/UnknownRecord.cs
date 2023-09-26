namespace Warc;

using System;
using System.Net.Mime;
using System.Text;


public class UnknownRecord : WarcRecord
{

    public override string Type => RecordType.Unknown;

    public UnknownRecord() { }

    internal UnknownRecord(RawRecord rawRecord)
        : base(rawRecord)
    { }

    protected override void AppendRecordHeaders(StringBuilder builder)
    {
    }

    protected override bool ParseRecordHeader(string name, string value)
    {
        return false;
    }
}