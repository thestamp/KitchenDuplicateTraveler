using Traveler.Core.Models;
using Traveler.Core.Parsers;
namespace Traveler.Core.Services
{
    public class FileImportService
    {
        public HashSet<GameModel> ImportFile(string fileContent)
        {
            Console.WriteLine("🔍 FileImportService.ImportFile() called");
            var games = new HashSet<GameModel>();
            
            if (string.IsNullOrWhiteSpace(fileContent))
            {
                Console.WriteLine("❌ ERROR: File content is null or whitespace");
                return games;
            }

            Console.WriteLine($"✅ File content length: {fileContent.Length} characters");
            Console.WriteLine($"📄 First 200 characters: {fileContent.Substring(0, Math.Min(200, fileContent.Length))}");

            // Normalize line endings - CRITICAL for browser file uploads
            fileContent = fileContent.Replace("\r\n", "\n").Replace("\r", "\n");
            var lines = fileContent.Split('\n', StringSplitOptions.None);
            
            Console.WriteLine($"📊 Total lines after normalization: {lines.Length}");
            
            GameModel currentGame = null;
            List<string> currentTableLines = new List<string>();
            bool inScoreTable = false;
            bool inOptimumResultTable = false;
            bool inDoubleDummyTricks = false;
            int lineNumber = 0;
            int gamesCreated = 0;

            foreach (var line in lines)
            {
                lineNumber++;
                var trimmedLine = line.Trim();

                // Log every 50 lines or important lines
                if (lineNumber % 50 == 0)
                {
                    Console.WriteLine($"⏩ Processing line {lineNumber} of {lines.Length}");
                }

                // Skip comments and empty lines at file start
                if (trimmedLine.StartsWith("%"))
                {
                    Console.WriteLine($"💬 Line {lineNumber}: Comment skipped");
                    continue;
                }
                
                if (currentGame == null && string.IsNullOrWhiteSpace(trimmedLine))
                {
                    Console.WriteLine($"⏭️  Line {lineNumber}: Empty line before first game, skipped");
                    continue;
                }

                // Check for DoubleDummyTricks section (we skip this data)
                if (trimmedLine.StartsWith("[DoubleDummyTricks"))
                {
                    Console.WriteLine($"🎲 Line {lineNumber}: Entering DoubleDummyTricks section");
                    inDoubleDummyTricks = true;
                    continue;
                }

                // If in DoubleDummyTricks, skip until next tag
                if (inDoubleDummyTricks)
                {
                    if (trimmedLine.StartsWith("["))
                    {
                        Console.WriteLine($"🎲 Line {lineNumber}: Exiting DoubleDummyTricks section");
                        inDoubleDummyTricks = false;
                        // Don't continue - process this line as a tag below
                    }
                    else
                    {
                        continue;
                    }
                }

                // Check for OptimumResultTable section (we skip this data)
                if (trimmedLine.StartsWith("[OptimumResultTable"))
                {
                    Console.WriteLine($"📋 Line {lineNumber}: Entering OptimumResultTable section");
                    inOptimumResultTable = true;
                    currentTableLines.Clear();
                    continue;
                }

                // If in OptimumResultTable, collect lines until we hit a tag
                if (inOptimumResultTable)
                {
                    if (trimmedLine.StartsWith("["))
                    {
                        Console.WriteLine($"📋 Line {lineNumber}: Exiting OptimumResultTable section");
                        // End of OptimumResultTable, don't process the data
                        inOptimumResultTable = false;
                        currentTableLines.Clear();
                        // Don't continue - process this line as a tag below
                    }
                    else
                    {
                        currentTableLines.Add(trimmedLine);
                        continue;
                    }
                }

                // Check for ScoreTable section
                if (trimmedLine.StartsWith("[ScoreTable"))
                {
                    Console.WriteLine($"📊 Line {lineNumber}: Entering ScoreTable section");
                    inScoreTable = true;
                    currentTableLines.Clear();
                    continue;
                }

                // If in score table, collect lines until we hit a tag
                if (inScoreTable)
                {
                    // Only end score table when we hit a new tag
                    if (trimmedLine.StartsWith("["))
                    {
                        Console.WriteLine($"📊 Line {lineNumber}: Exiting ScoreTable section with {currentTableLines.Count} lines");
                        // End of score table
                        if (currentGame != null && currentTableLines.Count > 0)
                        {
                            var results = PbnParser.ParseScoreTable(currentTableLines);
                            Console.WriteLine($"✅ Parsed {results.Count} game results from ScoreTable");
                            foreach (var result in results)
                            {
                                currentGame.GameResults.Add(result);
                            }
                        }
                        inScoreTable = false;
                        currentTableLines.Clear();
                        // Don't continue - process this tag line below
                    }
                    else
                    {
                        // Collect all non-tag lines (including empty lines)
                        currentTableLines.Add(trimmedLine);
                        continue;
                    }
                }

                // Start new game on Event tag
                if (trimmedLine.StartsWith("[Event"))
                {
                    if (currentGame != null)
                    {
                        Console.WriteLine($"🎮 Line {lineNumber}: Saving previous game (Board #{currentGame.BoardNumber}) with {currentGame.GameResults.Count} results");
                        games.Add(currentGame);
                        gamesCreated++;
                    }
                    currentGame = new GameModel();
                    currentGame.Event = PbnParser.ParseTag(trimmedLine, "Event");
                    Console.WriteLine($"🎮 Line {lineNumber}: Created new game, Event='{currentGame.Event}'");
                    continue;
                }

                if (currentGame == null)
                {
                    if (!string.IsNullOrWhiteSpace(trimmedLine))
                    {
                        Console.WriteLine($"⚠️  Line {lineNumber}: Tag found but no current game: {trimmedLine}");
                    }
                    continue;
                }

                // Parse other tags
                if (trimmedLine.StartsWith("[Site"))
                {
                    currentGame.Site = PbnParser.ParseTag(trimmedLine, "Site");
                    Console.WriteLine($"📍 Line {lineNumber}: Site='{currentGame.Site}'");
                }
                else if (trimmedLine.StartsWith("[Date"))
                {
                    currentGame.Date = PbnParser.ParseTag(trimmedLine, "Date");
                    Console.WriteLine($"📅 Line {lineNumber}: Date='{currentGame.Date}'");
                }
                else if (trimmedLine.StartsWith("[Board"))
                {
                    var boardStr = PbnParser.ParseTag(trimmedLine, "Board");
                    currentGame.BoardNumber = PbnParser.ParseBoardNumber(boardStr);
                    Console.WriteLine($"🎯 Line {lineNumber}: Board #{currentGame.BoardNumber}");
                }
                else if (trimmedLine.StartsWith("[Dealer"))
                {
                    var dealerStr = PbnParser.ParseTag(trimmedLine, "Dealer");
                    currentGame.Dealer = PbnParser.ParsePlayer(dealerStr);
                    Console.WriteLine($"👤 Line {lineNumber}: Dealer={currentGame.Dealer}");
                }
                else if (trimmedLine.StartsWith("[Vulnerable"))
                {
                    currentGame.Vulnerable = PbnParser.ParseTag(trimmedLine, "Vulnerable");
                    Console.WriteLine($"🎲 Line {lineNumber}: Vulnerable='{currentGame.Vulnerable}'");
                }
                else if (trimmedLine.StartsWith("[Deal"))
                {
                    var dealStr = PbnParser.ParseTag(trimmedLine, "Deal");
                    currentGame.DealString = dealStr;
                    currentGame.PlayerHands = PbnParser.ParseDeal(dealStr);
                    Console.WriteLine($"🃏 Line {lineNumber}: Deal parsed, {currentGame.PlayerHands.Count} hands");
                }
                else if (trimmedLine.StartsWith("["))
                {
                    // Log unknown tags
                    var tagName = trimmedLine.Split(' ', ']')[0].Substring(1);
                    Console.WriteLine($"🏷️  Line {lineNumber}: Skipping tag '{tagName}'");
                }
            }

            // Add the last game
            if (currentGame != null)
            {
                // Process any remaining score table lines
                if (inScoreTable && currentTableLines.Count > 0)
                {
                    Console.WriteLine($"📊 Processing remaining ScoreTable with {currentTableLines.Count} lines");
                    var results = PbnParser.ParseScoreTable(currentTableLines);
                    Console.WriteLine($"✅ Parsed {results.Count} game results from final ScoreTable");
                    foreach (var result in results)
                    {
                        currentGame.GameResults.Add(result);
                    }
                }
                Console.WriteLine($"🎮 Saving final game (Board #{currentGame.BoardNumber}) with {currentGame.GameResults.Count} results");
                games.Add(currentGame);
                gamesCreated++;
            }

            Console.WriteLine($"");
            Console.WriteLine($"═══════════════════════════════════════════════════════");
            Console.WriteLine($"✅ IMPORT COMPLETE");
            Console.WriteLine($"📊 Total games created: {gamesCreated}");
            Console.WriteLine($"📊 Games in HashSet: {games.Count}");
            Console.WriteLine($"═══════════════════════════════════════════════════════");
            
            if (games.Count == 0)
            {
                Console.WriteLine($"");
                Console.WriteLine($"❌ WARNING: No games were imported!");
                Console.WriteLine($"   File had {lines.Length} lines");
                Console.WriteLine($"   First 500 chars: {fileContent.Substring(0, Math.Min(500, fileContent.Length))}");
            }
            else
            {
                foreach (var game in games.OrderBy(g => g.BoardNumber))
                {
                    Console.WriteLine($"   Board {game.BoardNumber}: {game.GameResults.Count} results");
                }
            }

            return games;
        }
    }
}
