using EntityFramework.Exceptions.Common;
using Juulsgaard.Crud.Exceptions;
using Juulsgaard.Crud.Models;
using Juulsgaard.Tools.Extensions;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace Juulsgaard.Crud.Extensions;

public static class ExceptionExtensions
{
	public static DatabaseException Process(this DbUpdateException exception)
	{
		switch (exception) {
			case DbUpdateConcurrencyException concurrency:

				Log.Error(concurrency, "Concurrency error while updating DB");

				return new DatabaseConflictException(
					"Someone else has edited this data",
					concurrency.InnerException
				);

			case UniqueConstraintException unique: {
				var e = new DatabaseConflictException(
					$"Data conflict",
					unique.InnerException
				);

				e.DecorateLogger(Log.Logger).Error(unique, "DB Update broke Unique Constraint");

				return e;
			}
			case CannotInsertNullException nullInsert: {
				var columnName = (string?)nullInsert.InnerException?.Data.ReadValueOrDefault("ColumnName");

				Log.Error(
					nullInsert,
					"DB Update resulted in a null value in non-nullable field ({ColumnName})",
					columnName ?? "[not found]"
				);

				return new DatabaseException(
					$"Invalid state{(columnName != null ? $" - {columnName}" : "")}",
					nullInsert.InnerException
				);
			}
			default:
				Log.Error(
					exception,
					"Database error while saving changes: {InnerMessage}",
					exception.InnerException?.Message ?? "No Details"
				);

				return new DatabaseException(
					"Failed to update database",
					exception.InnerException ?? exception
				);
		}
	}

	public static DatabaseException ProcessAsUpdate(
		this DbUpdateException exception,
		string entityName,
		CrudExceptionLookup? exceptionLookup = null
	)
	{
		switch (exception) {
			case DbUpdateConcurrencyException concurrency:

				Log.Error(concurrency, "Concurrency error while updating {EntityName}", entityName);

				return new DatabaseConflictException(
					exceptionLookup?.Concurrency ?? $"Someone else has edited this {entityName}",
					concurrency.InnerException
				);

			case UniqueConstraintException unique: {
				var e = new DatabaseConflictException(
					exceptionLookup?.UniqueConflict ?? $"This version of {entityName} already exists",
					unique.InnerException
				);

				e.DecorateLogger(Log.Logger).Error(unique, "Update of {EntityName} violates Unique Constraint", entityName);

				return e;
			}
			case CannotInsertNullException nullInsert: {
				var columnName = (string?)nullInsert.InnerException?.Data.ReadValueOrDefault("ColumnName");

				Log.Error(
					nullInsert,
					"Tried to insert null in non-nullable field ({ColumnName}) while updating {EntityName}",
					columnName ?? "[not found]",
					entityName
				);

				return new DatabaseException(
					exceptionLookup?.NullInsert
				 ?? $"Cannot update {entityName} with null value{(columnName != null ? $" - {columnName}" : "")}",
					nullInsert.InnerException
				);
			}
			default:
				Log.Error(
					exception,
					"Database error while updating {EntityName}: {InnerMessage}",
					entityName,
					exception.InnerException?.Message ?? "No Details"
				);

				return new DatabaseException(
					exceptionLookup?.Default ?? $"Failed to update {entityName}",
					exception.InnerException ?? exception
				);
		}
	}

	public static DatabaseException ProcessAsMove(
		this DbUpdateException exception,
		string entityName,
		CrudExceptionLookup? exceptionLookup = null
	)
	{
		if (exception is DbUpdateConcurrencyException concurrency) {
			Log.Error(concurrency, "Concurrency error while moving {EntityName}", entityName);

			return new DatabaseConflictException(
				exceptionLookup?.Concurrency ?? $"Someone else has edited this {entityName}",
				concurrency.InnerException
			);
		}

		Log.Error(
			exception,
			"Database error while moving {EntityName}: {InnerMessage}",
			entityName,
			exception.InnerException?.Message ?? "No Details"
		);

		return new DatabaseException(exceptionLookup?.Default ?? $"Failed to move {entityName}", exception.InnerException ?? exception);
	}

