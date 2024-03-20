using DocumentFormat.OpenXml.Wordprocessing;

namespace Juulsgaard.Spreadsheets.Writer.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class SheetColumnAttribute(
	string? name = null,
	bool hidden = false
) : Attribute
{
	internal readonly string? Name = name;
	internal readonly bool Hidden = hidden;
}