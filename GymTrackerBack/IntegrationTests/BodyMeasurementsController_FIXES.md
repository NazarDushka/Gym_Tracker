# Рекомендуемые исправления для BodyMeasurementsController

## Исправление 1??: Убрать дублирование SaveChangesAsync()

### Вариант A: Оставить SaveChangesAsync в Repository (проще)

**BodyMeasurementsRepository.cs - Метод AddLogAsync:**
```csharp
// БЫЛО:
public async Task AddLogAsync(MeasurementLog log)
{
    await _workoutDbContext.MeasurementLogs.AddAsync(log);
    await _workoutDbContext.SaveChangesAsync();
}

// СТАЛО:
public async Task AddLogAsync(MeasurementLog log)
{
    await _workoutDbContext.MeasurementLogs.AddAsync(log);
    // Убрать SaveChangesAsync() - пусть UnitOfWork справляется
}
```

**Аналогично для:**
- `AddTargetAsync()` - убрать `SaveChangesAsync()`
- `DeleteLogAsync()` - убрать `SaveChangesAsync()`
- `DeactivateTargetAsync()` - убрать `SaveChangesAsync()`

### Результат:
```csharp
// BodyMeasurementsController.cs
await _unitOfWork.Measurements.AddLogAsync(log);
await _unitOfWork.CompleteAsync();  // Одно сохранение вместо двух!
```

---

## Исправление 2??: Добавить [Authorize] и проверку пользователя

**BodyMeasurementsController.cs:**

```csharp
using Microsoft.AspNetCore.Authorization;

[Route("GymTracker/[controller]")]
[ApiController]
[Authorize]  // ? Добавить
public class MeasurementsController : ControllerBase
{
    private readonly IUnitOfWork _unitOfWork;
    
    public MeasurementsController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    // GET: api/measurements/types - может быть публичным
    [AllowAnonymous]
    [HttpGet("types")]
    public async Task<ActionResult<IEnumerable<MeasurementType>>> GetMeasurementTypes()
    {
        var types = await _unitOfWork.Measurements.GetAllTypesAsync();
        return Ok(types);
    }

    // GET: api/users/{userId}/measurements
    [HttpGet("~/api/users/{userId}/measurements")]
    public async Task<ActionResult<IEnumerable<MeasurementLog>>> GetUserLogs(int userId) 
    {
        // ? Проверка существования пользователя
        var user = await _unitOfWork.User.GetUser(userId);
        if (user == null)
            return NotFound(new { message = "Пользователь не найден" });

        var logs = await _unitOfWork.Measurements.GetLogsByUserIdAsync(userId);
        return Ok(logs);
    }

    // POST: api/measurements
    [HttpPost]
    public async Task<ActionResult> AddMeasurementLog([FromBody] MeasurementLog log)
    {
        // ? Проверка валидности
        if (log == null)
            return BadRequest(new { message = "Данные логирования не могут быть пустыми" });
        
        if (log.UserId <= 0)
            return BadRequest(new { message = "UserId должен быть больше 0" });
        
        if (log.MeasurementTypeId <= 0)
            return BadRequest(new { message = "MeasurementTypeId должен быть больше 0" });
        
        if (log.Value < 0)
            return BadRequest(new { message = "Value не может быть отрицательным" });

        // ? Проверка существования пользователя
        var user = await _unitOfWork.User.GetUser(log.UserId);
        if (user == null)
            return NotFound(new { message = "Пользователь не найден" });

        // ? Проверка существования типа измерения
        var types = await _unitOfWork.Measurements.GetAllTypesAsync();
        if (!types.Any(t => t.Id == log.MeasurementTypeId))
            return BadRequest(new { message = "Тип измерения не найден" });

        if (log.Date == default)
        {
            log.Date = DateTime.UtcNow;
        }

        log.MeasurementType = null;

        try
        {
            await _unitOfWork.Measurements.AddLogAsync(log);
            await _unitOfWork.CompleteAsync();
            return StatusCode(StatusCodes.Status201Created, log);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Ошибка при создании логирования", details = ex.Message });
        }
    }

    // DELETE: api/measurements/{id}
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteMeasurementLog(int id) 
    {
        try
        {
            var log = await _unitOfWork.Measurements.GetLogByIdAsync(id);
            if (log == null) 
                return NotFound(new { message = "Логирование не найдено" });

            // ? Используем найденный объект вместо повторного поиска
            await _unitOfWork.Measurements.DeleteLogAsync(id);
            await _unitOfWork.CompleteAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Ошибка при удалении логирования", details = ex.Message });
        }
    }

    // GET: api/users/{userId}/targets
    [HttpGet("~/api/users/{userId}/targets")]
    public async Task<ActionResult<IEnumerable<MeasurementTarget>>> GetUserTargets(int userId) 
    {
        // ? Проверка существования пользователя
        var user = await _unitOfWork.User.GetUser(userId);
        if (user == null)
            return NotFound(new { message = "Пользователь не найден" });

        var targets = await _unitOfWork.Measurements.GetActiveTargetsByUserIdAsync(userId);
        return Ok(targets);
    }

    // POST: api/targets
    [HttpPost("targets")]
    public async Task<ActionResult> AddTarget([FromBody] MeasurementTarget target)
    {
        // ? Валидация входных данных
        if (target == null)
            return BadRequest(new { message = "Целевое значение не может быть пустым" });
        
        if (target.UserId <= 0)
            return BadRequest(new { message = "UserId должен быть больше 0" });
        
        if (target.MeasurementTypeId <= 0)
            return BadRequest(new { message = "MeasurementTypeId должен быть больше 0" });
        
        if (target.TargetValue < 0)
            return BadRequest(new { message = "TargetValue не может быть отрицательным" });

        // ? Проверка существования пользователя
        var user = await _unitOfWork.User.GetUser(target.UserId);
        if (user == null)
            return NotFound(new { message = "Пользователь не найден" });

        try
        {
            var activeTargets = await _unitOfWork.Measurements.GetActiveTargetsByUserIdAsync(target.UserId);
            var oldTarget = activeTargets.FirstOrDefault(t => t.MeasurementTypeId == target.MeasurementTypeId);

            if (oldTarget != null)
            {
                await _unitOfWork.Measurements.DeactivateTargetAsync(oldTarget.Id);
            }

            target.CreatedAt = DateTime.UtcNow;
            target.IsActive = true;
            target.MeasurementType = null;

            await _unitOfWork.Measurements.AddTargetAsync(target);
            await _unitOfWork.CompleteAsync();

            return StatusCode(StatusCodes.Status201Created, target);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Ошибка при создании целевого значения", details = ex.Message });
        }
    }

    // DELETE (Deactivate): api/targets/{id}
    [HttpDelete("targets/{id}")]
    public async Task<ActionResult> DeactivateTarget(int id) 
    {
        try
        {
            await _unitOfWork.Measurements.DeactivateTargetAsync(id);
            await _unitOfWork.CompleteAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Ошибка при деактивации целевого значения", details = ex.Message });
        }
    }
}
```

