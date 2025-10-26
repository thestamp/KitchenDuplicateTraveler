using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Traveller.Core.Models;
using Traveller.Core.Parsers;

namespace Traveller.Core.Services
{
    public class FileImportService
    {
        public HashSet<GameModel> ImportFile(string fileContent)
        {
            var games = new HashSet<GameModel>();
            
            if (string.IsNullOrWhiteSpace(fileContent))
                return games;

            var lines = fileContent.Split(new[] { '\r', '\n' }, StringSplitOptions.None);
            GameModel currentGame = null;
            List<string> currentTableLines = new List<string>();
            bool inScoreTable = false;
            bool inOptimumResultTable = false;
            bool inDoubleDummyTricks = false;

            foreach (var line in lines)
            {
                var trimmedLine = line.Trim();

                // Skip comments and empty lines at file start
                if (trimmedLine.StartsWith("%") || (currentGame == null && string.IsNullOrWhiteSpace(trimmedLine)))
                    continue;

                // Check for DoubleDummyTricks section (we skip this data)
                if (trimmedLine.StartsWith("[DoubleDummyTricks"))
                {
                    inDoubleDummyTricks = true;
                    continue;
                }

                // If in DoubleDummyTricks, skip until next tag
                if (inDoubleDummyTricks)
                {
                    if (trimmedLine.StartsWith("["))
                    {
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
                    inOptimumResultTable = true;
                    currentTableLines.Clear();
                    continue;
                }

                // If in OptimumResultTable, collect lines until we hit a tag
                if (inOptimumResultTable)
                {
                    if (trimmedLine.StartsWith("["))
                    {
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
                        // End of score table
                        if (currentGame != null && currentTableLines.Count > 0)
                        {
                            var results = PbnParser.ParseScoreTable(currentTableLines);
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
                        games.Add(currentGame);
                    }
                    currentGame = new GameModel();
                    currentGame.Event = PbnParser.ParseTag(trimmedLine, "Event");
                    continue;
                }

                if (currentGame == null)
                    continue;

                // Parse other tags
                if (trimmedLine.StartsWith("[Site"))
                {
                    currentGame.Site = PbnParser.ParseTag(trimmedLine, "Site");
                }
                else if (trimmedLine.StartsWith("[Date"))
                {
                    currentGame.Date = PbnParser.ParseTag(trimmedLine, "Date");
                }
                else if (trimmedLine.StartsWith("[Board"))
                {
                    var boardStr = PbnParser.ParseTag(trimmedLine, "Board");
                    currentGame.BoardNumber = PbnParser.ParseBoardNumber(boardStr);
                }
                else if (trimmedLine.StartsWith("[Dealer"))
                {
                    var dealerStr = PbnParser.ParseTag(trimmedLine, "Dealer");
                    currentGame.Dealer = PbnParser.ParsePlayer(dealerStr);
                }
                else if (trimmedLine.StartsWith("[Vulnerable"))
                {
                    currentGame.Vulnerable = PbnParser.ParseTag(trimmedLine, "Vulnerable");
                }
                else if (trimmedLine.StartsWith("[Deal"))
                {
                    var dealStr = PbnParser.ParseTag(trimmedLine, "Deal");
                    currentGame.DealString = dealStr;
                    currentGame.PlayerHands = PbnParser.ParseDeal(dealStr);
                }
                // Skip tags with "?" values - these are placeholders
                // Also skip West, North, East, South tags (player names)
                // Also skip Scoring tag and other optional tags we don't use
            }

            // Add the last game
            if (currentGame != null)
            {
                // Process any remaining score table lines
                if (inScoreTable && currentTableLines.Count > 0)
                {
                    var results = PbnParser.ParseScoreTable(currentTableLines);
                    foreach (var result in results)
                    {
                        currentGame.GameResults.Add(result);
                    }
                }
                games.Add(currentGame);
            }

            return games;
        }
    }
}
