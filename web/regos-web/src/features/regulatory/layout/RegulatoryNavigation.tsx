const items = ["Products", "Submissions", "Authorities", "Templates"];

export function RegulatoryNavigation() {
  return (
    <nav className="w-60 border-r p-3">
      {items.map((item) => (
        <button
          key={item}
          className="block w-full rounded-md px-3 py-2 text-left hover:bg-muted"
        >
          {item}
        </button>
      ))}
    </nav>
  );
}
