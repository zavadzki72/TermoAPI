using System.Collections.Generic;

namespace Termo.API.Models
{
    public class DictionaryResponse
    {
        public string Class { get; set; }
        public List<string> Meanings { get; set; }
        public string Etymology { get; set; }

    }
}
