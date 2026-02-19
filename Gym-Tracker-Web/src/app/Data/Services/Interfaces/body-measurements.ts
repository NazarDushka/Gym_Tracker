import { User } from "./user";

export interface BodyMeasurements {
    Id: number;
    Weight: number;
    Chest?: number;
    Waist?: number;
    Biceps?:number;
    UserId?:number;
    User:User;
}
