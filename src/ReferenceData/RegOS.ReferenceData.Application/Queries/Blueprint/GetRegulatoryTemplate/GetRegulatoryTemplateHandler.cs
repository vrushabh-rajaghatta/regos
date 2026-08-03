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
                .ThenInclude(v => v.Sections)
            .Include(x => x.Versions)
                .ThenInclude(v => v.RequiredDocuments)
            .Include(x => x.Versions)
                .ThenInclude(v => v.ValidationRules)
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
                v.PublishedOnUtc,
                v.Sections
                    .OrderBy(s => s.Order)
                    .Select(s => new TemplateSectionDto(
                        s.Id,
                        s.Code,
                        s.Title,
                        s.ParentSectionId?.Value,
                        s.Order))
                    .ToList(),
                v.RequiredDocuments
                    .OrderBy(d => d.Order)
                    .Select(d => new RequiredDocumentDto(
                        d.Id,
                        d.SectionId,
                        d.DocumentTypeId,
                        d.IsMandatory,
                        d.Order))
                    .ToList(),
                v.ValidationRules
                    .OrderBy(r => r.Order)
                    .Select(r => new ValidationRuleDto(
                        r.Id,
                        r.Code,
                        r.RuleType.ToString(),
                        r.Severity.ToString(),
                        r.SectionId?.Value,
                        r.Parameters,
                        r.Message,
                        r.Order))
                    .ToList()))
            .ToList();

        return new RegulatoryTemplateDetailDto(
            template.Id,
            template.Code,
            template.Name,
            template.AuthorityId,
            template.ApplicationTypeId,
            template.Source,
            template.Status.ToString(),
            versions);
    }
}
