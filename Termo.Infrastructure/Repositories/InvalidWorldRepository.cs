using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Termo.Models.Entities;
using Termo.Models.Interfaces;

namespace Termo.Infrastructure.Repositories
{
    public class InvalidWorldRepository : IInvalidWorldRepository
    {

        private readonly ApplicationDbContext _dbContext;

        public InvalidWorldRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public List<Tuple<string, int>> GetMostInvalidWorlds()
        {
            var query = (
                from invalidWorld in _dbContext.InvalidWorlds
                group invalidWorld by invalidWorld.World into invalidWorldGroup
                select new Tuple<string, int>(invalidWorldGroup.Key, invalidWorldGroup.Count())
            )
            .ToList();

            var retorno = query
                .OrderByDescending(x => x.Item2)
                .Take(10)
                .ToList();

            return retorno;
        }

        public async Task Add(InvalidWorldEntity invalidWorldEntity)
        {
            await _dbContext.InvalidWorlds.AddAsync(invalidWorldEntity);
            await _dbContext.SaveChangesAsync();
        }
    }
}
