import { BodyMeasurements } from "./body-measurements";
import { Workout } from "./workout.interface";

export interface User {
    id: number,
    email: string,
    fullName: string,
    password: string,
    createdAt: string,
    workouts: Workout[],
    bodyMeasurements: BodyMeasurements[]
}
