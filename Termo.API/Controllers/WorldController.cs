using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Termo.API.BackgroundServices;
using Termo.Models;
using Termo.Models.ViewModels;

namespace Termo.API.Controllers
{

    [ApiController]
    [Route("[controller]")]
    [Authorize]
    public class WorldController : ControllerBase {

        private readonly IWorldService _worldService;
        private readonly IStatisticsService _statisticsService;
        private readonly IBackgroundTaskQueue _backgroundTaskQueue;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public WorldController(IWorldService worldService, IStatisticsService statisticsService, IBackgroundTaskQueue backgroundTaskQueue, IServiceScopeFactory serviceScopeFactory) {
            _worldService = worldService;
            _statisticsService = statisticsService;
            _backgroundTaskQueue = backgroundTaskQueue;
            _serviceScopeFactory = serviceScopeFactory;
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
        public async Task<IActionResult> ValidateWorld([FromBody] ValidateWorldViewModel validateWorldViewModel) {

            if(validateWorldViewModel.WorldReceived.Length != 5) {
                return BadRequest(new {
                    Key = "WORLD_MUST_BE_FIVE_CARACTERS",
                    Message = "A palavra precisa ter 5 caracteres"
                });
            }

            if(!(await _worldService.VerifyIdWorldExists(validateWorldViewModel.WorldReceived))) {
                ProccessNewWorld(validateWorldViewModel.WorldReceived);

                return BadRequest(new {
                    Key = "WORLD_DOES_NOT_EXISTS",
                    Message = $"A palavra: {validateWorldViewModel.WorldReceived} nao existe no nosso banco de dados"
                });
            }

            if(!(await _worldService.CanPlayerPlay(validateWorldViewModel.IpAdress))){
                return BadRequest(new {
                    Key = "PLAYER_NOT_CAN_PLAY",
                    Message = $"O Jogador com o IP: {validateWorldViewModel.IpAdress} ja alcançou o numero maximo de tentativas"
                });
            }

            return Ok(await _worldService.ValidateWorld(validateWorldViewModel));
        }

        private void ProccessNewWorld(string world)
        {
            _backgroundTaskQueue.QueueBackgroundWorkItem(async token =>
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var worldService = scope.ServiceProvider.GetService<IWorldService>();
                await worldService.GenerateWorldIfIsValid(world);
            });
        }
    }
}
