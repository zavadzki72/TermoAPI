using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Termo.Models.Entities;
using Termo.Models.Interfaces;

namespace Termo.Infrastructure.Repositories
{
    public class AttemptRepository : IAttemptRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public AttemptRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<AttemptEntity>> GetTriesByPlayer(int playerId)
        {
            var playerTries = await _dbContext.Tries.Where(x => x.PlayerId == playerId).ToListAsync();

            return playerTries;
        }

        public async Task<List<AttemptEntity>> GetTriesByPlayerAndDate(int playerId, DateTime tryDate)
        {
            var playerTries = await _dbContext.Tries.Where(x => x.PlayerId == playerId && x.AttemptDate.Date == tryDate.AddHours(-3).Date).ToListAsync();

            return playerTries;
        }

        public async Task<List<AttemptEntity>> GetTriesByPlayerIpAndDateOrderingByTryDate(string ipAdress, DateTime tryDate)
        {
            var playerTries = await _dbContext.Tries
                .Include(x => x.Player)
                .Where(x => x.AttemptDate.Date == tryDate.AddHours(-3).Date && x.Player.IpAdress.Equals(ipAdress))
                .OrderBy(x => x.AttemptDate)
                .ToListAsync();

            return playerTries;
        }

        public List<IGrouping<DateTime, AttemptEntity>> GetTriesGroupedByTryDate()
        {
            var playerTries = _dbContext.Tries
                .AsNoTracking()
                .AsEnumerable()
                .GroupBy(x => x.AttemptDate)
                .ToList();

            return playerTries;
        }

        public List<IGrouping<int, AttemptEntity>> GetTriesYesterday()
        {
            var dateToCompare = DateTime.UtcNow.AddHours(-3).AddDays(-1);

            var playerTries = _dbContext.Tries
                .Where(x => x.AttemptDate.Date == dateToCompare.Date)
                .AsNoTracking()
                .AsEnumerable()
                .GroupBy(x => x.PlayerId)
                .ToList();

            return playerTries;
        }

        public List<Tuple<string, int>> GetMostWorldsTried()
        {
            var query = (
                from tries in _dbContext.Tries
                group tries by tries.TriedWorld into g
                select new Tuple<string, int>(g.Key, g.Count())
            )
            .ToList();


            var retorno = query
                .OrderByDescending(x => x.Item2)
                .Take(10)
                .ToList();

            return retorno;
        }

        public async Task Add(AttemptEntity tryEntity)
        {
            await _dbContext.AddAsync(tryEntity);
            await _dbContext.SaveChangesAsync();
        }
    }
}
