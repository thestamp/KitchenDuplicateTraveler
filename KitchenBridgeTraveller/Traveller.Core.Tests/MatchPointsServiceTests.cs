using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;
using Traveller.Core.Services;

namespace Traveller.Core.Tests
{
    public class MatchPointsServiceTests
    {
        private readonly ITestOutputHelper _output;

        public MatchPointsServiceTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void CalculateMatchPoints_EmptyScores_ReturnsNewScoreAsFirst()
        {
            var service = new MatchPointsService();
            var result = service.CalculateMatchPoints(new List<int>(), 100);

            Assert.Single(result.Outcomes);
            Assert.Equal(100, result.Outcomes[0].Score);
            Assert.Equal(0, result.Outcomes[0].MatchPoints);
            Assert.Equal("1st", result.Outcomes[0].Ranking);
            Assert.True(result.Outcomes[0].IsNewScore);
            Assert.Equal(1, result.TotalTables);
            Assert.Equal(0, result.MaxPossibleMatchPoints);
        }

        [Fact]
        public void CalculateMatchPoints_SimpleScenario_CalculatesCorrectly()
        {
            var service = new MatchPointsService();
            var existingScores = new List<int> { 100, 120, 140 };
            var result = service.CalculateMatchPoints(existingScores, 130);

            Assert.Equal(4, result.TotalTables);
            Assert.Equal(3, result.MaxPossibleMatchPoints);
            Assert.Equal(4, result.Outcomes.Count);

            // Verify scores are in descending order
            var orderedScores = result.Outcomes.Select(o => o.Score).ToList();
            Assert.Equal(new List<int> { 140, 130, 120, 100 }, orderedScores);

            // Verify match points
            var score140 = result.Outcomes.First(o => o.Score == 140);
            Assert.Equal(3, score140.MatchPoints); // Beats 3 others
            Assert.Equal("1st", score140.Ranking);
            Assert.False(score140.IsNewScore);

            var score130 = result.Outcomes.First(o => o.Score == 130);
            Assert.Equal(2, score130.MatchPoints); // Beats 2 others
            Assert.Equal("2nd", score130.Ranking);
            Assert.True(score130.IsNewScore);

            var score120 = result.Outcomes.First(o => o.Score == 120);
            Assert.Equal(1, score120.MatchPoints); // Beats 1 other
            Assert.Equal("3rd", score120.Ranking);
            Assert.False(score120.IsNewScore);

            var score100 = result.Outcomes.First(o => o.Score == 100);
            Assert.Equal(0, score100.MatchPoints); // Beats none
            Assert.Equal("Last", score100.Ranking);
            Assert.False(score100.IsNewScore);
        }

        [Fact]
        public void CalculateMatchPoints_TieScenario_AveragesMatchPoints()
        {
            var service = new MatchPointsService();
            var existingScores = new List<int> { 100, 120, 140 };
            var result = service.CalculateMatchPoints(existingScores, 140);

            // Two scores of 140 should tie
            var score140Outcomes = result.Outcomes.Where(o => o.Score == 140).ToList();
            Assert.Single(score140Outcomes); // Should be grouped
            
            var score140 = score140Outcomes[0];
            Assert.Equal(2.5, score140.MatchPoints); // (2 + 3) / 2 = 2.5
            Assert.Equal("Tied for 1st (2 tables)", score140.Ranking);
        }

        [Fact]
        public void CalculateMatchPoints_MultipleTies_CalculatesCorrectly()
        {
            var service = new MatchPointsService();
            var existingScores = new List<int> { 100, 100, 120, 140 };
            var result = service.CalculateMatchPoints(existingScores, 100);

            Assert.Equal(5, result.TotalTables);

            // Three scores of 100 (tied for last)
            var score100 = result.Outcomes.First(o => o.Score == 100);
            Assert.Equal(1, score100.MatchPoints); // (0 + 1 + 2) / 3 = 1
            Assert.Equal("Tied for Last (3 tables)", score100.Ranking);

            // Single score of 120
            var score120 = result.Outcomes.First(o => o.Score == 120);
            Assert.Equal(3, score120.MatchPoints); // Beats 3 others
            Assert.Equal("2nd", score120.Ranking);

            // Single score of 140
            var score140 = result.Outcomes.First(o => o.Score == 140);
            Assert.Equal(4, score140.MatchPoints); // Beats 4 others
            Assert.Equal("1st", score140.Ranking);
        }

