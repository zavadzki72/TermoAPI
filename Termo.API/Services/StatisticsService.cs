using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Termo.Models;
using Termo.Models.Entities;
using Termo.Models.Interfaces;

namespace Termo.API.Services
{
    public class StatisticsService : IStatisticsService {

        private readonly IPlayerRepository _playerRepository;
        private readonly ITryRepository _tryRepository;
        private readonly IWorldRepository _worldRepository;

        public StatisticsService(IPlayerRepository playerRepository, ITryRepository tryRepository, IWorldRepository worldRepository) {
            _playerRepository = playerRepository;
            _tryRepository = tryRepository;
            _worldRepository = worldRepository;
        }

        public async Task<PlayerStatistic> GetPlayerStatistic(string ipAdress) {

            var playerStatistic = new PlayerStatistic
            {
                HoursToNewWorld = GetHoursToNewWorld()
            };

            var player = await _playerRepository.GetPlayerByIpAdress(ipAdress);

            if (player == null) {
                return playerStatistic;
            }

            var playerTries = await _tryRepository.GetTriesByPlayer(player.Id);

            if (playerTries == null || !playerTries.Any()) {
                playerStatistic.PlayerName = player.Name;
                return playerStatistic;
            }

            int totalGames = GetTotalGames(playerTries);
            int winRate = GetWinRate(playerTries, totalGames);
            int winSequency = GetActualWinSequency(playerTries);
            int bestSequency = GetBestWinSequency(playerTries);
            int quantityWinOneChance = GetQuantityToWin(playerTries, 1);
            int quantityWinTwoChance = GetQuantityToWin(playerTries, 2);
            int quantityWinThreeChance = GetQuantityToWin(playerTries, 3);
            int quantityWinFourChance = GetQuantityToWin(playerTries, 4);
            int quantityWinFiveChance = GetQuantityToWin(playerTries, 5);
            int quantityWinSixChance = GetQuantityToWin(playerTries, 6);
            int quantityLoses = GetTotalLoses(playerTries);
            string shareStr = await GetShareString(playerTries);

            playerStatistic.PlayerName = player.Name;
            playerStatistic.TotalGames = totalGames;
            playerStatistic.WinRate = winRate;
            playerStatistic.WinSequency = winSequency;
            playerStatistic.BestSequency = bestSequency;
            playerStatistic.QuantityWinOneChance = quantityWinOneChance;
            playerStatistic.QuantityWinTwoChance = quantityWinTwoChance;
            playerStatistic.QuantityWinThreeChance = quantityWinThreeChance;
            playerStatistic.QuantityWinFourChance = quantityWinFourChance;
            playerStatistic.QuantityWinFiveChance = quantityWinFiveChance;
            playerStatistic.QuantityWinSixChance = quantityWinSixChance;
            playerStatistic.QuantityLoses = quantityLoses;
            playerStatistic.ShareText = shareStr;

            return playerStatistic;
        }

        private static int GetTotalGames(List<TryEntity> tries) {
            int quantity = tries.GroupBy(x => x.TryDate.Date).Count();
            return quantity;
        }

        private static int GetWinRate(List<TryEntity> tries, int totalGames) {
            int quantityWins = tries.Where(x => x.Success).Count();
            int percenteWins = (int)Math.Round((double)(100 * quantityWins) / totalGames);
            return percenteWins;
        }

        private static int GetActualWinSequency(List<TryEntity> tries) {
            var senquencies = GetSenquencies(tries);
            return senquencies.FirstOrDefault();
        }

        private static int GetTotalLoses(List<TryEntity> tries) {

            var triesByDate = tries.GroupBy(x => x.TryDate.Date).OrderBy(x => x.Key);
            int count = 0;

            foreach(var tryByDate in triesByDate) {

                bool wins = false;

                foreach(var tryModel in tryByDate) {

                    if(tryModel.Success) {
                        wins = true;
                    }

                }

                if(!wins) {
                    count++;
                }
            }

            return count;
        }

        private static int GetBestWinSequency(List<TryEntity> tries) {
            var senquencies = GetSenquencies(tries);

            var ret = senquencies.OrderByDescending(x => x);

            return ret.FirstOrDefault();
        }

        private static List<int> GetSenquencies(List<TryEntity> tries) {

            List<int> senquencies = new();
            int count = 0;

            var triesByDate = tries.GroupBy(x => x.TryDate.Date).OrderByDescending(x => x.Key);

            foreach(var tryByDate in triesByDate) {

                bool wins = false;

                foreach(var tryModel in tryByDate) {

                    if(tryModel.Success) {
                        count++;
                        wins = true;
                    }

                }

                if(!wins) {
                    senquencies.Add(count);
                    count = 0;
                }
            }

            return senquencies;
        }

        private static int GetQuantityToWin(List<TryEntity> tries, int quantityExpected) {

            int count = 0;

            var triesByDate = tries.GroupBy(x => x.TryDate.Date).OrderByDescending(x => x.Key);

            foreach(var tryByDate in triesByDate) {
                foreach(var tryModel in tryByDate) {

                    if(tryModel.Success) {
                        if(tryByDate.Count() == quantityExpected) {
                            count++;
                        }
                    }

                }
            }

            return count;
        }

        private static TimeSpan GetHoursToNewWorld() {
            return new TimeSpan(23, 59, 59) - DateTime.Now.TimeOfDay;
        }

        private async Task<string> GetShareString(List<TryEntity> tries) {

            var triesToday = tries.Where(x => x.TryDate.Date == DateTime.UtcNow.AddHours(-3).Date).OrderBy(x => x.TryDate).ToList();

            if(triesToday.Count < 6 && !triesToday.Any(x => x.Success)) {
                return string.Empty;
            }

            var quantityGames = await _worldRepository.GetWorldsNotWaiting();

            var qttTry = triesToday.Any(x => x.Success) ? triesToday.Count.ToString() : "X";

            string initial = $"Joguei palavra.marccusz.com #{quantityGames.Count} {qttTry}/6\n\n";

            foreach(var tryIndex in triesToday) {
                char[] worldEmoji = new char[5];

                var tryModel = JsonConvert.DeserializeObject<Try>(tryIndex.JsonTry);

                foreach(var greenLetter in tryModel.GreenLetters) {
                    worldEmoji[greenLetter.Key - 1] = 'V';
                }

                foreach(var greenLetter in tryModel.YellowLetters) {
                    worldEmoji[greenLetter.Key - 1] = 'A';
                }

                foreach(var greenLetter in tryModel.BlackLetters) {
                    worldEmoji[greenLetter.Key - 1] = 'P';
                }

                initial += $"{new string(worldEmoji)}\n";

            }

            return initial;
        }

    }
}
