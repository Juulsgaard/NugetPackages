using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Juulsgaard.Tools.Extensions;

public static class StringExtensions
{
	/// <summary>
	/// Turns the string into a Slug
	/// </summary>
	/// <param name="str">Original string</param>
	/// <returns>Slug</returns>
	public static string Slugify(this string str)
	{
		return Regex.Replace(str.Trim(), @"\W+", "-").ToLower();
	}

	/// <summary>
	/// Normalises a string
	/// </summary>
	/// <param name="str">The string to normalise</param>
	/// <returns></returns>
	public static string DbNormalise(this string str)
	{
		return str.Trim().ToUpper();
	}

	/// <summary>
	/// Converts Pascal case to a fully spaced out sentence
	/// </summary>
	/// <param name="str">Original string</param>
	/// <param name="lowercaseFirstLetter">Force first letter into lowercase</param>
	/// <returns>The sentence</returns>
	public static string PascalToSpacedWords(this string? str, bool lowercaseFirstLetter = false)
	{
		var name = str.LowerFirst();
		name = Regex.Replace(name, @"[A-Z]", match => $" {match.Value.ToLower()}");
		return lowercaseFirstLetter ? name : char.ToUpper(name[0]) + name[1..];
	}

	/// <summary>
	/// Set the first letter to Uppercase
	/// </summary>
	/// <param name="str">The original string</param>
	/// <returns>New string</returns>
	public static string UpperFirst(this string? str)
	{
		if (string.IsNullOrWhiteSpace(str)) return "";
		return char.ToUpper(str[0]) + (str.Length > 1 ? str[1..] : "");
	}

	/// <summary>
	/// Set the first letter to Lowercase
	/// </summary>
	/// <param name="str">The original string</param>
	/// <returns>New string</returns>
	public static string LowerFirst(this string? str)
	{
		if (string.IsNullOrWhiteSpace(str)) return "";
		return char.ToLower(str[0]) + (str.Length > 1 ? str[1..] : "");
	}

	/// <summary>
	/// Converts a string to Title Case
	/// </summary>
	/// <param name="str"></param>
	/// <returns></returns>
	public static string ToTitleCase(this string? str)
	{
		if (string.IsNullOrWhiteSpace(str)) return "";
		return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(str.ToLower());
	}
	
	/// <summary>
	/// Converts a string to kebab-case
	/// </summary>
	/// <param name="str"></param>
	/// <returns></returns>
	public static string ToKebabCase(this string? str)
	{
		if (string.IsNullOrWhiteSpace(str)) return "";
		str = str.Trim().LowerFirst();
		str = Regex.Replace(str, @"[A-Z]", match => $"-{match.Value.ToLower()}");
		str = Regex.Replace(str, @"\W+", "-");
		return str;
	}

	/// <summary>
	/// Replaces the file extension of a string
	/// </summary>
	/// <param name="fileName">The file name</param>
	/// <param name="extension">The new extension</param>
	/// <returns></returns>
	public static string ReplaceExtension(this string fileName, string extension)
	{
		if (!fileName.Contains('.')) return $"{fileName}.{extension}";

		return Regex.Replace(fileName, @"\.[^\.]+$", $".{extension}");
	}

	/// <summary>
	/// Created a Sha256 hash from a string
	/// </summary>
	/// <param name="str">Original string</param>
	/// <returns>SHA256 Hash</returns>
	public static string Sha256Hash(this string str)
	{
		using SHA256 sha256Hash = SHA256.Create();

		// ComputeHash - returns byte array  
		byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(str));

		// Convert byte array to a string   
		StringBuilder builder = new StringBuilder();
		for (int i = 0; i < bytes.Length; i++) {
			builder.Append(bytes[i].ToString("x2"));
		}

		return builder.ToString();
	}

	/// <summary>
	/// Removes all HTML tags from the string
	/// </summary>
	/// <param name="str"></param>
	/// <returns></returns>
	public static string StripHtmlTags(this string str)
	{
		return Regex.Replace(str, @"<[^>]+>", "");
	}

	/// <summary>
	/// Converts a string into an enum, provided that the enum is in PascalCase
	/// </summary>
	/// <param name="str"></param>
	/// <typeparam name="TEnum"></typeparam>
	/// <returns></returns>
	public static TEnum ToPascalEnum<TEnum>(this string str) where TEnum : struct, Enum
	{
		str = Regex.Replace(str, @"-[a-z]", x => x.Value[1..].ToUpper());
		return Enum.Parse<TEnum>(str.Trim().UpperFirst().Replace("-", ""));
	}

	/// <summary>
	/// Normalises a URL by trimming and removing any trailing slashes
	/// </summary>
	/// <param name="url"></param>
	/// <returns></returns>
	public static string NormaliseUrl(this string url)
	{
		return url.Trim().TrimEnd('/').ToLower();
	}
	
	/// <summary>
	/// Returns true if a string is null or whitespace
	/// </summary>
	/// <param name="str"></param>
	/// <returns></returns>
	public static bool IsEmpty([NotNullWhen(false)] this string? str)
	{
		return string.IsNullOrWhiteSpace(str);
	}
	
	/// <summary>
	/// Returns true if a string is not null and not whitespace
	/// </summary>
	/// <param name="str"></param>
	/// <returns></returns>
	public static bool IsNotEmpty([NotNullWhen(true)] this string? str)
	{
		return !string.IsNullOrWhiteSpace(str);
	}
	
	/// <summary>
	/// Returns null if the string is null or whitespace.
	/// Otherwise it returns the string unaltered.
	/// </summary>
	/// <param name="str"></param>
	/// <returns></returns>
	public static string? EmptyAsNull(this string? str)
	{
		return string.IsNullOrWhiteSpace(str) ? null : str;
	}
	
	/// <summary>
	/// Turns a nullable string into a non-nullable string by replacing null with an empty string
	/// </summary>
	/// <param name="str"></param>
	/// <returns></returns>
	public static string NullAsEmpty(this string? str)
	{
		return str ?? "";
	}
}