namespace Juulsgaard.Tools.FileTypes;

public class FileTypeList<TEnum> : List<FileType<TEnum>> where TEnum : struct, Enum
{
	public TEnum? FindMatch(string filename, string mimetype)
	{
		var fileType = this.FirstOrDefault(x => x.IsMatch(filename, mimetype));
		return fileType?.Value;
	}

	public FileType<TEnum>? GetTypeOrDefault(TEnum value)
	{
		return this.FirstOrDefault(x => x.Value.Equals(value));
	}
	
	public FileType<TEnum> GetType(TEnum value)
	{
		return this.First(x => x.Value.Equals(value));
	}
}