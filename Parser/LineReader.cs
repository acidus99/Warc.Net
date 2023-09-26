namespace Warc;

using System.IO;

/// <summary>
/// Reads the lines representing WARC fields from an input stream
/// </summary>
internal class LineReader
{
    const int MaxLineSize = 64 * 1024;

    byte[] lineBuffer = new byte[MaxLineSize];

    Stream inputStream;

    public int RecordNumber { get; set; } = 0;

    public LineReader(Stream input)
    {
        inputStream = input;
    }

	public string GetLine()
	{
        int bufferPosition = 0;
        while (bufferPosition < lineBuffer.Length)
        {
            byte curr = GetByte();

            //look for the CRLF ending...
            if (curr == 13)
            {
                //control characters are not allowed in headers, so this must be start of CRLF
                curr = GetByte();
                if (curr != 10)
                {
                    throw new WarcFormatException("Illegal character in field. CR not followed by a LF.", RecordNumber, GetFileOffset());
                }
                //got a CRLF, so return the line
                if (bufferPosition > 0)
                {
                    return System.Text.Encoding.UTF8.GetString(lineBuffer, 0, bufferPosition);
                }
                return "";
            }
            else
            {
                if(IsInvalidFieldCharacter(curr))
                {
                    throw new WarcFormatException($"Illegal character '0x{curr.ToString("X2")}' in field.", RecordNumber, GetFileOffset());
                }
                lineBuffer[bufferPosition] = curr;
                bufferPosition++;
            }
        }
        throw new WarcFormatException($"WARC field length exceeded {MaxLineSize}. May be a malformed line missing a CRLF.", RecordNumber, GetFileOffset());
    }

    //check for control characters and delete
    public static bool IsInvalidFieldCharacter(byte b)
        => (b < 32 || b == 127);

    private byte GetByte()
    {
        int curr = inputStream.ReadByte();
        if (curr == -1)
        {
            throw new WarcFormatException("Tried to read past the end of the stream. May be a incorrect Content-Length or truncated record.", RecordNumber, GetFileOffset());
        }
        return (byte)curr;
    }

    private long? GetFileOffset()
        => inputStream.CanSeek ? inputStream.Position : null;
}
