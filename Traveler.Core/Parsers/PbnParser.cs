using System.Diagnostics.Contracts;
using System.Text.RegularExpressions;
using Traveler.Core.Models;



namespace Traveler.Core.Parsers
{
    public static class PbnParser
    {
        public static string ParseTag(string line, string tagName)
        {
            var pattern = $@"\[{tagName}\s+""([^""]*)""\]";
            var match = Regex.Match(line, pattern);
            return match.Success ? match.Groups[1].Value : null;
        }

        public static Dictionary<GameModel.Player, string> ParseDeal(string dealString)
        {
            var hands = new Dictionary<GameModel.Player, string>();

            if (string.IsNullOrWhiteSpace(dealString))
                return hands;

            // Deal format: "N:Q87.96.KQ854.T98 K5.Q875.JT7.K653 A.KJT4.A96.AQJ74 JT96432.A32.32.2"
            // Format is: "Dealer:North East South West" where each hand is Spades.Hearts.Diamonds.Clubs
            var parts = dealString.Split(':');
            if (parts.Length != 2)
                return hands;

            var dealer = ParsePlayer(parts[0]);
            var handStrings = parts[1].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (handStrings.Length != 4)
                return hands;

            var players = new[] { dealer, GetNextPlayer(dealer), GetNextPlayer(GetNextPlayer(dealer)), GetNextPlayer(GetNextPlayer(GetNextPlayer(dealer))) };

            for (int i = 0; i < 4; i++)
            {
                hands[players[i]] = handStrings[i];
            }

            return hands;
        }

        public static GameModel.Player ParsePlayer(string playerStr)
        {
            if (string.IsNullOrWhiteSpace(playerStr))
                return GameModel.Player.North;

            switch (playerStr.Trim().ToUpper())
            {
                case "N":
                case "NORTH":
                    return GameModel.Player.North;
                case "E":
                case "EAST":
                    return GameModel.Player.East;
                case "S":
                case "SOUTH":
                    return GameModel.Player.South;
                case "W":
                case "WEST":
                    return GameModel.Player.West;
                default:
                    return GameModel.Player.North;
            }
        }

        private static GameModel.Player GetNextPlayer(GameModel.Player current)
        {
            switch (current)
            {
                case GameModel.Player.North:
                    return GameModel.Player.East;
                case GameModel.Player.East:
                    return GameModel.Player.South;
                case GameModel.Player.South:
                    return GameModel.Player.West;
                case GameModel.Player.West:
                    return GameModel.Player.North;
                default:
                    return GameModel.Player.North;
            }
        }

        public static List<GameModel.GameResult> ParseScoreTable(List<string> scoreLines)
        {
            Console.WriteLine($"🔍 ParseScoreTable called with {scoreLines.Count} lines");
            var results = new List<GameModel.GameResult>();

            int lineNum = 0;
            foreach (var line in scoreLines)
            {
                lineNum++;

                if (string.IsNullOrWhiteSpace(line))
                {
                    Console.WriteLine($"   Line {lineNum}: Empty/whitespace - SKIP");
                    continue;
                }

                var trimmedLine = line.Trim();
                Console.WriteLine($"   Line {lineNum}: '{trimmedLine}'");

                // Skip any line that looks like a header or contains special formatting
                if (trimmedLine.Contains("PairId") ||
                    trimmedLine.Contains("Declarer") ||
                    trimmedLine.Contains("Contract") ||
                    trimmedLine.Contains("Result") ||
                    trimmedLine.Contains("\\") ||
                    trimmedLine.Contains(";") ||
                    trimmedLine.Contains("Denomination"))
                {
                    Console.WriteLine($"      ↳ Contains header keyword - SKIP");
                    continue;
                }

                // Split by multiple spaces to get the fields
                var parts = Regex.Split(trimmedLine, @"\s+");
                Console.WriteLine($"      ↳ Split into {parts.Length} parts: [{string.Join(", ", parts)}]");

                // Handle both formats:
                // Format 1 (5 parts): PairNS PairEW Contract Declarer Result
                // Format 2 (4 parts): TableNum Contract Declarer Result
                
                int pairNS, pairEW;
                string contract;
                string declarerStr;
                int result;

                if (parts.Length == 4)
                {
                    // Format 2: TableNum Contract Declarer Result
                    if (!int.TryParse(parts[0], out int tableNum))
                    {
                        Console.WriteLine($"      ↳ Parts[0]='{parts[0]}' not an integer - SKIP");
                        continue;
                    }

                    contract = parts[1];
                    declarerStr = parts[2];

                    if (!int.TryParse(parts[3], out result))
                    {
                        Console.WriteLine($"      ↳ Parts[3]='{parts[3]}' not an integer - SKIP");
                        continue;
                    }

                    // Use table number for both pair IDs (they're not provided in this format)
                    pairNS = tableNum;
                    pairEW = tableNum;
                }
                else if (parts.Length >= 5)
                {
                    // Format 1: PairNS PairEW Contract Declarer Result
                    if (!int.TryParse(parts[0], out pairNS))
                    {
                        Console.WriteLine($"      ↳ Parts[0]='{parts[0]}' not an integer - SKIP");
                        continue;
                    }
                    if (!int.TryParse(parts[1], out pairEW))
                    {
                        Console.WriteLine($"      ↳ Parts[1]='{parts[1]}' not an integer - SKIP");
                        continue;
                    }

                    contract = parts[2];
                    declarerStr = parts[3];

                    if (!int.TryParse(parts[4], out result))
                    {
                        Console.WriteLine($"      ↳ Parts[4]='{parts[4]}' not an integer - SKIP");
                        continue;
                    }
                }
                else
                {
                    Console.WriteLine($"      ↳ Unexpected format ({parts.Length} parts) - SKIP");
                    continue;
                }

                var declarer = ParsePlayer(declarerStr);

                Console.WriteLine($"      ↳ ✅ PARSED: PairNS={pairNS}, PairEW={pairEW}, Contract={contract}, Declarer={declarer}, Result={result}");

                results.Add(new GameModel.GameResult
                {
                    PairIdNS = pairNS,
                    PairIdEW = pairEW,
                    Contract = contract,
                    Declarer = declarer,
                    Result = result
                });
            }

            Console.WriteLine($"🔍 ParseScoreTable returning {results.Count} results");
            return results;
        }

        public static int ParseBoardNumber(string boardStr)
        {
            if (int.TryParse(boardStr, out int board))
                return board;
            return 0;
        }
    }
}