﻿using System.Text;

namespace WarcDotNet;

public class UnknownRecord : WarcRecord
{

    public override string Type => RecordType.Unknown;

    public UnknownRecord() { }

    internal UnknownRecord(RawRecord rawRecord)
        : base(rawRecord)
    { }

    protected override void AppendRecordFields(StringBuilder builder)
    {
    }

    protected override bool ParseRecordField(string name, string value)
    {
        return false;
    }
}