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

	public string? GetLine()
	{
        int bufferPosition = 0;
        while (bufferPosition < lineBuffer.Length)
        {
            int curr = inputStream.ReadByte();
            //we hit an EOF
            if(curr == -1)
            {
                //if we have nothing in our buffer, this is the natural end of the file
                if (bufferPosition == 0)
                {
                    return null;
                }
                //otherwise this is malformed WARC record
                throw new WarcFormatException("Tried to read past the end of the stream. May be a incorrect Content-Length or truncated record.", RecordNumber, GetFileOffset());
            }

            //look for the CRLF ending...
            if (curr == 13)
            {
                //control characters are not allowed in headers, so this must be start of CRLF
                curr = inputStream.ReadByte();
                if (curr != 10)
                {
                    //look for more specific "read passed EOF" condition
                    if(curr == -1)
                    {
                        throw new WarcFormatException("Tried to read past the end of the stream. May be a incorrect Content-Length or truncated record.", RecordNumber, GetFileOffset());
                    }
                    throw new WarcFormatException("Illegal character in field. CR not followed by a LF.", RecordNumber, GetFileOffset());
                }
                //got a CRLF, so return the line
                if (bufferPosition > 0)
                {
                    return System.Text.Encoding.UTF8.GetString(lineBuffer, 0, bufferPosition);
                }
                return null;
            }
            else
            {
                byte b = (byte)curr;

                if(IsInvalidFieldCharacter(b))
                {
                    throw new WarcFormatException($"Illegal character '0x{b.ToString("X2")}' in field.", RecordNumber, GetFileOffset());
                }
                lineBuffer[bufferPosition] = b;
                bufferPosition++;
            }
        }
        throw new WarcFormatException($"WARC field length exceeded {MaxLineSize}. May be a malformed line missing a CRLF.", RecordNumber, GetFileOffset());
    }

    //check for control characters and delete
    public static bool IsInvalidFieldCharacter(byte b)
        => (b < 32 || b == 127);

    private long? GetFileOffset()
        => inputStream.CanSeek ? inputStream.Position : null;
}
