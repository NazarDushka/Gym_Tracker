using GymTracker.DTOs.BodyMeasurements;
namespace GymTracker.Services.BodyMeasurements
{
    public interface IBodyMeasurementsService
    {
        Task<IEnumerable<MeasurementTypeDto>> GetMeasurementTypesAsync();
        Task<IEnumerable<MeasurementLogDto>> GetMeasurementLogsAsync(int userId);
        Task<IEnumerable<MeasurementLogDto>> GetLastUsersMeasurementLogsAsync(int userId);
        Task<IEnumerable<MeasurementTargetDto>> GetActiveMeasurementTargetsAsync(int userId);
         Task<MeasurementTargetDto> CreateMeasurementTargetAsync(int userId, CreateTargetRequest request);
         Task<MeasurementTargetDto> UpdateMeasurementTargetAsync(int userId, CreateTargetRequest request);
         Task<MeasurementLogDto> CreateMeasurementLogAsync(int userId, CreateMeasurementLogRequest request);
         Task<MeasurementLogDto> UpdateMeasurementLogAsync(int userId, Guid logId, CreateMeasurementLogRequest request);
         Task DeleteMeasurementLogAsync(int userId, Guid measurementLogId);
         Task DeactivateTargetAsync(int userId, Guid measurementId);
         Task DeactivateTargetAsync(int userId, int targetId);
    }
}
