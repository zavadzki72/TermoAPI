using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Termo.Models.Entities;
using Termo.Models.Interfaces;

namespace Termo.Infrastructure.Repositories
{
    public class PlayerRepository : IPlayerRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public PlayerRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<PlayerEntity> GetPlayerByIpAdress(string ipAdress)
        {
            var player = await _dbContext.Players.FirstOrDefaultAsync(x => x.IpAdress.Equals(ipAdress));

            return player;
        }

        public async Task Add(PlayerEntity player)
        {
            await _dbContext.AddAsync(player);
            await _dbContext.SaveChangesAsync();
        }
    }
}
