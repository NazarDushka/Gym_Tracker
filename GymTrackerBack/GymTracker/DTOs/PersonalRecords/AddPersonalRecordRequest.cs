namespace GymTracker.DTOs.PersonalRecords
{
    public class AddPersonalRecordRequest
    {
        public int ExerciseId { get; set; }
        public int Reps { get; set; }
        public float Weight { get; set; }
        public int WorkoutSetId { get; set; }
    }

    public class UpdatePersonalRecordRequest
    {
        public int ExerciseId { get; set; }
        public int Reps { get; set; }
        public float Weight { get; set; }
        public int WorkoutSetId { get; set; }
    }

    
}
