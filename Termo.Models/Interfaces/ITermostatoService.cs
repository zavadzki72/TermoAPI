using System.Threading.Tasks;
using Termo.Models.ViewModels;

namespace Termo.Models.Interfaces
{
    public interface ITermostatoService
    {
        Task<TermostatoResponse> GetTodayTermostato();
    }
}
