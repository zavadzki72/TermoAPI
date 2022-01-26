using System;
using System.Collections.Generic;

namespace Termo.API {
    public class Try {
        public DateTime DateTry { get; set; }
        public bool IsSucces { get; set; }
        public Dictionary<int, string> GreenLetters { get; set; }
        public Dictionary<int, string> YellowLetters { get; set; }
        public Dictionary<int, string> BlackLetters { get; set; }
    }
}
