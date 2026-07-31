import { Button } from "@/components/ui/button";

import { languageName } from "../constants/languages";
import { useRemoveTradeName } from "../hooks/useRemoveTradeName";
import type { TradeName } from "../types/MedicinalProduct";

interface MarketTradeNamesProps {
  medicinalProductId: string;
  tradeNames: TradeName[];
}

/**
 * What the product is called here, one name per language.
 *
 * Removing is how a name is corrected: there is no rename, because without
 * effective dating a rename is indistinguishable from remove-then-add and
 * offering one would imply a history the model does not keep.
 */
export function MarketTradeNames({
  medicinalProductId,
  tradeNames,
}: MarketTradeNamesProps) {
  const mutation = useRemoveTradeName(medicinalProductId);

  if (tradeNames.length === 0) {
    return (
      <span className="text-muted-foreground" data-testid="market-unnamed">
        Not named yet
      </span>
    );
  }

  return (
    <ul className="space-y-1">
      {tradeNames.map((tradeName) => (
        <li
          key={tradeName.tradeNameId}
          className="flex items-center gap-2"
          data-testid="market-trade-name"
        >
          <span className="font-medium">{tradeName.name}</span>
          <span className="text-xs text-muted-foreground">
            {languageName(tradeName.language)}
          </span>

          <Button
            variant="ghost"
            size="sm"
            disabled={mutation.isPending}
            aria-label={`Remove ${tradeName.name}`}
            onClick={() => mutation.mutate(tradeName.tradeNameId)}
          >
            Remove
          </Button>
        </li>
      ))}
    </ul>
  );
}
