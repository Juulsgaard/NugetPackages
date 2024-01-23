using Shared.Interfaces;

namespace Crud.Builders.Extensions;

public static class CrudTargetExtensions
{
	public static CrudTargetConfig<TModel> NotArchived<TModel>(
		this CrudTargetConfig<TModel> target
		) where TModel : class, IArchivable
	{
		target.Where(x => x.ArchivedAt == null);
		return target;
	}

	public static CrudTargetConfig<TModel> IsArchived<TModel>(
		this CrudTargetConfig<TModel> target
	) where TModel : class, IArchivable
	{
		target.Where(x => x.ArchivedAt != null);
		return target;
	}
}