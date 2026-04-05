using GymTracker.Models;
using GymTracker.Repository.Auth;

namespace GymTracker.Interfaces
{
    public interface IUser
    {
        Task<string> GetByEmail(string email, string passwd);
        Task<User> GetUser(int id);
        Task AddUser(SignupRequest user);
        Task UpdateUser(User user);

        Task DeleteUser(User user);
    }
}
