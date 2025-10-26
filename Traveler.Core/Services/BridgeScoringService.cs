using Traveler.Core.Models;

namespace Traveler.Core.Services
{
    public class BridgeScoringService
    {
        public int CalculateNorthScore(GameModel.GameResult result, string vulnerable)
        {
            // Determine if NS or EW are vulnerable
            bool nsVul = vulnerable == "All" || vulnerable == "NS";
            bool ewVul = vulnerable == "All" || vulnerable == "EW";

            // Determine if declarer is NS or EW
            bool declarerIsNS = result.Declarer == GameModel.Player.North || result.Declarer == GameModel.Player.South;
            bool declarerVul = declarerIsNS ? nsVul : ewVul;

            // Parse contract
            var contract = result.Contract;
            if (string.IsNullOrWhiteSpace(contract))
                return 0;

            // Check for passed hand
            if (contract.ToUpper() == "PASS" || contract.ToUpper() == "P")
                return 0;

            // Parse level (first character)
            if (!int.TryParse(contract.Substring(0, 1), out int level))
                return 0;

            // Parse denomination
            char denomination = contract.Length > 1 ? contract[1] : ' ';

            // Check for doubled/redoubled
            bool doubled = contract.Contains("x") || contract.Contains("X");
            bool redoubled = contract.Contains("xx") || contract.Contains("XX");
            if (redoubled) doubled = false; // redoubled overrides doubled

            // Calculate score based on result (tricks made)
            int tricksNeeded = 6 + level;
            int tricksMade = result.Result;
            int tricksOver = tricksMade - tricksNeeded;

            int score = 0;

            if (tricksOver >= 0)
            {
                // Contract made
                score = CalculateContractScore(level, denomination, tricksOver, doubled, redoubled, declarerVul);
            }
            else
            {
                // Contract failed
                int undertricks = Math.Abs(tricksOver);
                score = -CalculateUndertrickPenalty(undertricks, doubled, redoubled, declarerVul);
            }

            // If declarer is EW, negate the score for NS perspective
            if (!declarerIsNS)
                score = -score;

            return score;
        }

        private int CalculateContractScore(int level, char denomination, int overtricks, bool doubled, bool redoubled, bool vulnerable)
        {
            int baseScore = 0;
            int overtrickValue = 0;

            // Calculate base contract score
            switch (char.ToUpper(denomination))
            {
                case 'C':
                case 'D':
                    baseScore = level * 20;
                    overtrickValue = 20;
                    break;
                case 'H':
                case 'S':
                    baseScore = level * 30;
                    overtrickValue = 30;
                    break;
                case 'N': // No Trump
                    baseScore = 40 + (level - 1) * 30;
                    overtrickValue = 30;
                    break;
                default:
                    return 0;
            }

            // Apply doubled/redoubled multiplier to base score
            if (redoubled)
                baseScore *= 4;
            else if (doubled)
                baseScore *= 2;

            // Add game/slam bonus
            int bonus = 0;
            if (baseScore >= 100)
            {
                // Game bonus
                bonus = vulnerable ? 500 : 300;
            }
            else
            {
                // Part game bonus
                bonus = 50;
            }

            // Add slam bonus
            if (level == 6)
            {
                bonus += vulnerable ? 750 : 500; // Small slam
            }
            else if (level == 7)
            {
                bonus += vulnerable ? 1500 : 1000; // Grand slam
            }

            // Add doubled/redoubled bonus
            if (doubled)
                bonus += 50;
            else if (redoubled)
                bonus += 100;

            // Calculate overtrick score
            int overtrickScore = 0;
            if (doubled)
            {
                overtrickScore = overtricks * (vulnerable ? 200 : 100);
            }
            else if (redoubled)
            {
                overtrickScore = overtricks * (vulnerable ? 400 : 200);
            }
            else
            {
                overtrickScore = overtricks * overtrickValue;
            }

            return baseScore + bonus + overtrickScore;
        }

        private int CalculateUndertrickPenalty(int undertricks, bool doubled, bool redoubled, bool vulnerable)
        {
            if (!doubled && !redoubled)
            {
                // Not doubled
                return undertricks * (vulnerable ? 100 : 50);
            }

            int penalty = 0;
            int multiplier = redoubled ? 2 : 1;

            for (int i = 1; i <= undertricks; i++)
            {
                if (vulnerable)
                {
                    // Vulnerable: 200 for first, 300 for subsequent
                    penalty += (i == 1 ? 200 : 300) * multiplier;
                }
                else
                {
                    // Non-vulnerable: 100 for first, 200 for 2nd/3rd, 300 for subsequent
                    if (i == 1)
                        penalty += 100 * multiplier;
                    else if (i <= 3)
                        penalty += 200 * multiplier;
                    else
                        penalty += 300 * multiplier;
                }
            }

            return penalty;
        }
    }
}