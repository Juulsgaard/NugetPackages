﻿namespace Juulsgaard.SpreadsheetReader.Models;

public interface ISheetColumn
{
	public string Name { get; }
	public string Slug { get; }
	public bool Hidden { get; }
	public int Position { get; }
}