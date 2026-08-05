using RegOS.Registration.Domain.Aggregates.SiteApprovals;

namespace RegOS.Registration.Application.Commands.WithdrawSiteApproval;

/// <summary>
/// Removes an approval recorded in error.
/// </summary>
/// <remarks>
/// <b>A correction, not a variation.</b> This is for an approval that was never
/// true — recorded against the wrong licence, or the wrong site. A site
/// genuinely <em>removed</em> from a licence by variation is a different act
/// with its own date, and when somebody asks for it, it is a field on the row
/// rather than a delete (ES-018).
/// </remarks>
public sealed record WithdrawSiteApprovalCommand(SiteApprovalId SiteApprovalId);
