using GymTracker.Interfaces;
using GymTracker.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GymTracker.Repository.Auth
{
    public class UserRepository : IUser
    {
        private readonly WorkoutDbContext _workoutDbContext;
        readonly JwtService _jwtService;

        public UserRepository(WorkoutDbContext workoutDbContext, JwtService jwtService)
        {
            _workoutDbContext = workoutDbContext;
            _jwtService = jwtService;
        }



        public async Task AddUser(SignupRequest user)
        {
            // Валидация входных данных
            if (string.IsNullOrWhiteSpace(user?.Name))
                throw new Exception("Имя пользователя не может быть пустым.");
            
            if (string.IsNullOrWhiteSpace(user?.Email))
                throw new Exception("Email не может быть пустым.");
            
            if (string.IsNullOrWhiteSpace(user?.Password))
                throw new Exception("Пароль не может быть пустым.");

            // Нормализация email
            var normalizedEmail = user.Email.Trim().ToLower();
            
            // Объединенная проверка (1 запрос вместо 2)
            if (_workoutDbContext.Users.Any(a => a.FullName == user.Name || a.Email == normalizedEmail))
            {
                throw new Exception("Пользователь с таким именем или email уже существует.");
            }
            
            var newUser = new User
            {
                FullName = user.Name,
                Email = normalizedEmail,
                CreatedAt = DateTime.UtcNow,
              //  BodyMeasurements=null,
                Workouts=null,
            };
            var hasher = new PasswordHasher<User>();
            newUser.PasswordHash = hasher.HashPassword(newUser, user.Password);

            await _workoutDbContext.Users.AddAsync(newUser);
            await _workoutDbContext.SaveChangesAsync();
        }

        public async Task DeleteUser(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            
             _workoutDbContext.Users.Remove(user);
             await _workoutDbContext.SaveChangesAsync();
        }

        public async Task<string> GetByEmail(string email, string passwd)
        {
            // Валидация входных данных
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email не может быть пустым.", nameof(email));
            
            if (string.IsNullOrWhiteSpace(passwd))
                throw new ArgumentException("Пароль не может быть пустым.", nameof(passwd));
            
            var normalizedEmail = email.Trim().ToLower();
            var account = await _workoutDbContext.Users.FirstOrDefaultAsync(x => x.Email == normalizedEmail);
            if (account == null)
            {
                throw new Exception("Неверный email или пароль");
            }
            var result = new PasswordHasher<User>().VerifyHashedPassword(account, account.PasswordHash, passwd);
            if (result == PasswordVerificationResult.Success)
            {
                return _jwtService.GenerateToken(account);
            }
            else
            {
                throw new Exception("Неверный email или пароль");
            }
        }

        public async Task<User> GetUser(int id)
        {
            return await _workoutDbContext.Users.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task UpdateUser(User user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            
            _workoutDbContext.Users.Update(user);
            await _workoutDbContext.SaveChangesAsync();
        }
    }
}
