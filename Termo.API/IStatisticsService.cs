using System.Threading.Tasks;

namespace Termo.API {
    public interface IStatisticsService {

        Task<PlayerStatistic> GetPlayerStatistic(string ipAdress);

    }
}
