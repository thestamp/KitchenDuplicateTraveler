using Traveler.Console.Services;
using Traveler.Core.Services;

namespace Traveler.Console
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                // Check if a file was dragged onto the executable
                if (args.Length == 0)
                {
                    System.Console.WriteLine("ERROR: No file provided.");
                    System.Console.WriteLine("Usage: Drag and drop a PBN file onto this executable.");
                    System.Console.WriteLine("\nPress any key to exit...");
                    System.Console.ReadKey();
                    return;
                }

                string inputFilePath = args[0];

                // Validate file exists
                if (!File.Exists(inputFilePath))
                {
                    System.Console.WriteLine($"ERROR: File not found: {inputFilePath}");
                    System.Console.WriteLine("\nPress any key to exit...");
                    System.Console.ReadKey();
                    return;
                }

                System.Console.WriteLine($"Processing file: {inputFilePath}");

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
                var boardData = new List<Traveler.Console.Models.GameData>();

                foreach (var game in games.OrderBy(g => g.BoardNumber))
                {
                    if (game.GameResults.Any())
                    {
                        // Calculate NS scores for all results
                        var nsScores = game.GameResults
                            .Select(r => scoringService.CalculateNorthScore(r, game.Vulnerable))
                            .ToList();

                        // Get all ranking options
                        var matchPointsOptions = matchPointsService.GetAllRankingOptions(nsScores);

                        // Create score details
                        var scoreDetails = matchPointsOptions.Select(option => new Traveler.Console.Models.GameData.ScoreDetail
                        {
                            Score = option.IsStoredScore && int.TryParse(option.ScoreDisplay, out int score) ? score : 0,
                            MatchPoints = option.MatchPoints,
                            Ranking = option.Ranking,
                            IsStoredScore = option.IsStoredScore
                        }).ToList();

                        var gameData = new Traveler.Console.Models.GameData
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