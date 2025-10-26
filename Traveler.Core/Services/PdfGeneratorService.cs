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
                        // Process boards 2 per page
                        for (int i = 0; i < boards.Count; i += 2)
                        {
                            if (i > 0)
                            {
                                column.Item().PageBreak();
                            }

                            column.Item().Row(row =>
                            {
                                // Left board
                                row.RelativeItem().Padding(5).Element(container =>
                                {
                                    RenderBoard(container, boards[i]);
                                });

                                // Right board (if exists)
                                if (i + 1 < boards.Count)
                                {
                                    row.RelativeItem().Padding(5).Element(container =>
                                    {
                                        RenderBoard(container, boards[i + 1]);
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

            container.Border(1).BorderColor(Colors.Black).Padding(8).Column(column =>
            {
                // Header
                column.Item().Text(text =>
                {
                    text.Span("╔═══════════════════════════════════════════════════════════╗").FontSize(8);
                });

                column.Item().Text(text =>
                {
                    text.Span($"║ BOARD {game.BoardNumber,-2}  Dealer: {game.Dealer,-5}  Vulnerable: {game.Vulnerable,-10}        ║").FontSize(8);
                });

                column.Item().Text(text =>
                {
                    text.Span("╚═══════════════════════════════════════════════════════════╝").FontSize(8);
                });

                column.Item().PaddingTop(5);

                // Display hands
                column.Item().Text("HANDS:").FontSize(8).Bold();
                column.Item().LineHorizontal(1).LineColor(Colors.Grey.Medium);

                foreach (var player in new[] { GameModel.Player.North, GameModel.Player.East, GameModel.Player.South, GameModel.Player.West })
                {
                    if (game.PlayerHands.ContainsKey(player))
                    {
                        column.Item().Text($"{FormatPlayerName(player)}: {FormatHand(game.PlayerHands[player])}").FontSize(7);
                    }
                }

                column.Item().PaddingTop(5);

                // Display match points
                if (gameData.ScoreDetails.Any())
                {
                    column.Item().Text("MATCH POINTS AWARDED:").FontSize(8).Bold();
                    column.Item().LineHorizontal(1).LineColor(Colors.Grey.Medium);

                    // Table header: Score, MP, Ranking
                    column.Item().Row(row =>
                    {
                        row.ConstantItem(50).Text("Score").FontSize(7).Bold();
                        row.ConstantItem(30).Text("MP").FontSize(7).Bold();
                        row.RelativeItem().Text("Ranking").FontSize(7).Bold();
                    });

                    column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                    // Match points rows
                    foreach (var detail in gameData.ScoreDetails)
                    {
                        column.Item().Row(row =>
                        {
                            var scoreDisplay = detail.IsStoredScore ? detail.Score.ToString() : "---";
                            
                            row.ConstantItem(50).Text(scoreDisplay).FontSize(7);
                            row.ConstantItem(30).Text($"{detail.MatchPoints:F1}").FontSize(7);
                            row.RelativeItem().Text(detail.Ranking).FontSize(7);
                        });
                    }
                }
            });
        }

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
    }
}