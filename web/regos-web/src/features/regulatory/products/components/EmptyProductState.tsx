export function EmptyProductState() {
  return (
    <div className="flex flex-col items-center justify-center py-16 text-center">
      <h2 className="text-xl font-semibold">No products found</h2>

      <p className="mt-2 text-muted-foreground">
        Register your first product to get started.
      </p>
    </div>
  );
}
