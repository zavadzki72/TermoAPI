using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Termo.API.Database;
using Termo.API.Entities;
using Termo.API.ExternalServices;
using Termo.API.Models;

namespace Termo.API {
    public class WorldService : IWorldService {

        private const string WORLD_OF_DAY_CACHEKEY = "WORLD_OF_DAY";
        private const int NUMBER_MAX_TRIES = 6;
        private string WORLD_TO_DISCOVERY;

        private readonly ApplicationDbContext _dbContext;
        private readonly IMemoryCache _memoryCache;
        private readonly IDictionaryService _dictionaryService;

        public WorldService(ApplicationDbContext dbContext, IMemoryCache memoryCache, IDictionaryService dictionaryService) {
            _dbContext = dbContext;
            _memoryCache = memoryCache;
            _dictionaryService = dictionaryService;
        }

        #region GetWorld
        private async Task<string> GetWorld() {

            string world;

            if (_memoryCache.TryGetValue(WORLD_OF_DAY_CACHEKEY, out string worldJson)) {

                var cahceWorld = JsonConvert.DeserializeObject<WorldEntity>(worldJson);

                if(ValidateWorldIsValid(cahceWorld)) {
                    return cahceWorld.Name;
                }

                world = await GetNewWorld(cahceWorld);

                return world;
            }

            var actualWorld = await _dbContext.Worlds.FirstOrDefaultAsync(x => x.WorldStatus.Equals(WorldStatusEnumerator.USING));

            if(actualWorld != null && ValidateWorldIsValid(actualWorld)) {
                GenerateWorldCache(actualWorld);
                return actualWorld.Name;
            }

            world = await GetNewWorld(actualWorld);

            return world;
        }

        private async Task<string> GetNewWorld(WorldEntity actualWorld) {
            var world = await _dbContext.Worlds.Where(x => x.WorldStatus.Equals(WorldStatusEnumerator.WATING)).OrderBy(r => Guid.NewGuid()).FirstOrDefaultAsync();

            world.WorldStatus = WorldStatusEnumerator.USING;
            world.UsedDate = DateTime.UtcNow.AddHours(-3);
            await UpdateWorld(world);

            if(actualWorld != null) {
                actualWorld.WorldStatus = WorldStatusEnumerator.USED;
                await UpdateWorld(actualWorld);
            }

            GenerateWorldCache(world);

            return world.Name;
        }

        private void GenerateWorldCache(WorldEntity world) {

            var worldJson = JsonConvert.SerializeObject(world);

            var cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromDays(1));
            _memoryCache.Set(WORLD_OF_DAY_CACHEKEY, worldJson, cacheEntryOptions);
        }

        private async Task UpdateWorld(WorldEntity world) {
            _dbContext.Update(world);
            await _dbContext.SaveChangesAsync();
        }

        public bool ValidateWorldIsValid(WorldEntity world) {
            return (world.UsedDate.Value.Date == DateTime.UtcNow.AddHours(-3).Date);
        }
        #endregion;

        #region ValidateWorld
        private static readonly Dictionary<int, string> _greenLetters = new();
        private static readonly Dictionary<int, string> _yellowLetters = new();
        private static readonly Dictionary<int, string> _blackLetters = new();

        public async Task<bool> VerifyIdWorldExists(string inputWorld) {
            
            inputWorld = inputWorld.ToUpper();

            var worldBd = await _dbContext.Worlds.FirstOrDefaultAsync(x => x.Name.Equals(inputWorld));

            return (worldBd != null);
        }

        public async Task<Try> ValidateWorld(string inputWorld, string ipAdress, string playerName) {

            inputWorld = inputWorld.ToUpper();

            WORLD_TO_DISCOVERY = await GetWorld();

            _greenLetters.Clear();
            _yellowLetters.Clear();
            _blackLetters.Clear();

            var tryEqual = await ValidateIfIsEqual(inputWorld, ipAdress, playerName);

            if(tryEqual != null) {
                return tryEqual;
            }

            DefineYellowAndBlackLetters(inputWorld);
            DefineGreenLetters(inputWorld);
            RemoveGreenLettersInYellowLetters();

            var returnModel = new Try {
                IsSucces = false,
                DateTry = DateTime.UtcNow.AddDays(-3),
                GreenLetters = _greenLetters,
                YellowLetters = _yellowLetters,
                BlackLetters = _blackLetters
            };

            var player = await GeneratePlayerIfNotExists(ipAdress, playerName);

            await GenerateTryInDatabase(returnModel, player);

            var playerTries = await GetTriesOfPlayerToday(player);

            if(playerTries != null && playerTries.Count >= NUMBER_MAX_TRIES) {
                returnModel.World = await GetWorld();
            }

            return returnModel;
        }

        private async Task<Try> ValidateIfIsEqual(string inputWorld, string ipAdress, string playerName) {

            if(!inputWorld.Equals(WORLD_TO_DISCOVERY, StringComparison.CurrentCultureIgnoreCase)) {
                return null;
            }

            for(int i = 0; i<inputWorld.Length; i++) {
                _greenLetters.Add(i+1, inputWorld[i].ToString());
            }

            var returnModel = new Try {
                IsSucces = true,
                DateTry = DateTime.UtcNow.AddHours(-3),
                GreenLetters = _greenLetters,
                YellowLetters = _yellowLetters,
                BlackLetters = _blackLetters
            };

            var player = await GeneratePlayerIfNotExists(ipAdress, playerName);

            await GenerateTryInDatabase(returnModel, player);

            return returnModel;
        }

