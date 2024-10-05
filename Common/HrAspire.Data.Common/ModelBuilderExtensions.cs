namespace HrAspire.Data.Common;

using System.Reflection;

using HrAspire.Data.Common.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

public static class ModelBuilderExtensions
{
    private static readonly MethodInfo SetIsDeletedQueryFilterMethod =
        typeof(ModelBuilderExtensions).GetMethod(nameof(SetIsDeletedQueryFilter), BindingFlags.NonPublic | BindingFlags.Static)!;

    public static ModelBuilder SetUtcKindToDateTimeProperties(this ModelBuilder modelBuilder)
    {
        // Consider all DateTime props in DB as values in UTC timezone
        var specifyUtcDateTimeKindValueConverter = new ValueConverter<DateTime, DateTime>(
            v => v,
            v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

        var properties = modelBuilder.Model
            .GetEntityTypes()
            .SelectMany(e => e.GetProperties().Where(p => p.ClrType == typeof(DateTime) || p.ClrType == typeof(DateTime?)));

        foreach (var property in properties)
        {
            property.SetValueConverter(specifyUtcDateTimeKindValueConverter);
        }

        return modelBuilder;
    }

    public static ModelBuilder SetGlobalIsDeletedQueryFilter(this ModelBuilder modelBuilder)
    {
        // Set global query filter for non-deleted entities only
        var deletableEntityTypes = modelBuilder.Model.GetEntityTypes().Where(et => typeof(IDeletableEntity).IsAssignableFrom(et.ClrType));
        foreach (var deletableEntityType in deletableEntityTypes)
        {
            var method = SetIsDeletedQueryFilterMethod.MakeGenericMethod(deletableEntityType.ClrType);
            method.Invoke(null, [modelBuilder]);
        }

        return modelBuilder;
    }

    public static ModelBuilder DisableForeignKeyCascadeDelete(this ModelBuilder modelBuilder)
    {
        // Disable cascade delete
        var foreignKeys = modelBuilder.Model
            .GetEntityTypes()
            .SelectMany(e => e.GetForeignKeys().Where(f => f.DeleteBehavior == DeleteBehavior.Cascade));

        foreach (var foreignKey in foreignKeys)
        {
            foreignKey.DeleteBehavior = DeleteBehavior.Restrict;
        }

        return modelBuilder;
    }

    private static void SetIsDeletedQueryFilter<T>(ModelBuilder builder)
        where T : class, IDeletableEntity
        => builder.Entity<T>().HasQueryFilter(e => !e.IsDeleted);
}