        [Fact]
        public void CalculateMatchPoints_AllTied_AllGetSameMatchPoints()
        {
            var service = new MatchPointsService();
            var existingScores = new List<int> { 100, 100, 100 };
            var result = service.CalculateMatchPoints(existingScores, 100);

            Assert.Equal(4, result.TotalTables);
            Assert.Single(result.Outcomes);

            var outcome = result.Outcomes[0];
            Assert.Equal(100, outcome.Score);
            Assert.Equal(1.5, outcome.MatchPoints); // (0 + 1 + 2 + 3) / 4 = 1.5
            Assert.Equal("Tied for 1st (4 tables)", outcome.Ranking);
        }

        [Fact]
        public void CalculateMatchPoints_TiedForMiddle_CalculatesCorrectly()
        {
            var service = new MatchPointsService();
            var existingScores = new List<int> { 80, 100, 120, 140 };
            var result = service.CalculateMatchPoints(existingScores, 100);

            // Two scores of 100 (tied for 3rd/4th out of 5)
            var score100 = result.Outcomes.First(o => o.Score == 100);
            Assert.Equal(1.5, score100.MatchPoints); // Beats 1 (80), ties with 1 other 100: 1 + 0.5 = 1.5
            Assert.Equal("Tied for 3rd (2 tables)", score100.Ranking);
        }

        [Fact]
        public void CalculateMatchPoints_NewScoreIsHighest_BeatsAll()
        {
            var service = new MatchPointsService();
            var existingScores = new List<int> { 100, 120, 140 };
            var result = service.CalculateMatchPoints(existingScores, 200);

            var score200 = result.Outcomes.First(o => o.Score == 200);
            Assert.Equal(3, score200.MatchPoints);
            Assert.Equal("1st", score200.Ranking);
            Assert.True(score200.IsNewScore);
        }

        [Fact]
        public void CalculateMatchPoints_NewScoreIsLowest_BeatsNone()
        {
            var service = new MatchPointsService();
            var existingScores = new List<int> { 100, 120, 140 };
            var result = service.CalculateMatchPoints(existingScores, 50);

            var score50 = result.Outcomes.First(o => o.Score == 50);
            Assert.Equal(0, score50.MatchPoints);
            Assert.Equal("Last", score50.Ranking);
            Assert.True(score50.IsNewScore);
        }

        [Fact]
        public void CalculateMatchPoints_ComplexTieScenario_CalculatesCorrectly()
        {
            var service = new MatchPointsService();
            var existingScores = new List<int> { 100, 100, 120, 140, 140, 140 };
            var result = service.CalculateMatchPoints(existingScores, 120);

            _output.WriteLine($"Total Tables: {result.TotalTables}");
            _output.WriteLine($"Max Possible MP: {result.MaxPossibleMatchPoints}");
            _output.WriteLine("");
            
            foreach (var outcome in result.Outcomes)
            {
                _output.WriteLine($"Score: {outcome.Score,4} | MP: {outcome.MatchPoints,4} | Ranking: {outcome.Ranking,-25} | New: {outcome.IsNewScore}");
            }

            Assert.Equal(7, result.TotalTables);

            // Three 140s (tied for 1st)
            var score140 = result.Outcomes.First(o => o.Score == 140);
            Assert.Equal(5, score140.MatchPoints); // Beats 4, ties with 2: 4 + 1 = 5
            Assert.Equal("Tied for 1st (3 tables)", score140.Ranking);

            // Two 120s (tied for 4th)
            var score120 = result.Outcomes.First(o => o.Score == 120);
            Assert.Equal(2.5, score120.MatchPoints); // Beats 2, ties with 1: 2 + 0.5 = 2.5
            Assert.Equal("Tied for 4th (2 tables)", score120.Ranking);

            // Two 100s (tied for last)
            var score100 = result.Outcomes.First(o => o.Score == 100);
            Assert.Equal(0.5, score100.MatchPoints); // Beats 0, ties with 1: 0 + 0.5 = 0.5
            Assert.Equal("Tied for Last (2 tables)", score100.Ranking);
        }

