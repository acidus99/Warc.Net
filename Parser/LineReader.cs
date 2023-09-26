namespace Warc;

using System.IO;

internal class LineReader
{
    const int MaxLineSize = 64 * 1024;

    byte[] lineBuffer = new byte[MaxLineSize];

    long recordOffset = 0;
    long positionOffset = 0;

    public void SetRecordStart(long startOffset)
    {
        recordOffset = startOffset;
        positionOffset = 0;
    }

	public string GetLine(Stream stream)
	{
        positionOffset = 0;
        int bufferPosition = 0;
        while (bufferPosition < lineBuffer.Length)
        {
            byte curr = GetByte(stream);

            //look for the CRLF ending...
            if (curr == 13)
            {
                //control characters are not allowed in headers, so this must be start of CRLF
                curr = GetByte(stream);
                if (curr != 10)
                {
                    throw new WarcFormatException(GetFileOffset(), "Illegal character in field. CR not followed by a LF.");
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
                    throw new WarcFormatException(GetFileOffset(), $"Illegal character '0x{curr.ToString("X2")}' in field.");
                }
                lineBuffer[bufferPosition] = curr;
                bufferPosition++;
            }
        }
        throw new WarcFormatException(GetFileOffset(), $"WARC field length exceeded {MaxLineSize}. May be a malformed line missing a CRLF.");
    }

    //check for control characters and delete
    public static bool IsInvalidFieldCharacter(byte b)
        => (b < 32 || b == 127);

    private byte GetByte(Stream stream)
    {
        int curr = stream.ReadByte();
        positionOffset++;
        if (curr == -1)
        {
            throw new WarcFormatException(GetFileOffset(), "Tried to read past the end of the stream. May be a incorrect Content-Length or truncated record.");
        }
        return (byte)curr;
    }

    private long GetFileOffset()
        => recordOffset + positionOffset - 1;
}
