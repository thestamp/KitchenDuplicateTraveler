using Traveler.Core.Models;
using Traveler.Core.Services;
using Traveler.Wasm.Client.Models;
using System.Net.Http;
using System.Text.Json;
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

        public async Task<List<BridgeWebsTournament>> FetchBridgeWebsTournamentsAsync()
        {
            const string apiUrl = "https://www.bridgewebs.com/cgi-bin/bwor/bw.cgi?xml=1&club=bw&pid=xml_elog&mod=EventLog&rand=0.3640787998041133";
            
            try
            {
                var response = await _httpClient.GetAsync(apiUrl);
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                
                var jsonDoc = JsonDocument.Parse(content);
                var tournaments = new List<BridgeWebsTournament>();
                
                if (jsonDoc.RootElement.TryGetProperty("events", out var eventsArray))
                {
                    foreach (var eventElement in eventsArray.EnumerateArray())
                    {
                        var eventStr = eventElement.GetString();
                        if (string.IsNullOrEmpty(eventStr))
                            continue;
                        
                        var parts = eventStr.Split('|');
                        if (parts.Length >= 7)
                        {
                            var hasResults = parts[0] == "1";
                            
                            // Only include tournaments that start with "1" (have results)
                            if (hasResults)
                            {
                                tournaments.Add(new BridgeWebsTournament
                                {
                                    HasResults = hasResults,
                                    ClubId = parts[2],
                                    TimeAgo = parts[3],
                                    EventId = parts[5],
                                    ClubName = parts[6],
                                    EventName = parts.Length > 7 ? parts[7] : ""
                                });
                            }
                        }
                    }
                }
                
                return tournaments;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to fetch BridgeWebs tournaments: {ex.Message}", ex);
            }
        }

        public async Task<bool> ValidatePbnUrlAsync(BridgeWebsTournament tournament)
        {
            try
            {
                var pbnUrl = tournament.GetPbnUrl();
                var response = await _httpClient.GetAsync(pbnUrl);
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                
                // Basic validation - check if content looks like PBN
                if (string.IsNullOrWhiteSpace(content))
                {
                    tournament.ValidationError = "Empty file";
                    return false;
                }

                // Check for basic PBN structure
                if (!content.Contains("[Event") && !content.Contains("[Board"))
                {
                    tournament.ValidationError = "Not a valid PBN file";
                    return false;
                }

                // Try to parse it and check for boards with results
                var games = _fileImportService.ImportFile(content);
                if (games.Count == 0)
                {
                    tournament.ValidationError = "No games found in file";
                    return false;
                }

                // Check if any boards have results
                var boardsWithResults = games.Count(g => g.GameResults.Any());
                if (boardsWithResults == 0)
                {
                    tournament.ValidationError = "No boards with results found";
                    return false;
                }

                // Success - set a helpful message
                tournament.ValidationError = $"Valid: {boardsWithResults} board(s) with results";
                return true;
            }
            catch (HttpRequestException ex)
            {
                tournament.ValidationError = "File not found (404)";
                return false;
            }
            catch (Exception ex)
            {
                tournament.ValidationError = ex.Message.Length > 80 ? ex.Message.Substring(0, 77) + "..." : ex.Message;
                return false;
            }
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