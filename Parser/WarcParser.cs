namespace Warc;

using System.Reflection.PortableExecutable;
using System.Text;

public class WarcParser
{

    public bool HasRecords { get; private set; } = true;
    Stream input;
    LineReader lineReader;

    public string Filename { get; private set; }

    public WarcParser(string filename)
    {
        Filename = filename;
        input = File.OpenRead(filename);
        lineReader = new LineReader(input);
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
            input.ReadExactly(nextRecord.ContentBytes!, 0, nextRecord.ContentLength.Value);
        }

        //are we at the end of the WARC file?
        if (input.Length == input.Position + 4)
        {
            HasRecords = false;
            input.Close();
        }
        else
        {
            // skip the record's trailing CRLFCRLF
            input.Seek(4, SeekOrigin.Current);
        }

        return nextRecord;
    }
}



