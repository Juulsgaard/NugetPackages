namespace Juulsgaard.Spreadsheets.Writer.Attributes;

/// <summary>
/// Assign metadata to a Sheet Column
/// </summary>
/// <param name="name">A custom name</param>
/// <param name="hidden">Hidden columns are skipped</param>
[AttributeUsage(AttributeTargets.Property)]
public class SheetColumnAttribute(
	string? name = null,
	bool hidden = false
) : Attribute
{
	internal readonly string? Name = name;
	internal readonly bool Hidden = hidden;
}