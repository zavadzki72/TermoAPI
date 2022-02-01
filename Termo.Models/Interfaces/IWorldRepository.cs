using System.Collections.Generic;
using System.Threading.Tasks;
using Termo.Models.Entities;

namespace Termo.Models.Interfaces
{
    public interface IWorldRepository
    {
        Task<WorldEntity> GetActualWorld();
        Task<WorldEntity> GetLastWorldUsed();
        Task<WorldEntity> GetRandomWorldWithStatusWaiting();
        Task<WorldEntity> GetWorldByName(string worldStr);
        Task<List<WorldEntity>> GetWorldsNotWaiting();
        Task Add(WorldEntity world);
        Task Update(WorldEntity world);
    }
}
