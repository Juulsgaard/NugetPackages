using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using Juulsgaard.Spreadsheets.Writer.Attributes;
using Juulsgaard.Tools.Extensions;

namespace Juulsgaard.Spreadsheets.Writer.Tables;

/// <summary>
/// Configuration object for a SheetTable
/// </summary>
/// <typeparam name="T">The row type of the table</typeparam>
public class SheetTableConfig<T> where T : class
{
	public string Name { get; set; }

	private readonly List<PropertyInfo> _order;
	private readonly Dictionary<PropertyInfo, ISheetTableColumnConfig> _properties;
	
	internal SheetTableConfig(string defaultName)
	{
		Name = defaultName;

		_order = [];
		_properties = new();
		foreach (var property in typeof(T).GetProperties()) {
			var columnAttribute = property.GetCustomAttribute<SheetColumnAttribute>();
			if (columnAttribute is {Hidden: true}) continue;
			var name = columnAttribute?.Name ?? property.Name.PascalToSpacedWords();

			if (property.PropertyType.IsAssignableTo(typeof(IDictionary))) {
				_properties.Add(property, new SimpleSheetTableDictConfig(property, name));
			} else {
				_properties.Add(property, new SheetTablePropertyConfig(property, name));
			}
			
			_order.Add(property);
		}
	}

	private PropertyInfo GetProperty<TFunc>(Expression<TFunc> selector)
	{
		var expression = selector.Body;
		
		while (expression is UnaryExpression unaryExpression) {
			expression = unaryExpression.Operand;
		}
		
		if (expression is not MemberExpression member) {
			throw new ArgumentException("Selector has to be a property", nameof(selector));
		}

		if (member.Member.DeclaringType != typeof(T)) {
			throw new ArgumentException("Invalid selector", nameof(selector));
		}

		if (member.Member is not PropertyInfo property) {
			throw new ArgumentException("Selector has to target a property", nameof(selector));
		}

		return property;
	}

	/// <summary>
	/// Configure a collection property
	/// </summary>
	/// <param name="selector">Selector for the property</param>
	/// <returns>A property configuration</returns>
	/// <exception cref="InvalidDataException">Thrown an exception if the property or selector is invalid</exception>
	public ISheetTableDictConfig<TKey, TVal> Property<TKey, TVal>(Expression<Func<T, Dictionary<TKey, TVal>?>> selector) where TKey : notnull
	{
		var property = GetProperty(selector);
		var config = _properties.GetValueOrDefault(property);
		if (config is null) throw new InvalidDataException("This property isn't registered in the table");
		if (config is SheetTableDictConfig<TKey, TVal> dictConfig) return dictConfig;
		
		if (config is not SimpleSheetTableDictConfig simpleConfig) throw new InvalidDataException("Invalid dict property");
		dictConfig = new SheetTableDictConfig<TKey, TVal>(simpleConfig);
		_properties[config.Property] = dictConfig;

		return dictConfig;
	}
	
	/// <summary>
	/// Configure a property
	/// </summary>
	/// <param name="selector">Selector for the property</param>
	/// <returns>A property configuration</returns>
	/// <exception cref="InvalidDataException">Thrown an exception if the property or selector is invalid</exception>
	public ISheetTablePropertyConfig Property(Expression<Func<T, object?>> selector)
	{
		var property = GetProperty(selector);
		var config = _properties.GetValueOrDefault(property);
		if (config is null) throw new InvalidDataException("This property isn't registered in the table");
		if (config is not SheetTablePropertyConfig propConfig) throw new InvalidDataException("Invalid property");
		return propConfig;
	}

	internal IReadOnlyList<ISheetTableColumn> GetColumns(IReadOnlyList<T?> values)
	{
		var columns = new List<ISheetTableColumn>();
		foreach (var propertyInfo in _order) {
			var prop = _properties.GetValueOrDefault(propertyInfo);
			if (prop is null) continue;
			columns.AddRange(prop.ToColumns(values));
		}

		MakeNamesUnique(columns);
		return columns;
	}

	private static void MakeNamesUnique(IEnumerable<ISheetTableColumn> columns)
	{
		var names = new Dictionary<string, int>();
		
		foreach (var column in columns) {
			var nameCount = names.GetValueOrDefault(column.Name);

			// If first encounter, count the name and move on
			if (nameCount <= 0) {
				names[column.Name] = 1;
				continue;
			}
			
			nameCount++;
			
			while (true) {
				// Create new name
				var name = column.Name + nameCount;
				var newNameCount = names.GetValueOrDefault(name);
				
				// If new name already exists, try next
				if (newNameCount > 0) {
					nameCount++;
					continue;
				}

				// If new name isn't present, use it and track it
				column.Name = name;
				names[name] = 1;
				break;
			}
			
			// Persist count
			names[column.Name] = nameCount;
		}
	}

	/// <summary>
	/// Get all properties in the Table Config
	/// </summary>
	public IReadOnlyList<ISheetTableMemberConfig> GetProperties()
	{
		var configs = new List<ISheetTableMemberConfig>();
		foreach (var propertyInfo in _order) {
			var config = _properties.GetValueOrDefault(propertyInfo);
			if (config is not ISheetTableMemberConfig memberConfig) continue;
			configs.Add(memberConfig);
		}

		return configs;
	}
}