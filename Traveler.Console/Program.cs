using Traveler.Core.Services;
using Traveler.Core.Models;

namespace Traveler.Console
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                string inputFilePath;

                // Check if a file was provided as argument (drag and drop)
                if (args.Length == 0)
                {
                    // Prompt user for file path
                    System.Console.WriteLine("PBN File Processor");
                    System.Console.WriteLine("==================");
                    System.Console.WriteLine();
                    System.Console.Write("Enter the path to the PBN file: ");
                    inputFilePath = System.Console.ReadLine()?.Trim() ?? "";

                    // Remove quotes if user copy-pasted a path with quotes
                    if (inputFilePath.StartsWith("\"") && inputFilePath.EndsWith("\""))
                    {
                        inputFilePath = inputFilePath.Substring(1, inputFilePath.Length - 2);
                    }

                    if (string.IsNullOrWhiteSpace(inputFilePath))
                    {
                        System.Console.WriteLine("\nERROR: No file path provided.");
                        System.Console.WriteLine("\nPress any key to exit...");
                        System.Console.ReadKey();
                        return;
                    }
                }
                else
                {
                    inputFilePath = args[0];
                }

                // Validate file exists
                if (!File.Exists(inputFilePath))
                {
                    System.Console.WriteLine($"\nERROR: File not found: {inputFilePath}");
                    System.Console.WriteLine("\nPress any key to exit...");
                    System.Console.ReadKey();
                    return;
                }

                System.Console.WriteLine($"\nProcessing file: {inputFilePath}");

                // Read and parse the PBN file
                var fileContent = File.ReadAllText(inputFilePath);
                var fileImportService = new FileImportService();
                var games = fileImportService.ImportFile(fileContent);

                if (games.Count == 0)
                {
                    System.Console.WriteLine("ERROR: No game data found in file.");
                    System.Console.WriteLine("\nPress any key to exit...");
                    System.Console.ReadKey();
                    return;
                }

                System.Console.WriteLine($"Found {games.Count} board(s) to process.");

                // Calculate match points for each board
                var matchPointsService = new MatchPointsService();
                var scoringService = new BridgeScoringService();
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
                                NorthScore = scoringService.CalculateNorthScore(r, game.Vulnerable)
                            })
                            .ToList();

                        var nsScores = scoredResults.Select(sr => sr.NorthScore).ToList();

                        // Get all ranking options (includes actual scores and theoretical positions)
                        var matchPointsOptions = matchPointsService.GetAllRankingOptions(nsScores);

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
                    System.Console.WriteLine("ERROR: No boards with results found.");
                    System.Console.WriteLine("\nPress any key to exit...");
                    System.Console.ReadKey();
                    return;
                }

                // Generate PDF
                var pdfGenerator = new PdfGeneratorService();
                string outputPath = Path.Combine(
                    Path.GetDirectoryName(inputFilePath) ?? "",
                    Path.GetFileNameWithoutExtension(inputFilePath) + "_Results.pdf"
                );

                pdfGenerator.GeneratePdf(boardData, outputPath);

                System.Console.WriteLine($"\nPDF generated successfully: {outputPath}");
                System.Console.WriteLine("\nPress any key to exit...");
                System.Console.ReadKey();
            }
            catch (Exception ex)
            {
                System.Console.WriteLine($"\nERROR: {ex.Message}");
                System.Console.WriteLine($"\nStack Trace:\n{ex.StackTrace}");
                System.Console.WriteLine("\nPress any key to exit...");
                System.Console.ReadKey();
            }
        }
    }
}