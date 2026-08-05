using RegOS.Registration.Domain.Aggregates.SiteApprovals;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.Registration.Application.Commands.WithdrawSiteApproval;

public sealed class WithdrawSiteApprovalHandler
{
    private readonly ISiteApprovalRepository _approvals;

    public WithdrawSiteApprovalHandler(ISiteApprovalRepository approvals)
    {
        _approvals = approvals;
    }

    public async Task HandleAsync(
        WithdrawSiteApprovalCommand command,
        CancellationToken cancellationToken)
    {
        var approval = await _approvals.GetByIdAsync(
                command.SiteApprovalId, cancellationToken)
            ?? throw new NotFoundException(SiteApprovalErrors.NotFound);

        await _approvals.RemoveAsync(approval, cancellationToken);
    }
}
