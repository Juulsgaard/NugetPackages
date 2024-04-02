namespace Juulsgaard.Spreadsheets.Writer;

/// <summary>
/// Defines a 2D range of data in a sheet
/// </summary>
public readonly struct SheetRange
{
	public required uint Top { get; init; }
	public required uint Left { get; init; }
	public required uint Bottom { get; init; }
	public required uint Right { get; init; }

	/// <summary>
	/// Create a range based on a box
	/// </summary>
	/// <param name="x">X coordinate</param>
	/// <param name="y">Y coordinate</param>
	/// <param name="width">Range width</param>
	/// <param name="height">Range height</param>
	/// <exception cref="ArgumentException">Thrown an exception if the box has no height or width</exception>
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

	/// <summary>
	/// Create a range based on an Excel address range
	/// </summary>
	/// <param name="reference">The range reference</param>
	/// <exception cref="InvalidDataException">Thrown an exception if the format of the range is invalid</exception>
	public static SheetRange FromReference(string reference)
	{
		var split = reference.Split(':');
		if (split.Length != 2) throw new InvalidDataException("Invalid range format");
		var (left, top) = SheetWriterHelper.GetCellCoordinates(split[0]);
		var (right, bottom) = SheetWriterHelper.GetCellCoordinates(split[0]);
		return new SheetRange { Left = left, Top = top, Right = right, Bottom = bottom };
	}

	/// <summary>
	/// Format the range as an Excel address range
	/// </summary>
	/// <returns></returns>
	public string ToReference()
	{
		var min = SheetWriterHelper.IndexToColumnName(Left) + (Top + 1);
		var max = SheetWriterHelper.IndexToColumnName(Right) + (Bottom + 1);
		return $"{min}:{max}";
	}
}