namespace Warc;

using System.Reflection.PortableExecutable;
using System.Text;

public class WarcParser
{

    public bool HasRecords { get; private set; } = true;
    Stream input;
    LineReader lineReader;

    public WarcParser(string filename)
    {
        input = File.OpenRead(filename);
        lineReader = new LineReader(input);
    }

    public WarcRecord? GetNext()
    {
        if (!HasRecords)
        {
            return null;
        }
        var rawRecord = GetNextRawRecord();

        //internal record-specific constructors handle additional parsing
        switch (rawRecord.Type)
        {
            case RecordType.Request:
                return new RequestRecord(rawRecord);

            case RecordType.Response:
                return new ResponseRecord(rawRecord);

            case RecordType.WarcInfo:
                return new WarcInfoRecord(rawRecord);

            //TODO: add more records
        }

        return null;
    }

    /// <summary>
    /// reads the stream and returns a minimally parsed record
    /// </summary>
    /// <returns></returns>
    private RawRecord GetNextRawRecord()
    {
        var nextRecord = new RawRecord();

        string line = lineReader.GetLine();
        while (line.Length > 0)
        {
            nextRecord.AddHeaderLine(line);
            line = lineReader.GetLine();
        }

        if (nextRecord.ContentLength! > 0)
        {
            //now read in exactly the size of the content bytes
            input.ReadExactly(nextRecord.ContentBytes!, 0, nextRecord.ContentLength.Value);
        }
        if (input.Length == input.Position + 4)
        {
            HasRecords = false;
            input.Close();
        }
        else
        {
            // skip the trailing CRLFCRLF
            input.Seek(4, SeekOrigin.Current);
        }

        return nextRecord;
    }
}



