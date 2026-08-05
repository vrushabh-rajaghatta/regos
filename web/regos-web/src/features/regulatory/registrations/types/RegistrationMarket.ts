/**
 * One country something is held in. Navigation, not analytics — the count says
 * whether a market is worth opening, nothing more.
 */
export interface RegistrationMarket {
  countryId: string;
  countryName: string;
  countryCode: string;
  registrationCount: number;
  /**
   * The regulatory groupings this market belongs to — EU, ICH, PIC/S. They
   * overlap, and **empty is a recorded answer**: India belongs to none of the
   * five RegOS records.
   */
  regions: string[];
}
