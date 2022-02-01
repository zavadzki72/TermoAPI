using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Termo.Models.Entities;
using Termo.Models.Interfaces;
using Termo.Models.ViewModels;

namespace Termo.API.Services
{
    public class TermostatoService : ITermostatoService
    {
        private const string TERMOSTATO_OF_DAY_CACHEKEY = "TERMOSTATO_OF_DAY";

        private readonly IMemoryCache _memoryCache;
        private readonly ITryRepository _tryRepository;
        private readonly IWorldRepository _worldRepository;
        private readonly IInvalidWorldRepository _invalidWorldRepository;

        public TermostatoService(
            IMemoryCache memoryCache, 
            ITryRepository tryRepository, 
            IWorldRepository worldRepository, 
            IInvalidWorldRepository invalidWorldRepository
        )
        {
            _memoryCache = memoryCache;
            _tryRepository = tryRepository;
            _worldRepository = worldRepository;
            _invalidWorldRepository = invalidWorldRepository;
        }

        public async Task<TermostatoResponse> GetTodayTermostato()
        {
            if (_memoryCache.TryGetValue(TERMOSTATO_OF_DAY_CACHEKEY, out string termostatoJson))
            {
                var cahceTermostato = JsonConvert.DeserializeObject<TermostatoResponse>(termostatoJson);
                return cahceTermostato;
            }

            var termostato = await CalculeTodayTermostato();
            var newTermostatoJson = JsonConvert.SerializeObject(termostato);
            
            var cacheEntryOptions = new MemoryCacheEntryOptions().SetSlidingExpiration(TimeSpan.FromDays(1));
            _memoryCache.Set(TERMOSTATO_OF_DAY_CACHEKEY, newTermostatoJson, cacheEntryOptions);

            return termostato;
        }

        private async Task<TermostatoResponse> CalculeTodayTermostato()
        {
            var allTriesByDate = _tryRepository.GetTriesGroupedByTryDate();
            var allTriesYesterday = _tryRepository.GetTriesYesterday();
            var totalGames = GetTotalGames(allTriesByDate);
            var totalGamesYesterday = GetTotalGamesYesterday(allTriesYesterday);
            var mostInvalidWorlds = _invalidWorldRepository.GetMostInvalidWorlds();
            var mostTriedWorlds = _tryRepository.GetMostWorldsTried();

            var lastUsedWorld = await _worldRepository.GetLastWorldUsed();
            var numberLastUsedWorld = await GetNumberLastWorld();
            var winsFirstTime = GetPercentWinsFirstTime(allTriesByDate, totalGames);
            var wins = GetPercentWins(allTriesByDate, totalGames);
            var percentWinsFirstTime = (totalGamesYesterday == 0) ? 0 : (GetWinsFirstTimeYesterday(allTriesYesterday) * 100) / totalGamesYesterday;
            var percentWinsSecondTime = (totalGamesYesterday == 0) ? 0 : (GetSecondSecondTimeYesterday(allTriesYesterday) * 100) / totalGamesYesterday;
            var percentWinsThirdTime = (totalGamesYesterday == 0) ? 0 : (GetSecondThirdTimeYesterday(allTriesYesterday) * 100) / totalGamesYesterday;
            var percentWinsForthyTime = (totalGamesYesterday == 0) ? 0 : (GetSecondForthyTimeYesterday(allTriesYesterday) * 100) / totalGamesYesterday;
            var percentWinsFifthyTime = (totalGamesYesterday == 0) ? 0 : (GetSecondFifthyTimeYesterday(allTriesYesterday) * 100) / totalGamesYesterday;
            var percentWinsSixthyTime = (totalGamesYesterday == 0) ? 0 : (GetSecondSixthyTimeYesterday(allTriesYesterday) * 100) / totalGamesYesterday;
            var percentLoses = (totalGamesYesterday == 0) ? 0 : (GetLosesYesterday(allTriesYesterday) * 100) / totalGamesYesterday;
            var mostInvalidWorldsDictionaries = mostInvalidWorlds.ToDictionary(l => l.Item1, l => l.Item2);
            var worldsFirstTime = GetWorldsFirstTime(allTriesByDate);
            var mostTriedWorldsDictionaries = mostTriedWorlds.ToDictionary(l => l.Item1, l => l.Item2);

            var termostatoResponse = new TermostatoResponse
            {
                WorldLastDay = lastUsedWorld.Name,
                QuantityGamesGeneral = totalGames,
                NumberWorldLastDay = numberLastUsedWorld,
                PercentageFirstTryGeneral = winsFirstTime,
                PercentageWinGeneral = wins,
                PercentageWinOneChance = percentWinsFirstTime,
                PercentageWinTwoChance = percentWinsSecondTime,
                PercentageWinThreeChance = percentWinsThirdTime,
                PercentageWinFourChance = percentWinsForthyTime,
                PercentageWinFiveChance = percentWinsFifthyTime,
                PercentageWinSixChance = percentWinsSixthyTime,
                PercentageLoses = percentLoses,
                InvalidWorlds = mostInvalidWorldsDictionaries,
                FirstWorlds = worldsFirstTime,
                MostTriedWorlds = mostTriedWorldsDictionaries
            };

            return termostatoResponse;
        }

