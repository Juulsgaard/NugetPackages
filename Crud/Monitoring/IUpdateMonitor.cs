namespace Crud.Monitoring;

/// <summary>
/// A structure to report changes to a property during a given Update operation
/// </summary>
/// <typeparam name="TModel">The updated model type</typeparam>
public interface IUpdateMonitor<in TModel>
{

	/// <summary>
	/// A boolean indicating whether or not the value changed during the Update
	/// </summary>
	bool Changed { get; }

	/// <summary>
	/// Save the old state
	/// </summary>
	/// <param name="model">The old model state</param>
	void UpdateOld(TModel model);

	/// <summary>
	/// Save the new state
	/// </summary>
	/// <param name="model">The new model state</param>
	void UpdateNew(TModel model);
}