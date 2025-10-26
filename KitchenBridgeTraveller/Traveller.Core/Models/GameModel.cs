using System;
using System.Collections.Generic;
using System.Text;

namespace Traveller.Core.Models
{
    public class GameModel
    {
        public enum Player
        {
            North, South, East, West
        }

        public class GameResult
        {
            public int BoardRanking { get; set; }
            public int MatchPoints { get; set; }
            public int GameScore { get; set; }
            public string CardLead { get; set; }
        }

        public Dictionary<Player, string> PlayerHands { get; set; }
        public HashSet<GameResult> GameResults { get; set; }

    }
}
