using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Termo.Models.Interfaces;

namespace Termo.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class TermostatoController : ControllerBase
    {

        private readonly ITermostatoService _termostatoService;

        public TermostatoController(ITermostatoService termostatoService)
        {
            _termostatoService = termostatoService;
        }

        [HttpGet]
        [Route("GetTodayTermostato")]
        public async Task<IActionResult> GetTodayTermostato()
        {
            var response = await _termostatoService.GetTodayTermostato();

            return Ok(response);
        }

    }
}
