using System.Threading.Tasks;

namespace Termo.Models {
    public interface IStatisticsService {

        Task<PlayerStatistic> GetPlayerStatistic(string ipAdress);

    }
}
