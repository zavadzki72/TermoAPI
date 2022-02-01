using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Termo.Models.Entities;

namespace Termo.Models.Interfaces
{
    public interface ITryRepository
    {
        Task<List<TryEntity>> GetTriesByPlayer(int playerId);
        Task<List<TryEntity>> GetTriesByPlayerAndDate(int playerId, DateTime tryDate);
        Task<List<TryEntity>> GetTriesByPlayerIpAndDateOrderingByTryDate(string ipAdress, DateTime tryDate);
        List<IGrouping<DateTime, TryEntity>> GetTriesGroupedByTryDate();
        List<IGrouping<int, TryEntity>> GetTriesYesterday();
        List<Tuple<string, int>> GetMostWorldsTried();
        Task Add(TryEntity tryEntity);
    }
}