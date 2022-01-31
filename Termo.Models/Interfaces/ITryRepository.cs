using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Termo.Models.Entities;

namespace Termo.Models.Interfaces
{
    public interface ITryRepository
    {
        Task<List<TryEntity>> GetTriesByPlayer(int playerId);
        Task<List<TryEntity>> GetTriesByPlayerAndDate(int playerId, DateTime tryDate);
        Task<List<TryEntity>> GetTriesByPlayerIpAndDateOrderingByTryDate(string ipAdress, DateTime tryDate);
        Task Add(TryEntity tryEntity);
    }
}
