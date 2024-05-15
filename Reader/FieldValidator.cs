namespace WarcDotNet;

/// <summary>
/// Helper classes to make sure that validators
/// </summary>
internal static class FieldValidator
{
    public static bool IsAllowedName(char c)
    {
        //per section 4 of the WARC 1.1 spec
        const string separators = "()<>@,;:\\\"/[]?={} \t";

        if (!IsAllowedValue(c))
        {
            return false;
        }
        //names have additional rules
        return !separators.Contains(c);
    }

    public static bool IsAllowedName(string s)
    {
        foreach (char c in s)
        {
            if (!IsAllowedName(c))
            {
                return false;
            }
        }
        return true;
    }

    public static bool IsAllowedValue(char c)
    {
        return (!char.IsControl(c) && c != 127);
    }

    public static bool IsAllowedValue(string s)
    {
        foreach (char c in s)
        {
            if (!IsAllowedValue(c))
            {
                return false;
            }
        }
        return true;
    }
}