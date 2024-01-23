using Microsoft.EntityFrameworkCore.Metadata;

namespace Crud.Monitoring;

public class ListStateSnapshot<TModel> : IStateSnapshot<List<TModel>>
{
	private IEnumerable<IStateSnapshot<TModel>>? _snapshots;
	private List<TModel> _value;

	public ListStateSnapshot(IEnumerable<TModel>? model, IEntityType? entityType)
	{
		if (model == null) {
			_value = new();
			return;
		}
		
		if (entityType != null) {
			_snapshots = model.Select(x => new EntityStateSnapshot<TModel>(x, entityType)).ToList();
		} else {
			_snapshots = model.Select(x => new ValueStateSnapshot<TModel>(x)).ToList();
		}

		_value = _snapshots.Select(x => x.GetValue()).ToList();
	}

	public List<TModel> GetValue()
	{
		return _value;
	}

	public bool Compare<TOther>(IStateSnapshot<TOther> snapshot)
	{
		if (snapshot is not ListStateSnapshot<TModel> listSnapshot) {
			var value = GetValue();
			var otherValue = snapshot.GetValue();
			if (otherValue == null) return false;
			return value.Equals(otherValue); 
		}
		
		if (_snapshots == null) return listSnapshot._snapshots == null;
		if (listSnapshot._snapshots == null) return false;

		var snapshots = _snapshots.ToArray();
		var otherSnapshots = listSnapshot._snapshots.ToArray();
		
		if (snapshots.Length != otherSnapshots.Length) return false;
		for (var i = snapshots.Length - 1; i >= 0; i--) {
			if (!snapshots[i].Compare(otherSnapshots[i])) return false;
		}

		return true;
	}
}