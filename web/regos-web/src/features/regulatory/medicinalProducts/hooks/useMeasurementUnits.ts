import { useQuery } from "@tanstack/react-query";

import { listMeasurementUnits } from "../api/listMeasurementUnits";

export function useMeasurementUnits() {
  return useQuery({
    queryKey: ["measurement-units"],
    queryFn: listMeasurementUnits,

    // It changes when the platform ships a new vocabulary, not while a form is
    // open.
    staleTime: Infinity,
  });
}
