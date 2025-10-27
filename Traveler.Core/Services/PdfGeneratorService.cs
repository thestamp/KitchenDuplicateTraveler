using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Traveler.Core.Models;

namespace Traveler.Core.Services
{
    public class PdfGeneratorService
    {
        public PdfGeneratorService()
        {
            // Set QuestPDF license (Community license for free use)
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public void GeneratePdf(List<GameData> boards, string outputPath)
        {
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.Letter.Landscape());
                    page.Margin(0.5f, Unit.Inch);
                    page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Courier New"));

                    page.Content().Column(column =>
                    {
                        // Process boards 4 per page (2x2 grid)
                        for (int i = 0; i < boards.Count; i += 4)
                        {
                            if (i > 0)
                            {
                                column.Item().PageBreak();
                            }

                            // Top row (2 boards)
                            column.Item().Row(row =>
                            {
                                // Top-left quadrant
                                row.RelativeItem().Padding(3).Element(container =>
                                {
                                    RenderBoard(container, boards[i]);
                                });

                                // Top-right quadrant (if exists)
                                if (i + 1 < boards.Count)
                                {
                                    row.RelativeItem().Padding(3).Element(container =>
                                    {
                                        RenderBoard(container, boards[i + 1]);
                                    });
                                }
                                else
                                {
                                    row.RelativeItem(); // Empty space
                                }
                            });

                            // Bottom row (2 boards)
                            column.Item().Row(row =>
                            {
                                // Bottom-left quadrant (if exists)
                                if (i + 2 < boards.Count)
                                {
                                    row.RelativeItem().Padding(3).Element(container =>
                                    {
                                        RenderBoard(container, boards[i + 2]);
                                    });
                                }
                                else
                                {
                                    row.RelativeItem(); // Empty space
                                }

                                // Bottom-right quadrant (if exists)
                                if (i + 3 < boards.Count)
                                {
                                    row.RelativeItem().Padding(3).Element(container =>
                                    {
                                        RenderBoard(container, boards[i + 3]);
                                    });
                                }
                                else
                                {
                                    row.RelativeItem(); // Empty space
                                }
                            });
                        }
                    });
                });
            })
            .GeneratePdf(outputPath);
        }

        private void RenderBoard(IContainer container, GameData gameData)
        {
            var game = gameData.GameModel;

            container.Border(1).BorderColor(Colors.Black).Padding(6).Column(column =>
            {
                // Header - expanded with full text
                column.Item().Text(text =>
                {
                    text.Span("╔═══════════════════════════════════════════════════╗").FontSize(8);
                });

                column.Item().Text(text =>
                {
                    text.Span($"║ Board {game.BoardNumber,-2}  Dealer: {game.Dealer,-5}  Vulnerable: {game.Vulnerable,-10}   ║").FontSize(8);
                });

                column.Item().Text(text =>
                {
                    text.Span("╚═══════════════════════════════════════════════════╝").FontSize(8);
                });

                column.Item().PaddingTop(3);

                // Display hands
                column.Item().Text("HANDS:").FontSize(9).Bold();
                column.Item().LineHorizontal(1).LineColor(Colors.Grey.Medium);

                foreach (var player in new[] { GameModel.Player.North, GameModel.Player.East, GameModel.Player.South, GameModel.Player.West })
                {
                    if (game.PlayerHands.ContainsKey(player))
                    {
                        column.Item().Text($"{FormatPlayerName(player)}: {FormatHandCompact(game.PlayerHands[player])}").FontSize(10);
                    }
                }

                column.Item().PaddingTop(3);

                // Display match points
                if (gameData.ScoreDetails.Any())
                {
                    column.Item().Text("MATCH POINTS AWARDED:").FontSize(8).Bold();
                    column.Item().LineHorizontal(1).LineColor(Colors.Grey.Medium);

                    // Table header - expanded columns
                    column.Item().Row(row =>
                    {
                        row.ConstantItem(60).Text("Contract").FontSize(8).Bold();
                        row.ConstantItem(35).Text("Tricks").FontSize(8).Bold();
                        row.ConstantItem(45).Text("Score").FontSize(8).Bold();
                        row.ConstantItem(30).Text("MP").FontSize(8).Bold();
                        row.RelativeItem().Text("Ranking").FontSize(8).Bold();
                    });

                    column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                    // Match points rows
                    foreach (var detail in gameData.ScoreDetails)
                    {
                        column.Item().Row(row =>
                        {
                            // Full contract display
                            var contractDisplay = detail.IsStoredScore && !string.IsNullOrEmpty(detail.Contract)
                                ? $"{detail.Contract} by {detail.Declarer}"
                                : "---";
                            
                            var tricksDisplay = detail.IsStoredScore && detail.TricksMade > 0
                                ? detail.TricksMade.ToString()
                                : "---";
                            
                            var scoreDisplay = detail.IsStoredScore
                                ? detail.Score.ToString()
                                : "---";
                            
                            row.ConstantItem(60).Text(contractDisplay).FontSize(8);
                            row.ConstantItem(35).Text(tricksDisplay).FontSize(8);
                            row.ConstantItem(45).Text(scoreDisplay).FontSize(8);
                            row.ConstantItem(30).Text($"{detail.MatchPoints:F1}").FontSize(8);
                            row.RelativeItem().Text(detail.Ranking).FontSize(8);
                        });
                    }
                }
            });
        }

        private string FormatPlayerName(GameModel.Player player)
        {
            return $"{player,-5}";
        }

        private string FormatPlayerNameCompact(GameModel.Player player)
        {
            return player.ToString().Substring(0, 1); // Just N, E, S, W
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

        private string FormatHandCompact(string handString)
        {
            if (string.IsNullOrWhiteSpace(handString))
                return "";

            // Hand format is: Spades.Hearts.Diamonds.Clubs
            var suits = handString.Split('.');
            if (suits.Length != 4)
                return handString;

            // Use Unicode suit symbols - more compact spacing
            return $"♠{suits[0],-10} ♥{suits[1],-10} ♦{suits[2],-10} ♣{suits[3]}";
        }
    }
}