---

## Исправление 3??: Исправить Repository для удаления SaveChangesAsync()

**BodyMeasurementsRepository.cs:**

```csharp
public async Task AddLogAsync(MeasurementLog log)
{
    await _workoutDbContext.MeasurementLogs.AddAsync(log);
    // Убрать: await _workoutDbContext.SaveChangesAsync();
}

public async Task DeleteLogAsync(int logId)
{
    var log = await _workoutDbContext.MeasurementLogs.FindAsync(logId);
    if (log != null)
    {
        _workoutDbContext.MeasurementLogs.Remove(log);
        // Убрать: await _workoutDbContext.SaveChangesAsync();
    }
}

public async Task AddTargetAsync(MeasurementTarget target)
{
    await _workoutDbContext.MeasurementTargets.AddAsync(target);
    // Убрать: await _workoutDbContext.SaveChangesAsync();
}

public async Task DeactivateTargetAsync(int targetId)
{
    var target = await _workoutDbContext.MeasurementTargets.FindAsync(targetId);
    if (target != null)
    {
        target.IsActive = false;
        _workoutDbContext.MeasurementTargets.Update(target);
        // Убрать: await _workoutDbContext.SaveChangesAsync();
    }
}
```

---

## Исправление 4??: Унифицировать маршрутизацию

**ВАРИАНТ A: Использовать класс-уровень маршрут везде**

```csharp
[Route("GymTracker/[controller]")]
[ApiController]
[Authorize]
public class MeasurementsController : ControllerBase
{
    // GET: /GymTracker/Measurements/types
    [AllowAnonymous]
    [HttpGet("types")]
    public async Task<ActionResult<IEnumerable<MeasurementType>>> GetMeasurementTypes() { }

    // GET: /GymTracker/Measurements/user/{userId}/logs
    [HttpGet("user/{userId}/logs")]
    public async Task<ActionResult<IEnumerable<MeasurementLog>>> GetUserLogs(int userId) { }

    // POST: /GymTracker/Measurements
    [HttpPost]
    public async Task<ActionResult> AddMeasurementLog([FromBody] MeasurementLog log) { }

    // DELETE: /GymTracker/Measurements/{id}
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteMeasurementLog(int id) { }

    // GET: /GymTracker/Measurements/user/{userId}/targets
    [HttpGet("user/{userId}/targets")]
    public async Task<ActionResult<IEnumerable<MeasurementTarget>>> GetUserTargets(int userId) { }

    // POST: /GymTracker/Measurements/targets
    [HttpPost("targets")]
    public async Task<ActionResult> AddTarget([FromBody] MeasurementTarget target) { }

    // DELETE: /GymTracker/Measurements/targets/{id}
    [HttpDelete("targets/{id}")]
    public async Task<ActionResult> DeactivateTarget(int id) { }
}
```

---

## РЕЗЮМЕ КРИТИЧЕСКИХ ИЗМЕНЕНИЙ

| Проблема | Исправление | Файл | Приоритет |
|----------|-------------|------|-----------|
| Дублирование SaveChangesAsync() | Убрать из Repository | BodyMeasurementsRepository.cs | ?? |
| Отсутствие авторизации | Добавить [Authorize] | BodyMeasurementsController.cs | ?? |
| Отсутствие валидации пользователя | Проверить существование User | BodyMeasurementsController.cs | ?? |
| Отсутствие валидации данных | Добавить проверки в методы | BodyMeasurementsController.cs | ?? |
| Отсутствие обработки ошибок | Добавить try-catch блоки | BodyMeasurementsController.cs | ?? |
| Несогласованность маршрутов | Унифицировать маршруты | BodyMeasurementsController.cs | ?? |

