using Traveler.Core.Models;

namespace Traveler.Core.Models
{
    public class GameData
    {
        public class ScoreDetail
        {
            public string Contract { get; set; } = string.Empty;
            public string Declarer { get; set; } = string.Empty;
            public int TricksMade { get; set; }
            public int Score { get; set; }
            public double MatchPoints { get; set; }
            public double EastWestMatchPoints { get; set; }
            public string Ranking { get; set; } = string.Empty;
            public bool IsStoredScore { get; set; } // True if this is an actual game result, false if theoretical position
        }

        public GameModel GameModel { get; set; } = new GameModel();
        public List<ScoreDetail> ScoreDetails { get; set; } = new List<ScoreDetail>();
    }
}