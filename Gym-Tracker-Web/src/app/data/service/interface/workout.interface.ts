import { Sets } from "./sets.interface";

export interface Workout {
    id: number;
    userId: number;
    date: string;
    notes: string;
    sets: Sets[];
}

