export interface Flight {
  date: Date;
  scheduledArrTime: number;
  destAirportCode: string;
  orgAirportCode: string;
  scheduledDepTime: number;
  isDelayedPredicted: boolean;
  isDelayedActual: boolean | null;
}
