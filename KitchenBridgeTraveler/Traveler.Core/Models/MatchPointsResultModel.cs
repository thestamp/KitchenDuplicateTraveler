namespace Traveler.Core.Models
{
    public class MatchPointsResultModel
    {
        public class ScoreOutcome
        {
            public int Score { get; set; }
            public double MatchPoints { get; set; }
            public string Ranking { get; set; }
            public bool IsNewScore { get; set; }
        }

        public class RankingOption
        {
            public string ScoreDisplay { get; set; }
            public string Ranking { get; set; }
            public double MatchPoints { get; set; }
            public bool IsStoredScore { get; set; }
        }

        public List<ScoreOutcome> Outcomes { get; set; }
        public int TotalTables { get; set; }
        public double MaxPossibleMatchPoints { get; set; }

        public MatchPointsResultModel()
        {
            Outcomes = new List<ScoreOutcome>();
        }
    }
}