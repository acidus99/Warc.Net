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
        catch (ApplicationException ex)
        {
            Console.WriteLine($"Error read WARC Record: {ex.Message}");
            Console.WriteLine("Skipping...");
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

        //does the record have a body? if so, read it in
        if (nextRecord.ContentLength! > 0)
        {
            //now read in exactly the size of the content bytes
            inputStream.ReadExactly(nextRecord.ContentBytes!, 0, nextRecord.ContentLength.Value);
        }

        //read the trailing CRLFCRLF
        inputStream.ReadExactly(endOfRecordBuffer, 0, 4);
        //verify CRLFCRLF
        if (endOfRecordBuffer[0] != 13 ||
            endOfRecordBuffer[1] != 10 ||
            endOfRecordBuffer[2] != 13 ||
            endOfRecordBuffer[3] != 10)
        {
            int xxx = 5;
            //throw new FormatException("Did not see CRLFCRLF at end of record!");
        }

        //TODO: are we at the end of the file?
        return nextRecord;
    }
}



