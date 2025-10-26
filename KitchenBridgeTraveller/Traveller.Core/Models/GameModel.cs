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
            public int PairIdNS { get; set; }
            public int PairIdEW { get; set; }
            public string Contract { get; set; }
            public Player Declarer { get; set; }
            public int Result { get; set; }
            
            public int BoardRanking { get; set; }
            public int MatchPoints { get; set; }
            public int GameScore { get; set; }
            public string CardLead { get; set; }
        }

        public string Event { get; set; }
        public string Site { get; set; }
        public string Date { get; set; }
        public int BoardNumber { get; set; }
        public Player Dealer { get; set; }
        public string Vulnerable { get; set; }
        public string DealString { get; set; }
        
        public Dictionary<Player, string> PlayerHands { get; set; }
        public HashSet<GameResult> GameResults { get; set; }

        public GameModel()
        {
            PlayerHands = new Dictionary<Player, string>();
            GameResults = new HashSet<GameResult>();
        }
    }
}
