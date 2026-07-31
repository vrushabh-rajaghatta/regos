import { expiryPhrase, needsAttention } from "../utils/expiry";

interface Props {
  expiresOn: string | null;
  daysUntilExpiry: number | null;
  /** False once the lifecycle has ended — the countdown has stopped mattering. */
  hasRunningValidity: boolean;
  isExpired: boolean;
}

/**
 * How close an authorisation is to lapsing, in a table cell or a fact list.
 *
 * Renders the server's derived facts and adds only emphasis. What counts as
 * close enough to worry about lives in <c>expiry.ts</c>, deliberately apart from
 * the domain.
 */
export function RegistrationExpiry({
  expiresOn,
  daysUntilExpiry,
  hasRunningValidity,
  isExpired,
}: Props) {
  if (expiresOn === null) {
    return <span className="text-muted-foreground">—</span>;
  }

  const date = new Date(expiresOn).toLocaleDateString();

  // The date is still a fact worth showing, but nothing is counting down to it:
  // a surrendered authorisation keeps the expiry it was granted with.
  if (!hasRunningValidity || daysUntilExpiry === null) {
    return <span className="text-muted-foreground">{date}</span>;
  }

  return (
    <span
      className={
        isExpired
          ? "font-medium text-destructive"
          : needsAttention(daysUntilExpiry)
            ? "font-medium"
            : undefined
      }
      data-testid="registration-expiry"
    >
      {date}
      <span className="ml-2 text-xs text-muted-foreground">
        {expiryPhrase(daysUntilExpiry)}
      </span>
    </span>
  );
}
