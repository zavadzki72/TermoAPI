using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Termo.Models.Entities;
using Termo.Models.Interfaces;

namespace Termo.Infrastructure.Repositories
{
    public class TryRepository : ITryRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public TryRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<List<TryEntity>> GetTriesByPlayer(int playerId)
        {
            var playerTries = await _dbContext.Tries.Where(x => x.PlayerId == playerId).ToListAsync();

            return playerTries;
        }

        public async Task<List<TryEntity>> GetTriesByPlayerAndDate(int playerId, DateTime tryDate)
        {
            var playerTries = await _dbContext.Tries.Where(x => x.PlayerId == playerId && x.TryDate.Date == tryDate.AddHours(-3).Date).ToListAsync();

            return playerTries;
        }

        public async Task<List<TryEntity>> GetTriesByPlayerIpAndDateOrderingByTryDate(string ipAdress, DateTime tryDate)
        {
            var playerTries = await _dbContext.Tries
                .Include(x => x.Player)
                .Where(x => x.TryDate.Date == tryDate.AddHours(-3).Date && x.Player.IpAdress.Equals(ipAdress))
                .OrderBy(x => x.TryDate)
                .ToListAsync();

            return playerTries;
        }

        public async Task Add(TryEntity tryEntity)
        {
            await _dbContext.AddAsync(tryEntity);
            await _dbContext.SaveChangesAsync();
        }
    }
}
