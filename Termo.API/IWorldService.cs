using System.Collections.Generic;
using System.Threading.Tasks;

namespace Termo.API {
    public interface IWorldService {

        Task<string> GetWorld();
        Task<bool> VerifyIdWorldExists(string inputWorld);
        Task<Try> ValidateWorld(string inputWorld, string ipAdress, string playerName);
        Task<bool> CanPlayerPlay(string ipAdress, string playerName);
        Task<List<Try>> GetTriesTodayPlyer(string ipAdress);

    }
}
