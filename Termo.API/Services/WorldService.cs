using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Termo.Models;
using Termo.Models.Entities;
using Termo.Models.Enumerators;
using Termo.Models.ExternalServices;
using Termo.Models.Interfaces;

namespace Termo.API.Services
{
    public class WorldService : IWorldService {

        private const string WORLD_OF_DAY_CACHEKEY = "WORLD_OF_DAY";
        private const int NUMBER_MAX_TRIES = 6;
        private string WORLD_TO_DISCOVERY;

        private readonly IMemoryCache _memoryCache;
        private readonly IDictionaryService _dictionaryService;
        private readonly IPlayerRepository _playerRepository;
        private readonly IWorldRepository _worldRepository;
        private readonly ITryRepository _tryRepository;
        private readonly IInvalidWorldRepository _invalidWorldRepository;

        public WorldService(
            IMemoryCache memoryCache, 
            IDictionaryService dictionaryService,
            IPlayerRepository playerRepository,
            IWorldRepository worldRepository,
            ITryRepository tryRepository,
            IInvalidWorldRepository invalidWorldRepository
        ) {
            _memoryCache = memoryCache;
            _dictionaryService = dictionaryService;
            _playerRepository = playerRepository;
            _worldRepository = worldRepository;
            _tryRepository = tryRepository;
            _invalidWorldRepository = invalidWorldRepository;
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

            var actualWorld = await _worldRepository.GetActualWorld();

            if(actualWorld != null && ValidateWorldIsValid(actualWorld)) {
                GenerateWorldCache(actualWorld);
                return actualWorld.Name;
            }

            world = await GetNewWorld(actualWorld);

            return world;
        }

        private async Task<string> GetNewWorld(WorldEntity actualWorld) {
            var world = await _worldRepository.GetRandomWorldWithStatusWaiting();

            world.WorldStatus = WorldStatusEnumerator.USING;
            world.UsedDate = DateTime.UtcNow.AddHours(-3);
            await _worldRepository.Update(world);

            if (actualWorld != null) {
                actualWorld.WorldStatus = WorldStatusEnumerator.USED;
                await _worldRepository.Update(world);
            }

            GenerateWorldCache(world);

            return world.Name;
        }

        private void GenerateWorldCache(WorldEntity world) {

            var worldJson = JsonConvert.SerializeObject(world);

            var cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromDays(1));
            _memoryCache.Set(WORLD_OF_DAY_CACHEKEY, worldJson, cacheEntryOptions);
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

            var worldBd = await _worldRepository.GetWorldByName(inputWorld);

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
            RemoveYellowAndBlacksLettersIfThatIsInGreenList();
            PercorrePalavraAndVerifyQuantityLettersIsRigth(inputWorld);

            var returnModel = new Try {
                IsSucces = false,
                DateTry = DateTime.UtcNow.AddDays(-3),
                GreenLetters = _greenLetters,
                YellowLetters = _yellowLetters,
                BlackLetters = _blackLetters
            };

            var player = await GeneratePlayerIfNotExists(ipAdress, playerName);

            await GenerateTryInDatabase(returnModel, player, inputWorld);

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

            await GenerateTryInDatabase(returnModel, player, inputWorld);

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

        private static void RemoveGreenLettersInYellowLetters() {
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

        private static void RemoveYellowAndBlacksLettersIfThatIsInGreenList()
        {
            foreach(var letrasVerde in _greenLetters)
            {
                _yellowLetters.Remove(letrasVerde.Key);
                _blackLetters.Remove(letrasVerde.Key);
            }
        }

        private void PercorrePalavraAndVerifyQuantityLettersIsRigth(string inputWorld)
        {
            foreach (var letter in inputWorld.ToCharArray())
            {
                string letraToCompare = letter.ToString();
                var stringSplit = WORLD_TO_DISCOVERY.Split(letter);
                var quantidadeLetraNaPalavra = stringSplit.Length - 1;

                var quantidadeLetraVerde = _greenLetters.Where(x => x.Value == letraToCompare).Count();
                var quantidadeLetraAmarelo = _yellowLetters.Where(x => x.Value == letraToCompare).Count();
                var quantidadeTotal = quantidadeLetraVerde + quantidadeLetraAmarelo;

                if(quantidadeTotal > quantidadeLetraNaPalavra)
                {
                    var toRemove = _yellowLetters.Where(x => x.Value == letraToCompare).FirstOrDefault();
                    if(toRemove.Value != null)
                    {
                        _yellowLetters.Remove(toRemove.Key);
                        _blackLetters.TryAdd(toRemove.Key, toRemove.Value);
                    }
                }
            }
        }

        private async Task GenerateTryInDatabase(Try tryModel, PlayerEntity playerEntity, string worldInput) {

            var jsonTry = JsonConvert.SerializeObject(tryModel);

            var tryEntity = new TryEntity {
                Success = tryModel.IsSucces,
                TryDate = DateTime.UtcNow.AddHours(-3),
                PlayerId = playerEntity.Id,
                JsonTry = jsonTry,
                TriedWorld = worldInput
            };

            await _tryRepository.Add(tryEntity);
        }

        private async Task<PlayerEntity> GeneratePlayerIfNotExists(string ipAdress, string playerName) {

            var player = await _playerRepository.GetPlayerByIpAdress(ipAdress);

            if(player != null) {
                return player;
            }

            player = new PlayerEntity {
                IpAdress = ipAdress,
                Name = (string.IsNullOrWhiteSpace(playerName)) ? "NAO_INFORMADO" : playerName
            };

            await _playerRepository.Add(player);

            return player;
        }

        public async Task GenerateWorldIfIsValid(string world)
        {

            var worldMeaning = await _dictionaryService.GetWorldInDictionary(world);

            if (!worldMeaning.IsSuccessStatusCode)
            {
                await GenerateInvalidWorld(world);
                return;
            }

            var resultContent = worldMeaning.Content;

            if (resultContent == null || !resultContent.Any())
            {
                await GenerateInvalidWorld(world);
                return;
            }

            if(string.IsNullOrWhiteSpace(resultContent.First().Class))
            {
                await GenerateInvalidWorld(world);
                return;
            }

            var worldEntity = new WorldEntity
            {
                Name = world,
                WorldStatus = WorldStatusEnumerator.WATING
            };

            await _worldRepository.Add(worldEntity);
        }

        private async Task GenerateInvalidWorld(string world)
        {
            var invalidWorld = new InvalidWorldEntity
            {
                CreatedAt = DateTime.Now,
                World = world
            };

            await _invalidWorldRepository.Add(invalidWorld);
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
            var tries = await _tryRepository.GetTriesByPlayerAndDate(player.Id, DateTime.UtcNow);
            return tries;
        }
        #endregion

        #region GetProgessActualPlayer

        public async Task<List<Try>> GetTriesTodayPlyer(string ipAdress) {

            var tries = await _tryRepository.GetTriesByPlayerIpAndDateOrderingByTryDate(ipAdress, DateTime.UtcNow);

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
