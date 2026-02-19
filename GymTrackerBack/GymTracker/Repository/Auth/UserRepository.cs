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
        readonly JwtSerwice _jwtSerwice;

        // ДОБАВЬТЕ ЭТОТ КОНСТРУКТОР
        public UserRepository(WorkoutDbContext workoutDbContext, JwtSerwice jwtSerwice)
        {
            _workoutDbContext = workoutDbContext;
            _jwtSerwice = jwtSerwice;
        }



        public async Task AddUser(SignupRequest user)
        {
            
            if (_workoutDbContext.Users.Any(a => a.FullName == user.Name))
            {
                throw new Exception("Пользователь с таким именем уже существует.");
            }

            if (_workoutDbContext.Users.Any(a => a.Email == user.Email))
            {
                throw new Exception("Пользователь с таким email уже существует.");
            }
            
            var newUser = new User
            {
                FullName = user.Name,
                Email = user.Email,
                CreatedAt = DateTime.UtcNow,
                BodyMeasurements=null,
                Workouts=null,
            };
            var hasher = new PasswordHasher<User>();
            newUser.PasswordHash = hasher.HashPassword(newUser, user.Password);

            await _workoutDbContext.Users.AddAsync(newUser);
            await _workoutDbContext.SaveChangesAsync();
        }

        public void DeleteUser(User user)
        {
             _workoutDbContext.Users.Remove(user);
        }

        public async Task<string> GetByEmail(string email, string passwd)
        {
            var account = await _workoutDbContext.Users.FirstOrDefaultAsync(x => x.Email == email);
            if (account == null)
            {
                throw new Exception("Неверный email или пароль");
            }
            var result = new PasswordHasher<User>().VerifyHashedPassword(account, account.PasswordHash, passwd);
            if (result == PasswordVerificationResult.Success)
            {
                // Теперь _jwtSerwice не будет null
                return _jwtSerwice.GenerateToken(account);
            }
            else
            {
                throw new Exception("Неверный email или пароль");
            }
        }

        public async Task<User?> GetUser(int id)
        {
            return await _workoutDbContext.Users.FirstOrDefaultAsync(x => x.Id == id);
        }

        public void UpdateUser(User user)
        {
            _workoutDbContext.Users.Update(user);
        }
    }
}
