using GymTracker.Models;

namespace GymTracker.Interfaces
{
    public interface IBodyMeasurementRep
    {
        Task<IEnumerable<BodyMeasurements>> GetBodyMeasurementsByUserId(int userId);
        Task<BodyMeasurements> GetBodyMeasurementById(int id);
        Task AddBodyMeasurement(BodyMeasurements bodyMeasurement);
        Task UpdateBodyMeasurement(BodyMeasurements bodyMeasurement);
        Task DeleteBodyMeasurement(int id);
    }
}
