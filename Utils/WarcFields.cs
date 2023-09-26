namespace Warc;

using System;
using System.Text;

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

        public override string ToString()
        {
            if (Count ==0)
            {
                return "";
            }
            StringBuilder sb = new StringBuilder();
		this.ForEach(x => sb.AppendLine($"{x.Name}: {x.Value}")); ;
		return sb.ToString();
        }
}

/// <summary>
/// name/value pairs 
/// </summary>
public class WarcField
{
	public required string Name { get; set; }

	public required string Value { get; set; }
}