        private async Task<int> GetNumberLastWorld()
        {
            var worldsNotWaiting = await _worldRepository.GetWorldsNotWaiting();

            return worldsNotWaiting.Count - 1;
        }

        private static int GetPercentWinsFirstTime(List<IGrouping<DateTime, TryEntity>> allTriesByDate, int totalGames)
        {
            var winsFirstTime = GetWinsFirstTime(allTriesByDate);

            var percentage = (winsFirstTime * 100) / totalGames;
            return percentage;
        }

        private static int GetPercentWins(List<IGrouping<DateTime, TryEntity>> allTriesByDate, int totalGames)
        {
            var winsFirstTime = GetWins(allTriesByDate);

            var percentage = (winsFirstTime * 100) / totalGames;
            return percentage;
        }

        private static int GetWinsFirstTime(List<IGrouping<DateTime, TryEntity>> allTriesByDate)
        {
            var success = 0;

            foreach (var triesByDate in allTriesByDate)
            {
                var triesByPlayer = triesByDate.GroupBy(x => x.PlayerId);

                foreach (var tentativas in triesByPlayer)
                {
                    var triesOrdered = tentativas.OrderBy(x => x.TryDate);
                    var primeiro = triesOrdered.FirstOrDefault();
                    if (primeiro.Success)
                    {
                        success++;
                    }
                }
                
            }

            return success;
        }

        private static int GetWins(List<IGrouping<DateTime, TryEntity>> allTriesByDate)
        {
            var count = 0;

            foreach (var triesByDate in allTriesByDate)
            {
                var triesByPlayer = triesByDate.GroupBy(x => x.PlayerId);

                foreach (var tentativas in triesByPlayer)
                {
                    var triesOrdered = tentativas.OrderBy(x => x.TryDate);

                    foreach(var tentativa in triesOrdered)
                    {
                        if (tentativa.Success)
                        {
                            count++;
                        }
                    }
                }

            }

            return count;
        }

        private static int GetTotalGames(List<IGrouping<DateTime, TryEntity>> allTriesByDate)
        {
            var count = 0;

            foreach (var triesByDate in allTriesByDate)
            {
                var triesByPlayer = triesByDate.GroupBy(x => x.PlayerId);

                foreach (var tentativas in triesByPlayer)
                {
                    count++;
                }

            }

            return count;
        }

        private static int GetTotalGamesYesterday(List<IGrouping<int, TryEntity>> allTriesYesterday)
        {
            var totalGames = 0;

            foreach(var tentativas in allTriesYesterday)
            {
                totalGames++;
            }

            return totalGames;
        }

