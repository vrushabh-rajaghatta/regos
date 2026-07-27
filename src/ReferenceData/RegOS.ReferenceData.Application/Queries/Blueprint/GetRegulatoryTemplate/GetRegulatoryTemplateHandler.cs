using Microsoft.EntityFrameworkCore;

using RegOS.Persistence;
using RegOS.ReferenceData.Domain.Blueprint;

namespace RegOS.ReferenceData.Application.Queries.Blueprint.GetRegulatoryTemplate;

public sealed class GetRegulatoryTemplateHandler
{
    private readonly RegOSDbContext _dbContext;

    public GetRegulatoryTemplateHandler(RegOSDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<RegulatoryTemplateDetailDto?> HandleAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var templateId = new RegulatoryTemplateId(id);

        var template = await _dbContext.RegulatoryTemplates
            .AsNoTracking()
            .Include(x => x.Versions)
            .FirstOrDefaultAsync(x => x.Id == templateId, cancellationToken);

        if (template is null)
            return null;

        var versions = template.Versions
            .OrderBy(v => v.VersionNumber)
            .Select(v => new RegulatoryTemplateVersionDto(
                v.Id,
                v.VersionNumber,
                v.Status.ToString(),
                v.EffectiveFrom,
                v.EffectiveTo,
                v.PublishedOnUtc))
            .ToList();

        return new RegulatoryTemplateDetailDto(
            template.Id,
            template.Code,
            template.Name,
            template.AuthorityId,
            template.SubmissionTypeId,
            template.Source,
            template.Status.ToString(),
            versions);
    }
}
