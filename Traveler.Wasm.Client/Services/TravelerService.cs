using Traveler.Core.Models;
using Traveler.Core.Services;
using System.Net.Http;
using System.Threading.Tasks;

namespace Traveler.Wasm.Client.Services
{
    public class TravelerService
    {
        private readonly HttpClient _httpClient;
        private readonly FileImportService _fileImportService;
        private readonly BridgeScoringService _scoringService;
        private readonly MatchPointsService _matchPointsService;

        public TravelerService(
            HttpClient httpClient,
            FileImportService fileImportService,
            BridgeScoringService scoringService,
            MatchPointsService matchPointsService)
        {
            _httpClient = httpClient;
            _fileImportService = fileImportService;
            _scoringService = scoringService;
            _matchPointsService = matchPointsService;
        }

        public async Task<List<GameData>> ProcessPbnContentAsync(string pbnContent)
        {
            return await Task.Run(() => ProcessPbnContent(pbnContent));
        }

        public async Task<List<GameData>> ProcessPbnFromUrlAsync(string url)
        {
            try
            {
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                return await ProcessPbnContentAsync(content);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to download PBN file from URL: {ex.Message}", ex);
            }
        }

        private List<GameData> ProcessPbnContent(string fileContent)
        {
            var games = _fileImportService.ImportFile(fileContent);

            if (games.Count == 0)
            {
                throw new Exception("No game data found in file.");
            }

            var boardData = new List<GameData>();

            foreach (var game in games.OrderBy(g => g.BoardNumber))
            {
                if (game.GameResults.Any())
                {
                    // Calculate NS scores for all results
                    var scoredResults = game.GameResults
                        .Select(r => new
                        {
                            GameResult = r,
                            NorthScore = _scoringService.CalculateNorthScore(r, game.Vulnerable)
                        })
                        .ToList();

                    var nsScores = scoredResults.Select(sr => sr.NorthScore).ToList();

                    // Get all ranking options (includes actual scores and theoretical positions)
                    var matchPointsOptions = _matchPointsService.GetAllRankingOptions(nsScores);

                    // Calculate the maximum possible match points
                    double maxMatchPoints = nsScores.Count;

                    // Create score details from match points options
                    var scoreDetails = matchPointsOptions.Select(option =>
                    {
                        var detail = new GameData.ScoreDetail
                        {
                            MatchPoints = option.MatchPoints,
                            EastWestMatchPoints = maxMatchPoints - option.MatchPoints,
                            Ranking = option.Ranking,
                            IsStoredScore = option.IsStoredScore
                        };

                        if (option.IsStoredScore && int.TryParse(option.ScoreDisplay, out int score))
                        {
                            detail.Score = score;

                            // Find the first game result with this score
                            var matchingResult = scoredResults.FirstOrDefault(sr => sr.NorthScore == score);
                            if (matchingResult != null)
                            {
                                detail.Contract = matchingResult.GameResult.Contract ?? "";
                                var declarerStr = matchingResult.GameResult.Declarer.ToString() ?? "";
                                detail.Declarer = !string.IsNullOrEmpty(declarerStr) ? declarerStr.Substring(0, 1) : "";
                                detail.TricksMade = matchingResult.GameResult.Result;
                            }
                        }
                        else
                        {
                            detail.Score = 0;
                            detail.Contract = "";
                            detail.Declarer = "";
                            detail.TricksMade = 0;
                        }

                        return detail;
                    }).ToList();

                    var gameData = new GameData
                    {
                        GameModel = game,
                        ScoreDetails = scoreDetails
                    };

                    boardData.Add(gameData);
                }
            }

            if (boardData.Count == 0)
            {
                throw new Exception("No boards with results found.");
            }

            return boardData;
        }
    }
}