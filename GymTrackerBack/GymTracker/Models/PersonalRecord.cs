using GymTracker.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;

namespace GymTracker.Models
{
    [Table("PersonalRecords")]
    public class PersonalRecord
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int WorkoutSetId { get; set; }

        public float CalculatedMaxLift { get; set; }
        public int Reps { get; set; }
        public float Weight { get; set; }
        public int UserId { get; set; }
        public string ExerciseName { get; set; }
        public int ExerciseId { get; set; }
        public DateTime AchievedDate { get; set; }

        
    }
}

