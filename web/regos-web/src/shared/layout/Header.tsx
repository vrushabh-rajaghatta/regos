export function Header() {
  return (
    <header className="h-14 border-b flex items-center justify-between px-6 bg-background">
      <h1 className="text-lg font-semibold">RegOS</h1>

      <div className="text-sm text-muted-foreground">User</div>
    </header>
  );
}
