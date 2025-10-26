using System;
using System.Collections.Generic;
using System.Linq;
using Traveller.Core.Models;

namespace Traveller.Core.Services
{
    public class MatchPointsService
    {
        public MatchPointsResultModel CalculateMatchPoints(List<int> existingScores, int newScore)
        {
            var result = new MatchPointsResultModel();

            if (existingScores == null)
                existingScores = new List<int>();

            // Add the new score to the list
            var allScores = new List<int>(existingScores) { newScore };
            
            // Group scores to handle ties
            var scoreGroups = allScores
                .GroupBy(s => s)
                .OrderByDescending(g => g.Key)
                .ToList();

            result.TotalTables = allScores.Count;
            result.MaxPossibleMatchPoints = result.TotalTables - 1;

            int position = 0;
            
            foreach (var group in scoreGroups)
            {
                int score = group.Key;
                int count = group.Count();
                
                // Calculate match points for this score
                // Each score gets 1 MP for each lower score, 0.5 MP for each tied score (excluding self)
                int scoresBelow = allScores.Count(s => s < score);
                int tiedScores = count - 1; // Exclude self from ties
                
                double matchPoints = scoresBelow + (tiedScores * 0.5);
                
                // Determine ranking description
                string ranking = GetRankingDescription(position, count, result.TotalTables);
                
                var outcome = new MatchPointsResultModel.ScoreOutcome
                {
                    Score = score,
                    MatchPoints = matchPoints,
                    Ranking = ranking,
                    IsNewScore = (score == newScore && group.Contains(newScore))
                };
                
                result.Outcomes.Add(outcome);
                
                position += count;
            }

            return result;
        }

        public List<MatchPointsResultModel.RankingOption> GetAllRankingOptions(List<int> existingScores)
        {
            if (existingScores == null || existingScores.Count == 0)
                return new List<MatchPointsResultModel.RankingOption>();

            var options = new List<MatchPointsResultModel.RankingOption>();
            var sortedScores = existingScores.OrderByDescending(s => s).Distinct().ToList();
            
            int currentRank = 1;
            
            // Add option for scores above the highest
            var highestScore = sortedScores.First();
            var aboveHighestResult = CalculateMatchPoints(existingScores, highestScore + 100);
            var aboveHighestOutcome = aboveHighestResult.Outcomes.First(o => o.IsNewScore);
            options.Add(new MatchPointsResultModel.RankingOption
            {
                ScoreDisplay = "---",
                Ranking = aboveHighestOutcome.Ranking,
                MatchPoints = aboveHighestOutcome.MatchPoints,
                IsStoredScore = false
            });

            // For each stored score and the gaps between them
            for (int i = 0; i < sortedScores.Count; i++)
            {
                var score = sortedScores[i];
                
                // Add the stored score (tied)
                var tieResult = CalculateMatchPoints(existingScores, score);
                var tieOutcome = tieResult.Outcomes.First(o => o.IsNewScore);
                options.Add(new MatchPointsResultModel.RankingOption
                {
                    ScoreDisplay = score.ToString(),
                    Ranking = tieOutcome.Ranking,
                    MatchPoints = tieOutcome.MatchPoints,
                    IsStoredScore = true
                });
                
                // Add gap before next score (if not the last score)
                if (i < sortedScores.Count - 1)
                {
                    var nextScore = sortedScores[i + 1];
                    var midScore = (score + nextScore) / 2;
                    
                    var betweenResult = CalculateMatchPoints(existingScores, midScore);
                    var betweenOutcome = betweenResult.Outcomes.First(o => o.IsNewScore);
                    options.Add(new MatchPointsResultModel.RankingOption
                    {
                        ScoreDisplay = "---",
                        Ranking = betweenOutcome.Ranking,
                        MatchPoints = betweenOutcome.MatchPoints,
                        IsStoredScore = false
                    });
                }
            }

            // Add option for scores below the lowest
            var lowestScore = sortedScores.Last();
            var belowLowestResult = CalculateMatchPoints(existingScores, lowestScore - 100);
            var belowLowestOutcome = belowLowestResult.Outcomes.First(o => o.IsNewScore);
            options.Add(new MatchPointsResultModel.RankingOption
            {
                ScoreDisplay = "---",
                Ranking = belowLowestOutcome.Ranking,
                MatchPoints = belowLowestOutcome.MatchPoints,
                IsStoredScore = false
            });

            return options;
        }

        private string GetRankingDescription(int position, int count, int totalTables)
        {
            if (position == 0)
            {
                // First place
                return count > 1 ? $"Tied for 1st ({count} tables)" : "1st";
            }
            else if (position + count == totalTables)
            {
                // Last place
                return count > 1 ? $"Tied for Last ({count} tables)" : "Last";
            }
            else
            {
                // Middle positions
                int rank = position + 1;
                string suffix = GetOrdinalSuffix(rank);
                
                if (count > 1)
                {
                    return $"Tied for {rank}{suffix} ({count} tables)";
                }
                else
                {
                    return $"{rank}{suffix}";
                }
            }
        }

        private string GetOrdinalSuffix(int number)
        {
            if (number <= 0) return "";
            
            int lastDigit = number % 10;
            int lastTwoDigits = number % 100;
            
            if (lastTwoDigits >= 11 && lastTwoDigits <= 13)
                return "th";
            
            switch (lastDigit)
            {
                case 1: return "st";
                case 2: return "nd";
                case 3: return "rd";
                default: return "th";
            }
        }
    }
}