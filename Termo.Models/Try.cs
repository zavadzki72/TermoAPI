using System;
using System.Collections.Generic;

namespace Termo.Models {
    public class Try {
        public DateTime DateTry { get; set; }
        public bool IsSucces { get; set; }
        public Dictionary<int, string> GreenLetters { get; set; }
        public Dictionary<int, string> YellowLetters { get; set; }
        public Dictionary<int, string> BlackLetters { get; set; }
        public string World { get; set; }
    }
}
