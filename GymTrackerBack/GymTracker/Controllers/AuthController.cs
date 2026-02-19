using GymTracker.Interfaces;
using GymTracker.Models;
using GymTracker.Repository.Auth;
using GymTracker.Repository.UnitOfWork;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace GymTracker.Controllers
{
    [EnableCors("AllowAll")]
    [ApiController]
    [Route("[Controller]")]
    public class AuthController: ControllerBase
    {
        readonly IUnitOfWork _unitOfWork;

       public AuthController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(SignupRequest user)
        {
           
                await _unitOfWork.User.AddUser(user);
                return Ok(new { message = "Registration complete" });
           
           
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(string email,string passwd)
        {
            
                var token = await _unitOfWork.User.GetByEmail(email, passwd);
                return Ok(new { token });
        }

    }
}
