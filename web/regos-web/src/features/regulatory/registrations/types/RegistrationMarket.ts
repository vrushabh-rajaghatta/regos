/**
 * One country something is held in. Navigation, not analytics — the count says
 * whether a market is worth opening, nothing more.
 */
export interface RegistrationMarket {
  countryId: string;
  countryName: string;
  countryCode: string;
  registrationCount: number;
}
