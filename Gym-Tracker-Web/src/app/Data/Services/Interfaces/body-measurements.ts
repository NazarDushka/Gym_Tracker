import { User } from "./user";

export interface MeasurementType {
  id: string;
  name: string;
  unit: string;
}

export interface MeasurementLog {
  userId: number;
  measurementTypeId: string;
  value: number;
  date: Date | string;
  measurementType?: MeasurementType; 
}