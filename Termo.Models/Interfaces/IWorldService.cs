using System.Collections.Generic;
using System.Threading.Tasks;
using Termo.Models.ViewModels;

namespace Termo.Models {
    public interface IWorldService {

        Task<bool> VerifyIdWorldExists(string inputWorld);
        Task<Attempt> ValidateWorld(ValidateWorldViewModel validateWorldViewModel);
        Task<bool> CanPlayerPlay(string ipAdress);
        Task<List<Attempt>> GetTriesTodayPlyer(string ipAdress);
        Task GenerateWorldIfIsValid(string world);

    }
}