	public static DatabaseException ProcessAsArchive(
		this DbUpdateException exception,
		string entityName,
		CrudExceptionLookup? exceptionLookup = null
	)
	{
		switch (exception) {
			case DbUpdateConcurrencyException concurrency:

				Log.Error(concurrency, "Concurrency error while archiving {EntityName}", entityName);

				return new DatabaseConflictException(
					exceptionLookup?.Concurrency ?? $"Someone else has archived this {entityName}",
					concurrency.InnerException
				);

			case UniqueConstraintException unique:

				var e = new DatabaseConflictException(
					exceptionLookup?.UniqueConflict ?? $"This {entityName} has dependencies that need to be archived first",
					unique.InnerException
				);

				e.DecorateLogger(Log.Logger).Error(unique, "Archival of {EntityName} violates Unique Constraint", entityName);

				return e;

			case CannotInsertNullException nullInsert: {
				var columnName = (string?)nullInsert.InnerException?.Data.ReadValueOrDefault("ColumnName");

				Log.Error(
					nullInsert,
					"Archival of {EntityName} resulted in a null entry for a non-nullable column ({ColumnName})",
					entityName,
					columnName ?? "[not found]"
				);

				return new DatabaseException(exceptionLookup?.NullInsert ?? $"Cannot archive {entityName}", nullInsert.InnerException);
			}
			default:

				Log.Error(
					exception,
					"Database error while archiving {EntityName}: {InnerMessage}",
					entityName,
					exception.InnerException?.Message ?? "No Details"
				);

				return new DatabaseException(
					exceptionLookup?.Default ?? $"Failed to archive {entityName}",
					exception.InnerException ?? exception
				);
		}
	}

	public static DatabaseException ProcessAsCreate(
		this DbUpdateException exception,
		string entityName,
		CrudExceptionLookup? exceptionLookup = null
	)
	{
		switch (exception) {
			case DbUpdateConcurrencyException concurrency:

				Log.Error(concurrency, "Concurrency error while creating {EntityName}", entityName);

				return new DatabaseConflictException(
					exceptionLookup?.Concurrency ?? $"Someone else has edited this {entityName}",
					concurrency.InnerException
				);

			case UniqueConstraintException unique:

				var e = new DatabaseConflictException(
					exceptionLookup?.UniqueConflict ?? $"This {entityName} already exists",
					unique.InnerException
				);

				e.DecorateLogger(Log.Logger).Error(unique, "Creation of {EntityName} violates Unique Constraint", entityName);

				return e;

			case CannotInsertNullException nullInsert: {
				var columnName = (string?)nullInsert.InnerException?.Data.ReadValueOrDefault("ColumnName");

				Log.Error(
					nullInsert,
					"Tried to insert null in non-nullable field ({ColumnName}) while creating {EntityName}",
					columnName ?? "[not found]",
					entityName
				);

				return new DatabaseException(
					exceptionLookup?.NullInsert
				 ?? $"Cannot create {entityName} with null value{(columnName != null ? $" - {columnName}" : "")}",
					nullInsert.InnerException
				);
			}

			default:

				Log.Error(
					exception,
					"Database error while creating {EntityName}: {InnerMessage}",
					entityName,
					exception.InnerException?.Message ?? "No Details"
				);

				return new DatabaseException(
					exceptionLookup?.Default ?? $"Failed to create {entityName}",
					exception.InnerException ?? exception
				);
		}
	}

	public static DatabaseException ProcessAsDelete(
		this DbUpdateException exception,
		string entityName,
		CrudExceptionLookup? exceptionLookup = null
	)
	{
		switch (exception) {
			case DbUpdateConcurrencyException concurrency:

				Log.Error(concurrency, "Concurrency error while deleting {EntityName}", entityName);

				return new DatabaseConflictException(
					exceptionLookup?.Concurrency ?? $"Someone else has deleted this {entityName}",
					concurrency.InnerException
				);

			case UniqueConstraintException unique:

				var e = new DatabaseConflictException(
					exceptionLookup?.UniqueConflict ?? $"This {entityName} has dependencies that need to be deleted first",
					unique.InnerException
				);

				e.DecorateLogger(Log.Logger).Error(unique, "Deletion of {EntityName} violates Unique Constraint", entityName);

				return e;

			case CannotInsertNullException nullInsert: {
				var columnName = (string?)nullInsert.InnerException?.Data.ReadValueOrDefault("ColumnName");

				Log.Error(
					nullInsert,
					"Deletion of {EntityName} resulted in a null entry for a non-nullable column ({ColumnName})",
					entityName,
					columnName ?? "[not found]"
				);

				return new DatabaseException(exceptionLookup?.NullInsert ?? $"Cannot delete {entityName}", nullInsert.InnerException);
			}

			default:
				Log.Error(
					exception,
					"Database error while deleting {EntityName}: {InnerMessage}",
					entityName,
					exception.InnerException?.Message ?? "No Details"
				);

				return new DatabaseException(
					exceptionLookup?.Default ?? $"Failed to delete {entityName}",
					exception.InnerException ?? exception
				);
		}
	}
}