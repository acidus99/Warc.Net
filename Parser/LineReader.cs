
namespace Warc;

using System;
using System.IO;

internal class LineReader
{
	Stream input;

    byte[] lineBuffer = new byte[64 * 1024];
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
            byte curr = GetByte();
            if (curr == 13)
            {
                //control characters are not allowed in headers, so this must be start of CRLF
                curr = GetByte();
                if (curr != 10)
                {
                    throw new ApplicationException("LF not following CR");
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
                lineBuffer[position] = curr;
                position++;
            }
        }
        throw new ApplicationException("Line buffer exceeded");
    }

    private byte GetByte()
    {
        int curr = input.ReadByte();
        if (curr == -1)
        {
            throw new ApplicationException("Tried to read past the end of the stream");
        }
        return (byte)curr;
    }
}
