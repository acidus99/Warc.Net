using System;
using System.Text;

namespace Warc
{
	/// <summary>
	/// Helper class for creating WARC files for warcinfo and metadata records
	/// </summary>
	public class WarcFields : List<WarcField>
	{
		public const string ContentType = "application/warc-fields";

		public void Add(string name, string value)
		{
			Add(new WarcField { Name = name, Value = value });
		}

		public void Add(string name, object value)
			=> Add(name, value?.ToString() ?? "");

        public byte[]? ToBytes()
		{
            if (Count ==0)
            {
                return null;
            }
            StringBuilder sb = new StringBuilder();
            foreach (var field in this)
            {
                sb.AppendLine($"{field.Name}: {field.Value}");
            }
			return Encoding.UTF8.GetBytes(sb.ToString());
        }
	}

	public class WarcField
	{
		public required string Name { get; set; }

		public required string Value { get; set; }
	}
}

