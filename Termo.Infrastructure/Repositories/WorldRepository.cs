using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Termo.Models.Entities;
using Termo.Models.Enumerators;
using Termo.Models.Interfaces;

namespace Termo.Infrastructure.Repositories
{
    public class WorldRepository : IWorldRepository
    {

        private readonly ApplicationDbContext _dbContext;

        public WorldRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<WorldEntity> GetActualWorld()
        {
            var actualWorld = await _dbContext.Worlds.FirstOrDefaultAsync(x => x.WorldStatus.Equals(WorldStatusEnumerator.USING));

            return actualWorld;
        }

        public async Task<WorldEntity> GetLastWorldUsed()
        {
            var lastUsedWorld = await _dbContext.Worlds
                .Where(x => x.WorldStatus.Equals(WorldStatusEnumerator.USED))
                .OrderByDescending(x => x.UsedDate)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            return lastUsedWorld;
        }

        public async Task<WorldEntity> GetRandomWorldWithStatusWaiting()
        {
            var world = await _dbContext.Worlds
                .Where(x => x.WorldStatus.Equals(WorldStatusEnumerator.WATING))
                .OrderBy(r => Guid.NewGuid())
                .AsNoTracking()
                .FirstOrDefaultAsync();

            return world;
        }

        public async Task<WorldEntity> GetWorldByName(string worldStr)
        {
            var world = await _dbContext.Worlds
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Name.Equals(worldStr));

            return world;
        }

        public async Task<List<WorldEntity>> GetWorldsNotWaiting()
        {
            var worlds = await _dbContext.Worlds
                .Where(x => x.WorldStatus != WorldStatusEnumerator.WATING)
                .AsNoTracking()
                .ToListAsync();

            return worlds;
        }

        public async Task Add(WorldEntity world)
        {
            await _dbContext.AddAsync(world);
            await _dbContext.SaveChangesAsync();
        }

        public async Task Update(WorldEntity world)
        {
            _dbContext.Update(world);
            await _dbContext.SaveChangesAsync();
        }

    }
}