        private static int GetWinsFirstTimeYesterday(List<IGrouping<int, TryEntity>> allTriesYesterday)
        {
            var succes = 0;

            foreach (var tentativas in allTriesYesterday)
            {
                var tentativa = tentativas.OrderBy(x => x.TryDate).FirstOrDefault();
                if (tentativa.Success)
                {
                    succes++;
                }
            }

            return succes;
        }

        private static int GetSecondSecondTimeYesterday(List<IGrouping<int, TryEntity>> allTriesYesterday)
        {
            var succes = 0;

            foreach (var tentativas in allTriesYesterday)
            {
                var tentativa = tentativas.OrderBy(x => x.TryDate);
                var element = tentativa.ElementAtOrDefault(1);
                if (element != null && element.Success)
                {
                    succes++;
                }
            }

            return succes;
        }

        private static int GetSecondThirdTimeYesterday(List<IGrouping<int, TryEntity>> allTriesYesterday)
        {
            var succes = 0;

            foreach (var tentativas in allTriesYesterday)
            {
                var tentativa = tentativas.OrderBy(x => x.TryDate);
                var element = tentativa.ElementAtOrDefault(2);
                if (element != null && element.Success)
                {
                    succes++;
                }
            }

            return succes;
        }

        private static int GetSecondForthyTimeYesterday(List<IGrouping<int, TryEntity>> allTriesYesterday)
        {
            var succes = 0;

            foreach (var tentativas in allTriesYesterday)
            {
                var tentativa = tentativas.OrderBy(x => x.TryDate);
                var element = tentativa.ElementAtOrDefault(3);
                if (element != null && element.Success)
                {
                    succes++;
                }
            }

            return succes;
        }

        private static int GetSecondFifthyTimeYesterday(List<IGrouping<int, TryEntity>> allTriesYesterday)
        {
            var succes = 0;

            foreach (var tentativas in allTriesYesterday)
            {
                var tentativa = tentativas.OrderBy(x => x.TryDate);
                var element = tentativa.ElementAtOrDefault(4);
                if (element != null && element.Success)
                {
                    succes++;
                }
            }

            return succes;
        }

        private static int GetSecondSixthyTimeYesterday(List<IGrouping<int, TryEntity>> allTriesYesterday)
        {
            var succes = 0;

            foreach (var tentativas in allTriesYesterday)
            {
                var tentativa = tentativas.OrderBy(x => x.TryDate);
                var element = tentativa.ElementAtOrDefault(5);
                if (element != null && element.Success)
                {
                    succes++;
                }
            }

            return succes;
        }

        private static int GetLosesYesterday(List<IGrouping<int, TryEntity>> allTriesYesterday)
        {
            var loses = 0;

            foreach (var tentativas in allTriesYesterday)
            {
                var ganhou = false;
                
                foreach(var tentativa in tentativas)
                {
                    if (tentativa.Success)
                    {
                        ganhou = true;
                    }
                }

                if (!ganhou)
                {
                    loses++;
                }
            }

            return loses;
        }

        private static Dictionary<string, int> GetWorldsFirstTime(List<IGrouping<DateTime, TryEntity>> allTriesByDate)
        {
            Dictionary<string, int> worldsFirstTime = new();

            foreach (var triesByDate in allTriesByDate)
            {
                var triesByPlayer = triesByDate.GroupBy(x => x.PlayerId);

                foreach (var tentativas in triesByPlayer)
                {
                    var triesOrdered = tentativas.OrderBy(x => x.TryDate);
                    var primeiro = triesOrdered.FirstOrDefault();

                    if(worldsFirstTime.TryGetValue(primeiro.TriedWorld, out var qttAtual))
                    {
                        var quantia = qttAtual+1;
                        worldsFirstTime[primeiro.TriedWorld] = quantia;
                    }
                    else
                    {
                        worldsFirstTime.Add(primeiro.TriedWorld, 1);
                    }

                }

            }

            var orderedDictionary = worldsFirstTime.OrderByDescending(x => x.Value).Take(10);

            return orderedDictionary.ToDictionary(x => x.Key, x => x.Value);
        }
    }
}
