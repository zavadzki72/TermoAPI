using System.Collections.Generic;

namespace Termo.Models.ViewModels
{
    public class TermostatoResponse
    {
        public int NumberWorldLastDay { get; set; }
        public string WorldLastDay { get; set; }
        public int PercentageFirstTryGeneral { get; set; }
        public int PercentageWinGeneral { get; set; }
        public int QuantityGamesGeneral { get; set; }
        public int PercentageWinOneChance { get; set; }
        public int PercentageWinTwoChance { get; set; }
        public int PercentageWinThreeChance { get; set; }
        public int PercentageWinFourChance { get; set; }
        public int PercentageWinFiveChance { get; set; }
        public int PercentageWinSixChance { get; set; }
        public int PercentageLoses { get; set; }
        public Dictionary<string, int> InvalidWorlds { get; set; }
        public Dictionary<string, int> FirstWorlds { get; set; }
        public Dictionary<string, int> MostTriedWorlds { get; set; }

    }
}
