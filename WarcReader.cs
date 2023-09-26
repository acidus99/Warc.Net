namespace Warc;

using System.Collections;
using System.IO.Compression;

public class WarcReader : IDisposable
{
    byte[] endOfRecordBuffer = new byte[4];

    public bool HasRecords { get; private set; } = true;

    Stream fileStream;
    Stream inputStream;

    LineReader lineReader;
    bool isCompressed = false;
    private bool isDisposed;

    public string Filename { get; private set; }

    public WarcReader(string filename)
    {
        Filename = filename;
        var info = new FileInfo(filename);
        // Checks whether per-record compression based on *.gz file extension is used
        if (info.Extension == ".gz")
        {
            isCompressed = true;
        }

        fileStream = File.OpenRead(filename);

        inputStream = isCompressed ?
            new GZipStream(fileStream, CompressionMode.Decompress) :
            fileStream;

        lineReader = new LineReader();

        List<string> foo = new List<string>();
    }

    public WarcRecord GetNext()
    {
        if (!HasRecords)
        {
            throw new IndexOutOfRangeException("No more records are available");
        }
        var rawRecord = GetNextRawRecord();
        return ParseRawRecord(rawRecord);
    }

    private WarcRecord ParseRawRecord(RawRecord rawRecord)
    {
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

    /// <summary>
    /// validates that a raw records contains the minimum required headers
    /// </summary>
    /// <param name="rawRecord"></param>
    /// <exception cref="WarcFormatException"></exception>
    private void EnsureRequiredRawFields(RawRecord rawRecord)
    {
        if (rawRecord.Type == null)
        {
            throw new WarcFormatException(rawRecord.Offset, "Record missing required WARC Type field.");
        }
        if (rawRecord.Version == null)
        {
            throw new WarcFormatException(rawRecord.Offset, "Record missing required WARC Version field.");
        }

        if (rawRecord.ContentLength == null)
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
        var nextRecord = new RawRecord(fileStream.Position);

        lineReader.SetRecordStart(fileStream.Position);

        string line = lineReader.GetLine(inputStream);
        while (line.Length > 0)
        {
            nextRecord.AddHeaderLine(line);
            line = lineReader.GetLine(inputStream);
        }

        EnsureRequiredRawFields(nextRecord);

        try
        {
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

        }
        catch (EndOfStreamException)
        {
            throw new WarcFormatException(inputStream.Position, "File ends with incomplete record. Record's Content-Length field may be incorrect, or record is prematurely truncated.");
        }

        if (fileStream.Position >= fileStream.Length)
        {
            HasRecords = false;
        }

        return nextRecord;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!isDisposed)
        {
            if (disposing)
            {
                inputStream.Close();
                inputStream.Dispose();

                fileStream.Close();
                fileStream.Dispose();
            }
            isDisposed = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
