import { Workout } from "./workout.interface";
import { MeasurementLog } from "./body-measurements";

export interface User {
    id: number,
    email: string,
    fullName: string,
    password: string,
    createdAt: string,
    workouts: Workout[],
    bodyMeasurementLogs: MeasurementLog[]
}
