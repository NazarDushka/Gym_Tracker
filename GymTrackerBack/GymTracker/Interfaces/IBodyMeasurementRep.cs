using GymTracker.Models;
using System.Threading.Tasks;

namespace GymTracker.Interfaces
{
    public interface IMeasurementRepository
    {
        Task<IEnumerable<MeasurementType>> GetAllTypesAsync();

        Task<IEnumerable<MeasurementLog>> GetLogsByUserIdAsync(int userId);

        Task<IEnumerable<MeasurementLog>> GetLogsByTypeAsync(int userId, Guid typeId);

        Task<IEnumerable<MeasurementLog>> GetLastLogsForUserAsync(int userId);

        Task<MeasurementLog?> GetLogByIdAsync(Guid logId);
        Task AddLogAsync(MeasurementLog log);
        Task DeleteLogAsync(Guid logId);

        Task<IEnumerable<MeasurementTarget>> GetActiveTargetsByUserIdAsync(int userId);
        Task<MeasurementTarget?> GetTargetByIdAsync(int targetId);
        Task AddTargetAsync(MeasurementTarget target);
        Task DeactivateTargetAsync(int targetId, MeasurementTarget target);
    }
}
