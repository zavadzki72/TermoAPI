using System.Threading.Tasks;
using Termo.Models.Entities;

namespace Termo.Models.Interfaces
{
    public interface IPlayerRepository
    {
        Task<PlayerEntity> GetPlayerByIpAdress(string ipAdress);
        Task Add(PlayerEntity player);
    }
}
