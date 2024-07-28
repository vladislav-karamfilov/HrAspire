namespace HrAspire.Employees.Business.Mappers;

using HrAspire.Employees.Business.Documents;
using HrAspire.Employees.Data.Models;

using Riok.Mapperly.Abstractions;

[Mapper]
internal static partial class DocumentMapper
{
    public static partial IQueryable<DocumentServiceModel> ProjectToServiceModel(this IQueryable<Document> documentsQuery);

    public static partial IQueryable<DocumentDetailsServiceModel> ProjectToDetailsServiceModel(this IQueryable<Document> documentsQuery);
}
