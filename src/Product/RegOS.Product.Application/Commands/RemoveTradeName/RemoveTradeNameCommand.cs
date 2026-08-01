using RegOS.Product.Domain.Product;

namespace RegOS.Product.Application.Commands.RemoveTradeName;

public sealed record RemoveTradeNameCommand(
    MedicinalProductId MedicinalProductId,
    TradeNameId TradeNameId);
