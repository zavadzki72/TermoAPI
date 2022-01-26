using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Termo.API.Controllers {

    [ApiController]
    [Route("[controller]")]
    public class WorldController : ControllerBase {

        private readonly IWorldService _worldService;
        private readonly IStatisticsService _statisticsService;

        public WorldController(IWorldService worldService, IStatisticsService statisticsService) {
            _worldService = worldService;
            _statisticsService = statisticsService;
        }

        [HttpGet]
        [Route("GetWorld")]
        public async Task<IActionResult> GetWorld() {
            return Ok(await _worldService.GetWorld());
        }

        [HttpGet]
        [Route("GetPlayerTodayProgress")]
        public async Task<IActionResult> GetPlayerTodayProgress(string ipAdress) {

            var response = await _worldService.GetTriesTodayPlyer(ipAdress);

            return response == null ? NoContent() : Ok(response);
        }

        [HttpGet]
        [Route("GetStatistics")]
        public async Task<IActionResult> GetStatistics(string ipAdress) {
            return Ok(await _statisticsService.GetPlayerStatistic(ipAdress));
        }

        [HttpPost]
        [Route("ValidateWorld")]
        public async Task<IActionResult> ValidateWorld(string worldReceived, string ipAdress, string playerName) {

            if(worldReceived.Length != 5) {
                return BadRequest(new {
                    Key = "WORLD_MUST_BE_FIVE_CARACTERS",
                    Message = "A palavra precisa ter 5 caracteres"
                });
            }

            if(!(await _worldService.VerifyIdWorldExists(worldReceived))) {
                return BadRequest(new {
                    Key = "WORLD_DOES_NOT_EXISTS",
                    Message = $"A palavra: {worldReceived} nao existe no nosso banco de dados"
                });
            }

            if(!(await _worldService.CanPlayerPlay(ipAdress, playerName))){
                return BadRequest(new {
                    Key = "PLAYER_NOT_CAN_PLAY",
                    Message = $"O Jogador com o IP: {ipAdress} ja alcançou o numero maximo de tentativas"
                });
            }



            return Ok(await _worldService.ValidateWorld(worldReceived, ipAdress, playerName));
        }
    }
}
