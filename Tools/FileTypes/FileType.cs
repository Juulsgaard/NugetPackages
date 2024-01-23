namespace Juulsgaard.Tools.FileTypes;

public class FileType<TEnum> where TEnum : struct, Enum
{
	public required TEnum Value { get; init; }
	public List<string>? MimeTypes { get; init; }
	public List<string>? Extensions { get; init; }
	public Func<string, bool>? CustomValidation { get; init; }

	public bool IsMatch(string fileName, string mimeType)
	{
		if (MimeTypes?.Contains(mimeType) ?? false) {
			return true;
		}
		
		if (CustomValidation?.Invoke(mimeType) ?? false) {
			return true;
		}

		var extension = Path.GetExtension(fileName).TrimStart('.').ToLower();
		
		return Extensions?.Contains(extension) ?? false;
	}
}