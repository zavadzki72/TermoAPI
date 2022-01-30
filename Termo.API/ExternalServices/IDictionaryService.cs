using Refit;
using System.Collections.Generic;
using System.Threading.Tasks;
using Termo.API.Models;

namespace Termo.API.ExternalServices
{
    public interface IDictionaryService
    {
        [Get("/{world}")]
        Task<ApiResponse<List<DictionaryResponse>>> GetWorldInDictionary(string world);
    }
}