        private void DefineYellowAndBlackLetters(string inputWorld) {

            int index = 0;
            var arrayWorldCorrect = inputWorld.ToCharArray();
            foreach(var letra in arrayWorldCorrect) {

                if(WORLD_TO_DISCOVERY.Contains(letra) && DefineNumberLetters(letra.ToString(), _yellowLetters)) {
                    _yellowLetters.Add(index + 1, letra.ToString());
                } else {
                    _blackLetters.Add(index + 1, letra.ToString());
                }
                index++;

            }

        }

        private void DefineGreenLetters(string inputWorld) {
            for(int i = 0; i < WORLD_TO_DISCOVERY.Length; i++) {
                if(inputWorld[i].Equals(WORLD_TO_DISCOVERY[i])) {
                    _greenLetters.Add(i + 1, inputWorld[i].ToString());
                }
            }
        }

        private void RemoveGreenLettersInYellowLetters() {
            foreach(var letraVerde in _greenLetters) {

                if(_yellowLetters.Contains(letraVerde)) {
                    var letrasAmarelas = _yellowLetters.Where(x => x.Value.Equals(letraVerde.Value));
                    var toRemove = letrasAmarelas.FirstOrDefault(x => x.Key == letraVerde.Key);

                    _yellowLetters.Remove(toRemove.Key);
                }
            }
        }

        private bool DefineNumberLetters(string letra, Dictionary<int, string> letrasAmarelas) {

            var stringSplit = WORLD_TO_DISCOVERY.Split(letra);
            var quantidadeLetraNaPalavra = stringSplit.Length - 1;
            var quantidadeLetraNaEntrada = 0;

            foreach(var letraAmarela in letrasAmarelas) {

                if(letraAmarela.Value.Equals(letra, StringComparison.CurrentCultureIgnoreCase)) {
                    quantidadeLetraNaEntrada++;
                }

            }

            return (quantidadeLetraNaEntrada < quantidadeLetraNaPalavra);

        }

        private async Task GenerateTryInDatabase(Try tryModel, PlayerEntity playerEntity) {

            var jsonTry = JsonConvert.SerializeObject(tryModel);

            var tryEntity = new TryEntity {
                Success = tryModel.IsSucces,
                TryDate = DateTime.UtcNow.AddHours(-3),
                PlayerId = playerEntity.Id,
                JsonTry = jsonTry
            };

            await _dbContext.AddAsync(tryEntity);
            await _dbContext.SaveChangesAsync();
        }

        private async Task<PlayerEntity> GeneratePlayerIfNotExists(string ipAdress, string playerName) {

            var player = await _dbContext.Players.FirstOrDefaultAsync(x => x.IpAdress.Equals(ipAdress));

            if(player != null) {
                return player;
            }

            player = new PlayerEntity {
                IpAdress = ipAdress,
                Name = (string.IsNullOrWhiteSpace(playerName)) ? "NAO_INFORMADO" : playerName
            };

            await _dbContext.AddAsync(player);
            await _dbContext.SaveChangesAsync();

            return player;
        }

        public async Task GenerateWorldIfIsValid(string world)
        {

            var worldMeaning = await _dictionaryService.GetWorldInDictionary(world);

            if (!worldMeaning.IsSuccessStatusCode)
            {
                return;
            }

            var resultContent = worldMeaning.Content;

            if (resultContent == null || !resultContent.Any())
            {
                return;
            }

            if(string.IsNullOrWhiteSpace(resultContent.First().Class))
            {
                return;
            }

            var worldEntity = new WorldEntity
            {
                Name = world,
                WorldStatus = WorldStatusEnumerator.WATING
            };

            await _dbContext.AddAsync(worldEntity);
            await _dbContext.SaveChangesAsync();

        }
        #endregion

        #region ValidatePlayerCanPlay
        public async Task<bool> CanPlayerPlay(string ipAdress, string playerName) {
            var player = await GeneratePlayerIfNotExists(ipAdress, playerName);

            var tries = await GetTriesOfPlayerToday(player);

            if(tries.Where(x => x.Success == true).Any()) {
                return false;
            }

            if(tries != null && tries.Count >= NUMBER_MAX_TRIES) {
                return false;
            }

            return true;
        }

        public async Task<List<TryEntity>> GetTriesOfPlayerToday(PlayerEntity player) {
            var tries = await _dbContext.Tries.Where(x => x.PlayerId == player.Id && x.TryDate.Date == DateTime.UtcNow.AddHours(-3).Date).ToListAsync();

            return tries;
        }
        #endregion

        #region GetProgessActualPlayer

        public async Task<List<Try>> GetTriesTodayPlyer(string ipAdress) {

            var tries = await _dbContext.Tries
                .Include(x => x.Player)
                .Where(x => x.TryDate.Date == DateTime.UtcNow.AddHours(-3).Date && x.Player.IpAdress.Equals(ipAdress))
                .OrderBy(x => x.TryDate)
                .ToListAsync();

            if(tries == null) {
                return null;
            }

            var ret = tries.Select(x => {
                return JsonConvert.DeserializeObject<Try>(x.JsonTry);
            }).ToList();

            return ret;
        }


        #endregion

    }
}
