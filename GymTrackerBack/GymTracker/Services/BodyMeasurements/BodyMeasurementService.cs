using GymTracker.DTOs.BodyMeasurements;
using GymTracker.Interfaces;
using GymTracker.Models;

namespace GymTracker.Services.BodyMeasurements
{
    public class BodyMeasurementService : IBodyMeasurementsService
    {
        private readonly IUnitOfWork _unitOfWork;
        public BodyMeasurementService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<MeasurementTypeDto>> GetMeasurementTypesAsync()
        {
            var types = await _unitOfWork.Measurements.GetAllTypesAsync();
            if (types == null || !types.Any())
            {
                throw new KeyNotFoundException("No measurement types found");
            }
            return types.Select(t => new MeasurementTypeDto
            {
                Name = t.Name,
                Unit = t.Unit
            });
        }

        public async Task<IEnumerable<MeasurementLogDto>> GetMeasurementLogsAsync(int userId)
        {
            var logs = await _unitOfWork.Measurements.GetLogsByUserIdAsync(userId);
            if (logs == null || !logs.Any())
            {
                throw new KeyNotFoundException("No measurement logs found");
            }
            return logs.Select(MapLogToDto);
        }

        public async Task<IEnumerable<MeasurementTargetDto>> GetActiveMeasurementTargetsAsync(int userId)
        {
            var targets = await _unitOfWork.Measurements.GetActiveTargetsByUserIdAsync(userId);
            if (targets == null || !targets.Any())
            {
                throw new KeyNotFoundException("No measurement targets found");
            }
            return targets.Select(MapTargetToDto);
        }

        public async Task<MeasurementTargetDto> CreateMeasurementTargetAsync(int userId, CreateTargetRequest request)
        {
                var target = new MeasurementTarget
                {
                    UserId = userId,
                    MeasurementTypeId = request.MeasurementTypeId,
                    TargetValue = request.TargetValue,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };
                await _unitOfWork.Measurements.AddTargetAsync(target);
                await _unitOfWork.CompleteAsync();
                return MapTargetToDto(target);
        }

        public async Task<MeasurementLogDto> CreateMeasurementLogAsync(int userId, CreateMeasurementLogRequest request)
        {
                var log = new MeasurementLog
                {
                    UserId = userId,
                    MeasurementTypeId = request.MeasurementTypeId,
                    Value = request.Value,
                    Date = request.Date
                };
                await _unitOfWork.Measurements.AddLogAsync(log);
                await _unitOfWork.CompleteAsync();
                return MapLogToDto(log);
            
        }

        public async Task DeleteMeasurementLogAsync(int userId, Guid logId)
        {
            var log = await _unitOfWork.Measurements.GetLogByIdAsync(logId);
            if (log == null || log.UserId != userId)
            {
                throw new KeyNotFoundException("Measurement log not found or does not belong to the user");
            }
            await _unitOfWork.Measurements.DeleteLogAsync(logId);
            await _unitOfWork.CompleteAsync();
        }

        public async Task<MeasurementLogDto> UpdateMeasurementLogAsync(int userId, Guid logId, CreateMeasurementLogRequest request)
        {
            var log = await _unitOfWork.Measurements.GetLogByIdAsync(logId);
            if (log == null)
            {
                throw new KeyNotFoundException("Measurement log not found");
            }
            log.Value = request.Value;
            log.Date = request.Date;
            await _unitOfWork.CompleteAsync();
            return MapLogToDto(log);
        }

        public async Task DeactivateTargetAsync(int userId, int targetId)
        {
            var target = await _unitOfWork.Measurements.GetTargetByIdAsync(targetId);
            if (target == null || target.UserId != userId)
            {
                throw new KeyNotFoundException("Measurement target not found or does not belong to the user");
            }
            await _unitOfWork.Measurements.DeactivateTargetAsync(target.Id, target);
            await _unitOfWork.CompleteAsync();
        }

         

        //Will implement this method later, for now it throws a NotImplementedException
        public async Task<MeasurementTargetDto> UpdateMeasurementTargetAsync(int userId, CreateTargetRequest request)
        {
           throw new NotImplementedException();
        }



        private MeasurementLogDto MapLogToDto(MeasurementLog log)
        {
            return new MeasurementLogDto
            {
                MeasurementTypeId = log.MeasurementTypeId,
                Value = log.Value,
                Date = log.Date
            };
        }
        private MeasurementTargetDto MapTargetToDto(MeasurementTarget target)
        {
            return new MeasurementTargetDto
            {
                MeasurementTypeId = target.MeasurementTypeId,
                TargetValue = target.TargetValue,
                IsActive = target.IsActive,
                CreatedAt = target.CreatedAt
            };
        }
    }
}