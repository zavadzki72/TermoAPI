using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Termo.API.Database;
using Termo.API.Entities;

namespace Termo.API {
    public class StatisticsService : IStatisticsService {

        private readonly ApplicationDbContext _dbContext;

        public StatisticsService(ApplicationDbContext dbContext) {
            _dbContext = dbContext;
        }

        public async Task<PlayerStatistic> GetPlayerStatistic(string ipAdress) {

            var player = await _dbContext.Players.FirstOrDefaultAsync(x => x.IpAdress.Equals(ipAdress));

            if(player == null) {
                return new PlayerStatistic {
                    TotalGames = 0,
                    WinRate = 0,
                    WinSequency = 0,
                    BestSequency = 0,
                    QuantityWinOneChance = 0,
                    QuantityWinTwoChance = 0,
                    QuantityWinThreeChance = 0,
                    QuantityWinFourChance = 0,
                    QuantityWinFiveChance = 0,
                    QuantityWinSixChance = 0,
                    QuantityLoses = 0,
                    HoursToNewWorld = GetHoursToNewWorld()
                };
            }

            var playerTries = await _dbContext.Tries.Where(x => x.PlayerId == player.Id).ToListAsync();

            if(playerTries == null || !playerTries.Any()) {
                return new PlayerStatistic {
                    PlayerName = player.Name,
                    TotalGames = 0,
                    WinRate = 0,
                    WinSequency = 0,
                    BestSequency = 0,
                    QuantityWinOneChance = 0,
                    QuantityWinTwoChance = 0,
                    QuantityWinThreeChance = 0,
                    QuantityWinFourChance = 0,
                    QuantityWinFiveChance = 0,
                    QuantityWinSixChance = 0,
                    QuantityLoses = 0,
                    HoursToNewWorld = GetHoursToNewWorld()
                };
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

            return new PlayerStatistic {
                PlayerName = player.Name,
                TotalGames = totalGames,
                WinRate = winRate,
                WinSequency = winSequency,
                BestSequency = bestSequency,
                QuantityWinOneChance = quantityWinOneChance,
                QuantityWinTwoChance = quantityWinTwoChance,
                QuantityWinThreeChance = quantityWinThreeChance,
                QuantityWinFourChance = quantityWinFourChance,
                QuantityWinFiveChance = quantityWinFiveChance,
                QuantityWinSixChance = quantityWinSixChance,
                QuantityLoses = quantityLoses,
                HoursToNewWorld = GetHoursToNewWorld()
            };
        }

        private int GetTotalGames(List<TryEntity> tries) {
            int quantity = tries.GroupBy(x => x.TryDate.Date).Count();
            return quantity;
        }

        private int GetWinRate(List<TryEntity> tries, int totalGames) {
            int quantityWins = tries.Where(x => x.Success).Count();
            int percenteWins = (int)Math.Round((double)(100 * quantityWins) / totalGames);
            return percenteWins;
        }

        private int GetActualWinSequency(List<TryEntity> tries) {
            var senquencies = GetSenquencies(tries);
            return senquencies.FirstOrDefault();
        }

        private int GetTotalLoses(List<TryEntity> tries) {

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

        private int GetBestWinSequency(List<TryEntity> tries) {
            var senquencies = GetSenquencies(tries);

            var ret = senquencies.OrderByDescending(x => x);

            return ret.FirstOrDefault();
        }

        private List<int> GetSenquencies(List<TryEntity> tries) {

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

        private int GetQuantityToWin(List<TryEntity> tries, int quantityExpected) {

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

        private TimeSpan GetHoursToNewWorld() {
            return new TimeSpan(23, 59, 59) - DateTime.Now.TimeOfDay;
        }

    }
}
