namespace Warc;

using System.IO;

internal class LineReader
{
    const int MaxLineSize = 64 * 1024;

    Stream input;

    byte[] lineBuffer = new byte[MaxLineSize];
    int position;

    public LineReader(Stream stream)
	{
        input = stream;
	}

	public string GetLine()
	{
        position = 0;
        while (position < lineBuffer.Length)
        {
            long offset = input.Position;

            byte curr = GetByte();

            //look for the CRLF ending...
            if (curr == 13)
            {
                //control characters are not allowed in headers, so this must be start of CRLF
                curr = GetByte();
                if (curr != 10)
                {
                    throw new WarcFormatException(offset+1, "Illegal character in field. CR not followed by a LF.");
                }
                //got a CRLF, so return the line
                if (position > 0)
                {
                    return System.Text.Encoding.UTF8.GetString(lineBuffer, 0, position);
                }
                return "";
            }
            else
            {
                if(IsInvalidFieldCharacter(curr))
                {
                    throw new WarcFormatException(offset, $"Illegal character '0x{curr.ToString("X2")}' in field.");
                }
                lineBuffer[position] = curr;
                position++;
            }
        }
        throw new WarcFormatException(input.Position, $"WARC field length exceeded {MaxLineSize}. May be a malformed line missing a CRLF.");
    }

    //check for control characters and delete
    public static bool IsInvalidFieldCharacter(byte b)
        => (b < 32 || b == 127);

    private byte GetByte()
    {
        int curr = input.ReadByte();
        if (curr == -1)
        {
            throw new WarcFormatException(input.Position, "Tried to read past the end of the stream. May be a incorrect Content-Length or truncated record.");
        }
        return (byte)curr;
    }
}
