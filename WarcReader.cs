namespace Warc;

using System.Collections;
using System.IO.Compression;

public class WarcReader : IDisposable
{
    byte[] endOfRecordBuffer = new byte[4];

    Stream fileStream;
    Stream inputStream;

    LineReader lineReader;
    bool isCompressed = false;
    private bool isDisposed;
    int recordNumber = 0;

    bool isEOF = false;

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

        lineReader = new LineReader(inputStream);

        List<string> foo = new List<string>();
    }

    public WarcRecord? GetNextRecord()
    {
        if (isEOF)
        {
            return null;
        }
        RawRecord? rawRecord = GetNextRawRecord();
        if(rawRecord == null)
        {
            isEOF = true;
            return null;
        }
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
            throw new WarcFormatException("Record missing required WARC Type field.", recordNumber, GetFileOffset());
        }
        if (rawRecord.Version == null)
        {
            throw new WarcFormatException("Record missing required WARC Version field.", recordNumber, GetFileOffset());
        }

        if (rawRecord.ContentLength == null)
        {
            throw new WarcFormatException("Record missing required Content-Length field.", recordNumber, GetFileOffset());
        }
    }

    /// <summary>
    /// Reads the stream and returns a minimally parsed record. Returns a null on EOF
    /// </summary>
    /// <returns></returns>
    private RawRecord? GetNextRawRecord()
    {
        RawRecord? nextRecord = GetRecordFields();
        PopulateRecordBody(nextRecord);
        return nextRecord;
    }

    private RawRecord? GetRecordFields()
    {
        var nextRecord = new RawRecord(recordNumber, GetFileOffset());
        recordNumber++;
        lineReader.RecordNumber = recordNumber;

        string? line;
        do
        {
            line = lineReader.GetLine();
            nextRecord.AddHeaderLine(line);
        } while (line != null);

        //if the record is empty, we have hit the end of the file and have no more records
        if (nextRecord.IsEmpty)
        {
            return null;
        }

        //now validate the fields were valid
        EnsureRequiredRawFields(nextRecord);

        return nextRecord;
    }

    private void PopulateRecordBody(RawRecord? rawRecord)
    {
        if(rawRecord == null)
        {
            return;
        }
        try
        {
            //does the record have a body? if so, read it in
            if (rawRecord.ContentLength! > 0)
            {
                //now read in exactly the size of the content bytes
                inputStream.ReadExactly(rawRecord.ContentBytes!, 0, rawRecord.ContentLength.Value);
            }

            //read and verify the trailing CRLFCRLF
            inputStream.ReadExactly(endOfRecordBuffer, 0, 4);
            if (endOfRecordBuffer[0] != 13 ||
                endOfRecordBuffer[1] != 10 ||
                endOfRecordBuffer[2] != 13 ||
                endOfRecordBuffer[3] != 10)
            {
                throw new WarcFormatException("Could not find CRLFCRLF at end of record. Record's Content-Length field may be incorrect.", recordNumber, GetFileOffset());
            }

        }
        catch (EndOfStreamException)
        {
            throw new WarcFormatException("File ends with incomplete record. Record's Content-Length field may be incorrect, or record is prematurely truncated.", recordNumber, GetFileOffset());
        }
    }

    private long? GetFileOffset()
    => inputStream.CanSeek ? inputStream.Position : null;


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
