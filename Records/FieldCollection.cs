namespace WarcDotNet;

using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Implements a collection of WARC fields
/// * Handles duplicate field names
/// * Ensures names/values follow allow character rules from WARC 1.1 spec
/// * Handles case-insensitive field names
/// </summary>
public class FieldCollection
{
    Dictionary<string, List<string>> BackingStore;
    Dictionary<string, string> FieldNames;

    public int Count
        => BackingStore.Count;

    public IEnumerable<string> Fields
        => FieldNames.Values;

    public FieldCollection()
    {
        BackingStore = new Dictionary<string, List<string>>();
        FieldNames = new Dictionary<string, string>();
    }

    public IEnumerable<string> this[string fieldName]
    {
        get
        {
            var normalizedKey = NormalizeFieldName(fieldName);
            if (!BackingStore.ContainsKey(normalizedKey))
            {
                throw new KeyNotFoundException($"Field name '{fieldName}' does not exist in collection.");
            }
            return BackingStore[normalizedKey];
        }
    }

    /// <summary>
    /// Adds a specific field to the collection
    /// </summary>
    /// <param name="fieldName"></param>
    /// <param name="value"></param>
    public void Add(string fieldName, string value)
    {
        if(!FieldValidator.IsAllowedName(fieldName))
        {
            throw new FormatException($"Field name '{fieldName}' contains illegal character");
        }

        if(!FieldValidator.IsAllowedValue(value))
        {
            throw new FormatException($"Field value '{value}' contains illegal character");
        }

        var normalizedKey = NormalizeFieldName(fieldName);
        if (!BackingStore.ContainsKey(normalizedKey))
        {
            BackingStore[normalizedKey] = new List<string>();
        }
        BackingStore[normalizedKey].Add(value);
        FieldNames[normalizedKey] = fieldName;
    }

    /// <summary>
    /// Does a field exist in this collection?
    /// </summary>
    /// <param name="fieldName"></param>
    /// <returns></returns>
    public bool ContainsField(string fieldName)
    {
        var normalizedKey = NormalizeFieldName(fieldName);
        return BackingStore.ContainsKey(normalizedKey);
    }

    /// <summary>
    /// How many types does a custom field appear in the collection?
    /// </summary>
    /// <param name="fieldName"></param>
    /// <returns></returns>
    public int FieldCount(string fieldName)
    {
        var normalizedKey = NormalizeFieldName(fieldName);
        if (!BackingStore.ContainsKey(normalizedKey))
        {
            return 0;
        }
        return BackingStore[normalizedKey].Count;
    }

    /// <summary>
    /// Removes all values for a given field
    /// </summary>
    /// <param name="fieldName"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    /// <exception cref="KeyNotFoundException"></exception>
    public bool RemoveAll(string fieldName)
    {
        var normalizedKey = NormalizeFieldName(fieldName);
        FieldNames.Remove(normalizedKey);
        return BackingStore.Remove(normalizedKey);
    }

    private string NormalizeFieldName(string fieldName)
        => fieldName.ToLower();
}

