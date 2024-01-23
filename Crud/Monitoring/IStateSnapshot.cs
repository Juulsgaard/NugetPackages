namespace Crud.Monitoring;

public interface IStateSnapshot<out TModel>
{
	TModel GetValue();

	bool Compare<TOther>(IStateSnapshot<TOther> snapshot);
}