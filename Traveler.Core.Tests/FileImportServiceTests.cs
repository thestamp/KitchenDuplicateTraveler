using Traveler.Core.Models;
using Traveler.Core.Parsers;
using Traveler.Core.Services;
using Xunit.Abstractions;

namespace Traveler.Core.Tests
{
    public class FileImportServiceTests
    {
        private readonly ITestOutputHelper _output;
        private readonly BridgeScoringService _scoringService;

        public FileImportServiceTests(ITestOutputHelper output)
        {
            _output = output;
            _scoringService = new BridgeScoringService();
        }

        [Fact]
        public void ImportFile_EmptyContent_ReturnsEmptySet()
        {
            var service = new FileImportService();
            var result = service.ImportFile("");

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public void ImportFile_SingleBoard_ParsesCorrectly()
        {
            var pbnContent = @"% PBN 2.1
[Event ""Test Event""]
[Site ""Test Site""]
[Date ""2023.10.09""]
[Board ""1""]
[Dealer ""N""]
[Vulnerable ""None""]
[Deal ""N:Q87.96.KQ854.T98 K5.Q875.JT7.K653 A.KJT4.A96.AQJ74 JT96432.A32.32.2""]
[ScoreTable ""PairId_NS\4R;PairId_EW\4R;Contract\5L;Declarer\1R;Result\2R""]
1    1    3NT   S 12
2    3    3S    W  7
";

            var service = new FileImportService();
            var result = service.ImportFile(pbnContent);

            Assert.Single(result);
            var game = result.First();
            Assert.Equal("Test Event", game.Event);
            Assert.Equal("Test Site", game.Site);
            Assert.Equal("2023.10.09", game.Date);
            Assert.Equal(1, game.BoardNumber);
            Assert.Equal(GameModel.Player.North, game.Dealer);
            Assert.Equal("None", game.Vulnerable);
            Assert.Equal(4, game.PlayerHands.Count);
            Assert.Equal(2, game.GameResults.Count);
        }

        [Fact]
        public void ParseDeal_ValidDealString_ParsesAllHands()
        {
            var dealString = "N:Q87.96.KQ854.T98 K5.Q875.JT7.K653 A.KJT4.A96.AQJ74 JT96432.A32.32.2";
            var hands = PbnParser.ParseDeal(dealString);

            Assert.Equal(4, hands.Count);
            Assert.True(hands.ContainsKey(GameModel.Player.North));
            Assert.True(hands.ContainsKey(GameModel.Player.East));
            Assert.True(hands.ContainsKey(GameModel.Player.South));
            Assert.True(hands.ContainsKey(GameModel.Player.West));
            Assert.Equal("Q87.96.KQ854.T98", hands[GameModel.Player.North]);
            Assert.Equal("K5.Q875.JT7.K653", hands[GameModel.Player.East]);
        }

        [Fact]
        public void ParseDeal_DifferentDealer_ParsesCorrectly()
        {
            var dealString = "E:A754.T8.AKJ64.85 83.AK964.95.J942 K62.QJ752.87.Q63 QJT9.3.QT32.AKT7";
            var hands = PbnParser.ParseDeal(dealString);

            Assert.Equal(4, hands.Count);
            Assert.Equal("A754.T8.AKJ64.85", hands[GameModel.Player.East]);
            Assert.Equal("83.AK964.95.J942", hands[GameModel.Player.South]);
            Assert.Equal("K62.QJ752.87.Q63", hands[GameModel.Player.West]);
            Assert.Equal("QJT9.3.QT32.AKT7", hands[GameModel.Player.North]);
        }

        [Fact]
        public void ParseScoreTable_ValidEntries_ParsesCorrectly()
        {
            var scoreLines = new List<string>
            {
                "PairId_NS\\4R;PairId_EW\\4R;Contract\\5L;Declarer\\1R;Result\\2R",
                "1    1    3NT   S 12",
                "2    3    3S    W  7",
                "3    5    4H    S  7"
            };

            var results = PbnParser.ParseScoreTable(scoreLines);

            Assert.Equal(3, results.Count);
            Assert.Equal(1, results[0].PairIdNS);
            Assert.Equal(1, results[0].PairIdEW);
            Assert.Equal("3NT", results[0].Contract);
            Assert.Equal(GameModel.Player.South, results[0].Declarer);
            Assert.Equal(12, results[0].Result);
        }

        [Fact]
        public void ParseScoreTable_ContractsWithDoubles_ParsesCorrectly()
        {
            var scoreLines = new List<string>
            {
                "5    2    3Sx   W  8",
                "1    6    5Dxx  S 11",
                "2    4    4Cx   S  8"
            };

            var results = PbnParser.ParseScoreTable(scoreLines);

            Assert.Equal(3, results.Count);
            Assert.Equal("3Sx", results[0].Contract);
            Assert.Equal("5Dxx", results[1].Contract);
            Assert.Equal("4Cx", results[2].Contract);
        }

        [Fact]
        public void ParsePlayer_AllVariations_ParsesCorrectly()
        {
            Assert.Equal(GameModel.Player.North, PbnParser.ParsePlayer("N"));
            Assert.Equal(GameModel.Player.East, PbnParser.ParsePlayer("E"));
            Assert.Equal(GameModel.Player.South, PbnParser.ParsePlayer("S"));
            Assert.Equal(GameModel.Player.West, PbnParser.ParsePlayer("W"));
            Assert.Equal(GameModel.Player.North, PbnParser.ParsePlayer("NORTH"));
            Assert.Equal(GameModel.Player.East, PbnParser.ParsePlayer("east"));
        }

        [Fact]
        public void ImportFile_MultipleBoards_ParsesAll()
        {
            var pbnContent = @"% PBN 2.1
[Event ""Event 1""]
[Board ""1""]
[Dealer ""N""]
[Vulnerable ""None""]
[Deal ""N:Q87.96.KQ854.T98 K5.Q875.JT7.K653 A.KJT4.A96.AQJ74 JT96432.A32.32.2""]

[Event ""Event 2""]
[Board ""2""]
[Dealer ""E""]
[Vulnerable ""NS""]
[Deal ""E:A754.T8.AKJ64.85 83.AK964.95.J942 K62.QJ752.87.Q63 QJT9.3.QT32.AKT7""]
";

            var service = new FileImportService();
            var result = service.ImportFile(pbnContent);

            Assert.Equal(2, result.Count);
            Assert.Contains(result, g => g.BoardNumber == 1);
            Assert.Contains(result, g => g.BoardNumber == 2);
        }

        //[Fact]
        //public void ImportFile_TestPbnFile_ParsesAndPrintsAll()
        //{
        //    // Read the test.pbn file
        //    var testFilePath = Path.Combine("", "test.pbn");
            
        //    if (!File.Exists(testFilePath))
        //    {
        //        _output.WriteLine($"Test file not found at: {testFilePath}");
        //        Assert.True(File.Exists(testFilePath), "test.pbn file should exist in Files directory");
        //        return;
        //    }

        //    var fileContent = File.ReadAllText(testFilePath);
        //    var service = new FileImportService();
        //    var games = service.ImportFile(fileContent);

        //    Assert.NotEmpty(games);
        //    _output.WriteLine($"Total boards parsed: {games.Count}");
        //    _output.WriteLine("");

        //    foreach (var game in games.OrderBy(g => g.BoardNumber))
        //    {
        //        _output.WriteLine($"===== BOARD {game.BoardNumber} =====");
        //        _output.WriteLine($"Event: {game.Event}");
        //        _output.WriteLine($"Site: {game.Site}");
        //        _output.WriteLine($"Date: {game.Date}");
        //        _output.WriteLine($"Dealer: {game.Dealer}");
        //        _output.WriteLine($"Vulnerable: {game.Vulnerable}");
        //        _output.WriteLine("");
        //        _output.WriteLine("Hands:");
        //        foreach (var hand in game.PlayerHands.OrderBy(h => h.Key))
        //        {
        //            _output.WriteLine($"  {hand.Key}: {hand.Value}");
        //        }
        //        _output.WriteLine("");
        //        _output.WriteLine("Results:");
        //        foreach (var result in game.GameResults.OrderBy(r => r.PairIdNS))
        //        {
        //            _output.WriteLine($"  NS Pair {result.PairIdNS} vs EW Pair {result.PairIdEW}: {result.Contract} by {result.Declarer}, Result: {result.Result}");
        //        }
        //        _output.WriteLine("");
        //    }

        //    // Verify specific board counts
        //    Assert.Equal(28, games.Count);
            
        //    // Verify first board
        //    var board1 = games.FirstOrDefault(g => g.BoardNumber == 1);
        //    Assert.NotNull(board1);
        //    Assert.Equal("Duplicate", board1.Event);
        //    Assert.Equal("Highcliffe Duplicate Bridge Club", board1.Site);
        //    Assert.Equal("2023.10.09", board1.Date);
        //    Assert.Equal(GameModel.Player.North, board1.Dealer);
        //    Assert.Equal("None", board1.Vulnerable);
        //    Assert.Equal(7, board1.GameResults.Count);
        //}

        [Fact]
        public void ImportFile_VulnerableVariations_ParsesCorrectly()
        {
            var pbnContent = @"
[Event ""Test""]
[Board ""1""]
[Vulnerable ""None""]
[Deal ""N:Q87.96.KQ854.T98 K5.Q875.JT7.K653 A.KJT4.A96.AQJ74 JT96432.A32.32.2""]

[Event ""Test""]
[Board ""2""]
[Vulnerable ""NS""]
[Deal ""N:Q87.96.KQ854.T98 K5.Q875.JT7.K653 A.KJT4.A96.AQJ74 JT96432.A32.32.2""]

[Event ""Test""]
[Board ""3""]
[Vulnerable ""EW""]
[Deal ""N:Q87.96.KQ854.T98 K5.Q875.JT7.K653 A.KJT4.A96.AQJ74 JT96432.A32.32.2""]

[Event ""Test""]
[Board ""4""]
[Vulnerable ""All""]
[Deal ""N:Q87.96.KQ854.T98 K5.Q875.JT7.K653 A.KJT4.A96.AQJ74 JT96432.A32.32.2""]
";

            var service = new FileImportService();
            var result = service.ImportFile(pbnContent);

            Assert.Equal(4, result.Count);
            Assert.Contains(result, g => g.Vulnerable == "None");
            Assert.Contains(result, g => g.Vulnerable == "NS");
            Assert.Contains(result, g => g.Vulnerable == "EW");
            Assert.Contains(result, g => g.Vulnerable == "All");
        }

        [Fact]
        public void ImportFile_AllDealers_ParsesCorrectly()
        {
            var pbnContent = @"
[Event ""Test""]
[Board ""1""]
[Dealer ""N""]
[Deal ""N:Q87.96.KQ854.T98 K5.Q875.JT7.K653 A.KJT4.A96.AQJ74 JT96432.A32.32.2""]

[Event ""Test""]
[Board ""2""]
[Dealer ""E""]
[Deal ""E:Q87.96.KQ854.T98 K5.Q875.JT7.K653 A.KJT4.A96.AQJ74 JT96432.A32.32.2""]

[Event ""Test""]
[Board ""3""]
[Dealer ""S""]
[Deal ""S:Q87.96.KQ854.T98 K5.Q875.JT7.K653 A.KJT4.A96.AQJ74 JT96432.A32.32.2""]

[Event ""Test""]
[Board ""4""]
[Dealer ""W""]
[Deal ""W:Q87.96.KQ854.T98 K5.Q875.JT7.K653 A.KJT4.A96.AQJ74 JT96432.A32.32.2""]
";

            var service = new FileImportService();
            var result = service.ImportFile(pbnContent);

            Assert.Equal(4, result.Count);
            Assert.Contains(result, g => g.Dealer == GameModel.Player.North);
            Assert.Contains(result, g => g.Dealer == GameModel.Player.East);
            Assert.Contains(result, g => g.Dealer == GameModel.Player.South);
            Assert.Contains(result, g => g.Dealer == GameModel.Player.West);
        }

        //[Fact]
        //public void ImportFile_TestPbnFile_FormattedHandsAndScores()
        //{
        //    // Read the test.pbn file
        //    var testFilePath = Path.Combine("", "test.pbn");
            
        //    if (!File.Exists(testFilePath))
        //    {
        //        _output.WriteLine($"Test file not found at: {testFilePath}");
        //        Assert.True(File.Exists(testFilePath), "test.pbn file should exist");
        //        return;
        //    }

        //    var fileContent = File.ReadAllText(testFilePath);
        //    var service = new FileImportService();
        //    var games = service.ImportFile(fileContent);

        //    Assert.NotEmpty(games);

        //    foreach (var game in games.OrderBy(g => g.BoardNumber))
        //    {
        //        _output.WriteLine($"");
        //        _output.WriteLine($"╔═══════════════════════════════════════════════════════════════════════════════╗");
        //        _output.WriteLine($"║ BOARD {game.BoardNumber,-2}  Dealer: {game.Dealer,-5}  Vulnerable: {game.Vulnerable,-10}                     ║");
        //        _output.WriteLine($"╚═══════════════════════════════════════════════════════════════════════════════╝");
        //        _output.WriteLine($"");
                
        //        // Display hands formatted nicely
        //        _output.WriteLine("HANDS:");
        //        _output.WriteLine("──────────────────────────────────────────────────────────────────────────────────");
        //        foreach (var player in new[] { GameModel.Player.North, GameModel.Player.East, GameModel.Player.South, GameModel.Player.West })
        //        {
        //            if (game.PlayerHands.ContainsKey(player))
        //            {
        //                _output.WriteLine($"{FormatPlayerName(player)}: {FormatHand(game.PlayerHands[player])}");
        //            }
        //        }
        //        _output.WriteLine($"");
                
        //        // Calculate and display scores
        //        if (game.GameResults.Any())
        //        {
        //            _output.WriteLine("TRAVELLER SCORES:");
        //            _output.WriteLine("──────────────────────────────────────────────────────────────────────────────────");
        //            _output.WriteLine($"{"Contract",-10} {"Declarer",-9} {"Lead",-6} {"NS Score",10}");
        //            _output.WriteLine("──────────────────────────────────────────────────────────────────────────────────");
                    
        //            var sortedResults = game.GameResults
        //                .Select(r => new
        //                {
        //                    Result = r,
        //                    NorthScore = CalculateNorthScore(r, game.Vulnerable)
        //                })
        //                .OrderByDescending(x => x.NorthScore)
        //                .ToList();
                    
        //            foreach (var item in sortedResults)
        //            {
        //                var result = item.Result;
        //                var score = item.NorthScore;
        //                var scoreDisplay = score >= 0 ? $"+{score}" : $"{score}";
        //                var lead = string.IsNullOrWhiteSpace(result.CardLead) ? "N/A" : result.CardLead;
                        
        //                _output.WriteLine($"{result.Contract,-10} {result.Declarer,-9} {lead,-6} {scoreDisplay,10}");
        //            }
        //            _output.WriteLine($"");
        //        }
        //    }
        //}

        //[Fact]
        //public void ImportFile_TestPbnFile_FormattedHandsAndMatchPoints()
        //{
        //    // Read the test.pbn file
        //    var testFilePath = Path.Combine("", "test.pbn");
            
        //    if (!File.Exists(testFilePath))
        //    {
        //        _output.WriteLine($"Test file not found at: {testFilePath}");
        //        Assert.True(File.Exists(testFilePath), "test.pbn file should exist");
        //        return;
        //    }

        //    var fileContent = File.ReadAllText(testFilePath);
        //    var fileImportService = new FileImportService();
        //    var matchPointsService = new MatchPointsService();
        //    var games = fileImportService.ImportFile(fileContent);

        //    Assert.NotEmpty(games);

        //    foreach (var game in games.OrderBy(g => g.BoardNumber))
        //    {
        //        _output.WriteLine($"");
        //        _output.WriteLine($"╔═══════════════════════════════════════════════════════════════════════════════╗");
        //        _output.WriteLine($"║ BOARD {game.BoardNumber,-2}  Dealer: {game.Dealer,-5}  Vulnerable: {game.Vulnerable,-10}                     ║");
        //        _output.WriteLine($"╚═══════════════════════════════════════════════════════════════════════════════╝");
        //        _output.WriteLine($"");
                
        //        // Display hands formatted nicely
        //        _output.WriteLine("HANDS:");
        //        _output.WriteLine("──────────────────────────────────────────────────────────────────────────────────");
        //        foreach (var player in new[] { GameModel.Player.North, GameModel.Player.East, GameModel.Player.South, GameModel.Player.West })
        //        {
        //            if (game.PlayerHands.ContainsKey(player))
        //            {
        //                _output.WriteLine($"{FormatPlayerName(player)}: {FormatHand(game.PlayerHands[player])}");
        //            }
        //        }
        //        _output.WriteLine($"");
                
        //        // Calculate and display match points
        //        if (game.GameResults.Any())
        //        {
        //            // Calculate NS scores for all results
        //            var nsScores = game.GameResults
        //                .Select(r => CalculateNorthScore(r, game.Vulnerable))
        //                .ToList();
                    
        //            // Get all ranking options
        //            var options = matchPointsService.GetAllRankingOptions(nsScores);
                    
        //            _output.WriteLine("MATCH POINTS AWARDED:");
        //            _output.WriteLine("──────────────────────────────────────────────────────────────────────────────────");
        //            _output.WriteLine($"  {"Score",-10} {"Ranking",-35} {"MP"}");
        //            _output.WriteLine("──────────────────────────────────────────────────────────────────────────────────");

        //            foreach (var option in options)
        //            {
        //                _output.WriteLine($"  {option.ScoreDisplay,-10} {option.Ranking,-35} {option.MatchPoints:F1}");
        //            }
                    
        //            _output.WriteLine($"");
        //        }
        //    }
        //}

        private string FormatPlayerName(GameModel.Player player)
        {
            return $"{player,-5}";
        }

        private string FormatHand(string handString)
        {
            if (string.IsNullOrWhiteSpace(handString))
                return "";
            
            // Hand format is: Spades.Hearts.Diamonds.Clubs
            var suits = handString.Split('.');
            if (suits.Length != 4)
                return handString;
            
            // Use Unicode suit symbols
            return $"♠{suits[0],-13} ♥{suits[1],-13} ♦{suits[2],-13} ♣{suits[3]}";
        }

        private int CalculateNorthScore(GameModel.GameResult result, string vulnerable)
        {
            return _scoringService.CalculateNorthScore(result, vulnerable);
        }
    }
}