        [Fact]
        public void CalculateMatchPoints_PrintsFormattedResults()
        {
            var service = new MatchPointsService();
            var existingScores = new List<int> { 100, 120, 140 };
            var result = service.CalculateMatchPoints(existingScores, 130);

            _output.WriteLine("");
            _output.WriteLine("═══════════════════════════════════════════════════════════");
            _output.WriteLine("  MATCH POINTS CALCULATION");
            _output.WriteLine("═══════════════════════════════════════════════════════════");
            _output.WriteLine($"  Total Tables: {result.TotalTables}");
            _output.WriteLine($"  Max Possible Match Points: {result.MaxPossibleMatchPoints}");
            _output.WriteLine("");
            _output.WriteLine("───────────────────────────────────────────────────────────");
            _output.WriteLine($"  {"Score",-8} {"Match Pts",-12} {"Ranking",-25} {"New"}");
            _output.WriteLine("───────────────────────────────────────────────────────────");
            
            foreach (var outcome in result.Outcomes)
            {
                var newMarker = outcome.IsNewScore ? "◄" : "";
                _output.WriteLine($"  {outcome.Score,-8} {outcome.MatchPoints,-12:F1} {outcome.Ranking,-25} {newMarker}");
            }
            
            _output.WriteLine("═══════════════════════════════════════════════════════════");
            _output.WriteLine("");
        }

        [Fact]
        public void CalculateMatchPoints_NewEntry_EvenDistribution_ShowsByRanking()
        {
            var service = new MatchPointsService();
            var baseScores = new List<int> { 100, 120, 140, 160, 180 };
            
            var options = service.GetAllRankingOptions(baseScores);
            
            _output.WriteLine("");
            _output.WriteLine("═══════════════════════════════════════════════════════════════════════════════");
            _output.WriteLine("  MATCH POINTS AWARDED - SCORE SET: {100, 120, 140, 160, 180}");
            _output.WriteLine("═══════════════════════════════════════════════════════════════════════════════");
            _output.WriteLine("");
            _output.WriteLine($"  {"Score",-10} {"Ranking",-30} {"MP"}");
            _output.WriteLine("───────────────────────────────────────────────────────────────────────────────");

            foreach (var option in options)
            {
                _output.WriteLine($"  {option.ScoreDisplay,-10} {option.Ranking,-30} {option.MatchPoints:F1}");
            }

            _output.WriteLine("═══════════════════════════════════════════════════════════════════════════════");
            _output.WriteLine("");
            
            // Verify we have all expected options
            Assert.NotEmpty(options);
        }

        [Fact]
        public void CalculateMatchPoints_NewEntry_WithTies_ShowsByRanking()
        {
            var service = new MatchPointsService();
            var baseScores = new List<int> { 100, 120, 140, 140, 140, 160, 180, 180 };
            
            var options = service.GetAllRankingOptions(baseScores);
            
            _output.WriteLine("");
            _output.WriteLine("═══════════════════════════════════════════════════════════════════════════════");
            _output.WriteLine("  MATCH POINTS AWARDED - SCORE SET: {100, 120, 140, 140, 140, 160, 180, 180}");
            _output.WriteLine("═══════════════════════════════════════════════════════════════════════════════");
            _output.WriteLine("");
            _output.WriteLine($"  {"Score",-10} {"Ranking",-35} {"MP"}");
            _output.WriteLine("───────────────────────────────────────────────────────────────────────────────");

            foreach (var option in options)
            {
                _output.WriteLine($"  {option.ScoreDisplay,-10} {option.Ranking,-35} {option.MatchPoints:F1}");
            }

            _output.WriteLine("═══════════════════════════════════════════════════════════════════════════════");
            _output.WriteLine("");
            
            // Verify we have all expected options
            Assert.NotEmpty(options);
        }
    }
}