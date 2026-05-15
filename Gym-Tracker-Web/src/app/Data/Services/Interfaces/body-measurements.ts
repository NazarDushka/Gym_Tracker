import { User } from "./user";

export interface MeasurementType {
  Id: number;
  Name: string;
  Unit: string;
}

export interface MeasurementLog {
  UserId: number;
  MeasurementTypeId: number;
  Value: number;
  Date: Date | string;
  Type?: MeasurementType; 
}