using Refit;
using System.Collections.Generic;
using System.Threading.Tasks;
using Termo.Models.ViewModels;

namespace Termo.Models.ExternalServices
{
    public interface IDictionaryService
    {
        [Get("/{world}")]
        Task<ApiResponse<List<DictionaryResponse>>> GetWorldInDictionary(string world);
    }
}
