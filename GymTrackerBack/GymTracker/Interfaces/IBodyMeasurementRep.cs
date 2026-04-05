using GymTracker.Models;
using System.Threading.Tasks;

namespace GymTracker.Interfaces
{
    public interface IMeasurementRepository
    {
        Task<IEnumerable<MeasurementType>> GetAllTypesAsync();

        Task<IEnumerable<MeasurementLog>> GetLogsByUserIdAsync(int userId);

        Task<IEnumerable<MeasurementLog>> GetLogsByTypeAsync(int userId, int typeId);

        Task<MeasurementLog?> GetLogByIdAsync(int logId);
        Task AddLogAsync(MeasurementLog log);
        Task DeleteLogAsync(int logId);

        Task<IEnumerable<MeasurementTarget>> GetActiveTargetsByUserIdAsync(int userId);
        Task AddTargetAsync(MeasurementTarget target);
        Task DeactivateTargetAsync(int targetId);
    }
}
