using RegOS.ProductDocument.Domain.Repositories;
using RegOS.SharedKernel.Exceptions;

namespace RegOS.ProductDocument.Application.Commands.ArchiveProductDocument;

public sealed class ArchiveProductDocumentHandler
{
    private readonly IProductDocumentRepository _repository;

    public ArchiveProductDocumentHandler(IProductDocumentRepository repository)
    {
        _repository = repository;
    }

    public async Task HandleAsync(
        ArchiveProductDocumentCommand command,
        CancellationToken cancellationToken)
    {
        var document = await _repository.GetByIdAsync(
            command.DocumentId,
            cancellationToken);

        // Not found — or found, but under a different product than the route
        // claims. Either way the addressed resource does not exist here.
        if (document is null || document.ProductId != command.ProductId)
            throw new NotFoundException(
                ProductDocumentLifecycleErrors.DocumentDoesNotExist);

        // Invalid transitions are enforced by the aggregate; they surface as
        // InvalidOperationException and map to 409 at the endpoint.
        document.Archive();

        await _repository.UpdateAsync(document, cancellationToken);
    }
}
