using System.Collections.Generic;
using System.Threading.Tasks;

namespace Termo.Models {
    public interface IWorldService {

        Task<bool> VerifyIdWorldExists(string inputWorld);
        Task<Try> ValidateWorld(string inputWorld, string ipAdress, string playerName);
        Task<bool> CanPlayerPlay(string ipAdress, string playerName);
        Task<List<Try>> GetTriesTodayPlyer(string ipAdress);
        Task GenerateWorldIfIsValid(string world);

    }
}
