using System.Net.Http;
using Traveler.Core.Models;

namespace Traveler.Core.Services
{
    public class PbnProcessingService
    {
        private readonly HttpClient _httpClient;
        private readonly FileImportService _fileImportService;
        private readonly BridgeScoringService _scoringService;
        private readonly MatchPointsService _matchPointsService;
        private readonly PdfGeneratorService _pdfGeneratorService;

        public PbnProcessingService(
            HttpClient httpClient,
            FileImportService fileImportService,
            BridgeScoringService scoringService,
            MatchPointsService matchPointsService,
            PdfGeneratorService pdfGeneratorService)
        {
            _httpClient = httpClient;
            _fileImportService = fileImportService;
            _scoringService = scoringService;
            _matchPointsService = matchPointsService;
            _pdfGeneratorService = pdfGeneratorService;
        }

        public async Task<string> DownloadPbnFromUrlAsync(string url)
        {
            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<byte[]> ProcessPbnAndGeneratePdfAsync(string fileContent)
        {
            return await Task.Run(() =>
            {
                // Parse the PBN file
                var games = _fileImportService.ImportFile(fileContent);

                if (games.Count == 0)
                {
                    throw new InvalidOperationException("No game data found in file.");
                }

                // Calculate match points for each board
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

                        // Create score details from match points options
                        var scoreDetails = matchPointsOptions.Select(option =>
                        {
                            var detail = new GameData.ScoreDetail
                            {
                                MatchPoints = option.MatchPoints,
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
                    throw new InvalidOperationException("No boards with results found.");
                }

                // Generate PDF to memory stream
                using var ms = new MemoryStream();
                var tempFile = Path.GetTempFileName();
                try
                {
                    _pdfGeneratorService.GeneratePdf(boardData, tempFile);
                    return File.ReadAllBytes(tempFile);
                }
                finally
                {
                    if (File.Exists(tempFile))
                    {
                        File.Delete(tempFile);
                    }
                }
            });
        }
    }
}