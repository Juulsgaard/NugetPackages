using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using Juulsgaard.Spreadsheets.Writer.Attributes;
using Juulsgaard.Tools.Extensions;

namespace Juulsgaard.Spreadsheets.Writer.Tables;

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

			if (property.PropertyType.ExtendsRawGeneric(typeof(Dictionary<,>))) {
				_properties.Add(property, new SheetTableDictConfig<object, object?>(property, name));
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

	public ISheetTableDictConfig<TKey, TVal> Property<TKey, TVal>(Expression<Func<T, Dictionary<TKey, TVal>?>> selector) where TKey : notnull
	{
		var property = GetProperty(selector);
		var config = _properties.GetValueOrDefault(property);
		if (config is null) throw new InvalidDataException("This property isn't registered in the table");
		if (config is not SheetTableDictConfig<TKey, TVal> dictConfig) throw new InvalidDataException("Invalid dict property");
		return dictConfig;
	}
	
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

		return columns;
	}

	public IReadOnlyList<ISheetTableMemberConfig> GetProperties(IReadOnlyList<T?> values)
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