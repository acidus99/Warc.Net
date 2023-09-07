namespace Warc;

using System.Reflection.PortableExecutable;
using System.Text;

public class WarcParser
{

    byte[] endOfRecordBuffer = new byte[4];

    public bool HasRecords { get; private set; } = true;
    Stream inputStream;
    LineReader lineReader;

    public string Filename { get; private set; }

    public WarcParser(string filename)
    {
        Filename = filename;
        inputStream = File.OpenRead(filename);
        lineReader = new LineReader(inputStream);
    }

    public WarcRecord? GetNext()
    {
        if (!HasRecords)
        {
            return null;
        }
        try
        {
            var rawRecord = GetNextRawRecord();

            //internal record-specific constructors handle additional parsing
            switch (rawRecord.Type)
            {
                case RecordType.Metadata:
                    return new MetadataRecord(rawRecord);

                case RecordType.Request:
                    return new RequestRecord(rawRecord);

                case RecordType.Response:
                    return new ResponseRecord(rawRecord);

                case RecordType.WarcInfo:
                    return new WarcInfoRecord(rawRecord);

                default:
                    return new UnknownRecord(rawRecord);
            }
        }
        catch (WarcFormatException ex)
        {
            Console.WriteLine(ex);
            throw ex;
        }
        
        return null;
    }

    /// <summary>
    /// validates that a raw records contains the minimum required headers
    /// </summary>
    /// <param name="rawRecord"></param>
    /// <exception cref="WarcFormatException"></exception>
    private void EnsureRequiredRawFields(RawRecord rawRecord)
    {
        if(rawRecord.Type == null)
        {
            throw new WarcFormatException(rawRecord.Offset, "Record missing required WARC Type field.");
        }
        if(rawRecord.Version == null)
        {
            throw new WarcFormatException(rawRecord.Offset, "Record missing required WARC Version field.");
        }

        if(rawRecord.ContentLength == null)
        {
            throw new WarcFormatException(rawRecord.Offset, "Record missing required Content-Length field.");
        }
    }

    /// <summary>
    /// reads the stream and returns a minimally parsed record
    /// </summary>
    /// <returns></returns>
    private RawRecord GetNextRawRecord()
    {
        var nextRecord = new RawRecord(inputStream.Position);

        string line = lineReader.GetLine();
        while (line.Length > 0)
        {
            nextRecord.AddHeaderLine(line);
            line = lineReader.GetLine();
        }

        EnsureRequiredRawFields(nextRecord);

        //does the record have a body? if so, read it in
        if (nextRecord.ContentLength! > 0)
        {
            //now read in exactly the size of the content bytes
            inputStream.ReadExactly(nextRecord.ContentBytes!, 0, nextRecord.ContentLength.Value);
        }

        //read and verify the trailing CRLFCRLF
        inputStream.ReadExactly(endOfRecordBuffer, 0, 4);
        if (endOfRecordBuffer[0] != 13 ||
            endOfRecordBuffer[1] != 10 ||
            endOfRecordBuffer[2] != 13 ||
            endOfRecordBuffer[3] != 10)
        {
            throw new WarcFormatException(inputStream.Position, "Could not find CRLFCRLF at end of record. Record's Content-Length field may be incorrect.");
        }

        if(inputStream.Position >= inputStream.Length)
        {
            HasRecords = false;
        }

        return nextRecord;
    }
}
