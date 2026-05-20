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
            try
            {
                await _unitOfWork.User.AddUser(user);
                return Ok(new { message = "Registration complete" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(string email, string password)
        {
            try
            {
                var token = await _unitOfWork.User.GetByEmail(email, password);
                return Ok(new { token });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

    }
}
