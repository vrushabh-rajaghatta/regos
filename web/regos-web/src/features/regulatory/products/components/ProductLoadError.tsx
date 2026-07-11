import { Button } from "@/components/ui/button";

interface ProductLoadErrorProps {
  retry(): void;
}

export function ProductLoadError({ retry }: ProductLoadErrorProps) {
  return (
    <div className="space-y-4">
      <p>Unable to load products.</p>

      <Button onClick={retry}>Try Again</Button>
    </div>
  );
}
