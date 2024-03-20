using System.ComponentModel;

namespace Juulsgaard.Spreadsheets.Writer;

public struct SheetRange
{
	public required uint Top { get; init; }
	public required uint Left { get; init; }
	public required uint Bottom { get; init; }
	public required uint Right { get; init; }

	public static SheetRange FromBox(uint x, uint y, uint width, uint height)
	{
		if (width <= 0) throw new ArgumentException("Box width cannot be 0", nameof(width));
		if (height <= 0) throw new ArgumentException("Box height cannot be 0", nameof(height));
		
		return new() {
			Left = x,
			Top = y,
			Right = x + (width - 1),
			Bottom = y + (height - 1)
		};
	}

	public static SheetRange FromReference(string reference)
	{
		var split = reference.Split(':');
		if (split.Length != 2) throw new InvalidDataException("Invalid range format");
		var (left, top) = SheetWriterHelper.GetCellCoordinates(split[0]);
		var (right, bottom) = SheetWriterHelper.GetCellCoordinates(split[0]);
		return new SheetRange { Left = left, Top = top, Right = right, Bottom = bottom };
	}

	public string ToReference()
	{
		var min = SheetWriterHelper.IndexToColumnName(Left) + (Top + 1);
		var max = SheetWriterHelper.IndexToColumnName(Right) + (Bottom + 1);
		return $"{min}:{max}";
	}
}