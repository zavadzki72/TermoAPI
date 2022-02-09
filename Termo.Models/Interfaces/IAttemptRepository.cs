using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Termo.Models.Entities;

namespace Termo.Models.Interfaces
{
    public interface IAttemptRepository
    {
        Task<List<AttemptEntity>> GetTriesByPlayer(int playerId);
        Task<List<AttemptEntity>> GetTriesByPlayerAndDate(int playerId, DateTime tryDate);
        Task<List<AttemptEntity>> GetTriesByPlayerIpAndDateOrderingByTryDate(string ipAdress, DateTime tryDate);
        List<IGrouping<DateTime, AttemptEntity>> GetTriesGroupedByTryDate();
        List<IGrouping<int, AttemptEntity>> GetTriesYesterday();
        List<Tuple<string, int>> GetMostWorldsTried();
        Task Add(AttemptEntity tryEntity);
    }
}