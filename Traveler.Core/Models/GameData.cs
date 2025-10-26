using Traveler.Core.Models;

namespace Traveler.Core.Models
{
    public class GameData
    {
        public class ScoreDetail
        {
            public int Score { get; set; }
            public double MatchPoints { get; set; }
            public string Ranking { get; set; } = string.Empty;
            public bool IsStoredScore { get; set; }
        }

        public GameModel GameModel { get; set; } = new GameModel();
        public List<ScoreDetail> ScoreDetails { get; set; } = new List<ScoreDetail>();
    }
}