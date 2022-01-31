using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Termo.API.Services;

namespace Termo.API.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase {

        [HttpGet]
        [Route("authorize/{password}")]
        [AllowAnonymous]
        public IActionResult Authorize(string password) {
            if(!password.Equals("palmeiras_nao_tem_mundial"))
                return Unauthorized("Senha errada");

            var token = TokenService.GenerateToken();
            return Ok(token);
        }

    }
}
