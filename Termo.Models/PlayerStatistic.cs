using System;

namespace Termo.Models
{
    public class PlayerStatistic {

        public string PlayerName { get; set; }
        public int TotalGames { get; set; }
        public int WinRate { get; set; }
        public int WinSequency { get; set; }
        public int BestSequency { get; set; }
        public int QuantityWinOneChance { get; set; }
        public int QuantityWinTwoChance { get; set; }
        public int QuantityWinThreeChance { get; set; }
        public int QuantityWinFourChance { get; set; }
        public int QuantityWinFiveChance { get; set; }
        public int QuantityWinSixChance { get; set; }
        public int QuantityLoses { get; set; }
        public TimeSpan HoursToNewWorld { get; set; }
        public string ShareText { get; set; }

    }
}
