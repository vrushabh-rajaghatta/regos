using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;

namespace RegOS.ReferenceData.Application.Queries.Blueprint.ListRegulatoryTemplates;

public sealed class ListRegulatoryTemplatesHandler
{
    private readonly RegOSDbContext _dbContext;

    public ListRegulatoryTemplatesHandler(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<RegulatoryTemplateDto>> HandleAsync(
        CancellationToken cancellationToken = default)
    {
        // Small, read-mostly reference data: materialize the tenant-filtered
        // rows, then map — keeping the enum-to-string and strongly-typed-id
        // conversions in memory rather than fighting LINQ translation.
        var templates = await _dbContext.RegulatoryTemplates
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .ThenBy(x => x.Id)
            .ToListAsync(cancellationToken);

        return templates
            .Select(x => new RegulatoryTemplateDto(
                x.Id,
                x.Code,
                x.Name,
                x.AuthorityId,
                x.ApplicationTypeId,
                x.Source,
                x.Status.ToString()))
            .ToList();
    }
}
