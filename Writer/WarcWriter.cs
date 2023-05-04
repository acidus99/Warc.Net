namespace Warc;

using System;
using System.IO;
using System.IO.Compression;
using System.Text;

/// <summary>
/// Represents a writer for WARC files that are formatted according to version 1.1 and 1.0.
/// </summary>
public class WarcWriter : IDisposable
{

    private const byte CarriageReturn = 13;
    private const byte LineFeed = 10;

    // The file to be written to
    private readonly FileStream fout;

    private bool isDisposed;

    public WarcWriter(string filePath, bool isForcedCompression = false)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            // FIXME: Add a test case
            throw new ArgumentNullException(nameof(filePath), "Destination path and filename for the WARC must be specified");
        }

        var info = new FileInfo(filePath);

        // Ensures that the path exists
        if (!Directory.Exists(info.DirectoryName))
        {
            // FIXME: Add a test case
            throw new ArgumentException("Path to output WARC doesn't exist.", nameof(filePath));
        }

        Filepath = filePath;
        fout = new FileStream(Filepath, FileMode.Create);

        // Checks whether per-record compression based on *.gz file extension is used
        if (info.Extension == ".gz")
        {
            IsCompressed = true;
        }
        else if (isForcedCompression)
        {
            // FIXME: Add a test case
            IsCompressed = true;
        }
        else
        {
            IsCompressed = false;
        }
    }

    /// <summary>
    /// Gets, for this instance, the full path of the WARC file to be written to.
    /// </summary>
    public string Filepath { get; private set; }

    /// <summary>
    /// Gets, for this instance, an indication of whether per-record GZIP compression is used.
    /// </summary>
    /// <remarks>This is controlled by the file extension of the WARC or via the
    /// <c>isForcedComparession</c> parameter that is passed to the constructor.</remarks>
    public bool IsCompressed { get; private set; }

    /// <summary>
    /// Gets, for this instance, the current size of the WARC.
    /// </summary>
    /// <remarks>This is useful to know when splitting a large record into multiple smaller WARCs.</remarks>
    public long Length => fout.Length;

    public void Dispose()
    {
        Dispose(isDisposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Writes the specified <paramref name="record"/> to the WARC output.
    /// </summary>
    /// <param name="record">A <see cref="Record"/>.</param>
    /// <remarks>Per-record compression is used, if applicable.</remarks>
    public void Write(WarcRecord record)
    {
        if (IsCompressed)
        {
            fout.Write(CompressRecord(record));
        }
        else
        {
            WriteRecordToStream(record, fout);
        }
    }

    protected virtual void Dispose(bool isDisposing)
    {
        if (isDisposed)
        {
            return;
        }

        if (isDisposing)
        {
            // Ensures that the WARC file stream is disposed
            ((IDisposable)fout).Dispose();
        }

        isDisposed = true;
    }

    /// <summary>
    /// Compresses the specified <paramref name="record"/> to a gzipped byte array.
    /// </summary>
    /// <param name="record">A <see cref="Record"/>.</param>
    /// <returns>gzipped byte array.</returns>
    private static byte[] CompressRecord(WarcRecord record)
    {
        using var memoryStream = new MemoryStream();
        using (GZipStream gzipStream = new GZipStream(memoryStream, CompressionMode.Compress))
        {
            WriteRecordToStream(record, gzipStream);
        }

        return memoryStream.ToArray();
    }

    /// <summary>
    /// Outputs the specified <paramref name="record"/> to the specified <paramref name="stream"/>.
    /// </summary>
    /// <param name="record">A <see cref="Record"/>.</param>
    /// <param name="stream">A <see cref="Stream"/>.</param>
    /// <remarks>Handles the headers and optional block.</remarks>
    private static void WriteRecordToStream(WarcRecord record, Stream stream)
    {
        // Writes the header
        var header = record.GetHeaders();
        stream.Write(Encoding.UTF8.GetBytes(header));
        stream.WriteByte(CarriageReturn);
        stream.WriteByte(LineFeed);

        // Writes the block, if any
        if(record.ContentLength > 0)
        {
            stream.Write(record.ContentBlock);
        }

        // Delimits a record with exactly two CRLFs
        stream.WriteByte(CarriageReturn);
        stream.WriteByte(LineFeed);
        stream.WriteByte(CarriageReturn);
        stream.WriteByte(LineFeed);
    }
}