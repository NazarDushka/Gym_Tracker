import { Sets } from "./sets.interface";

export interface Workout {
    id: number;
    date: string;
    notes: string;
    sets: Sets[];
}

