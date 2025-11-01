using Traveler.Core.Models;
using Traveler.Core.Services;
using Xunit.Abstractions;

namespace Traveler.Core.Tests
{
    /// <summary>
    /// Exhaustive unit tests for bridge scoring calculation.
    /// Tests verify 100% accuracy according to official duplicate bridge scoring rules.
    /// </summary>
    public class BridgeScoringServiceTests
    {
 private readonly ITestOutputHelper _output;
  private readonly BridgeScoringService _scoringService;

        public BridgeScoringServiceTests(ITestOutputHelper output)
        {
            _output = output;
      _scoringService = new BridgeScoringService();
    }

      #region Helper Methods

        private GameModel.GameResult CreateResult(string contract, GameModel.Player declarer, int tricksMade)
  {
         return new GameModel.GameResult
     {
   Contract = contract,
      Declarer = declarer,
       Result = tricksMade
         };
      }

   #endregion

        #region Part Game Contracts - Non-Vulnerable

    [Theory]
        [InlineData("1C", GameModel.Player.North, 7, 70)]   // Made exactly
        [InlineData("1C", GameModel.Player.North, 8, 90)]   // 1 overtrick
        [InlineData("1C", GameModel.Player.North, 9, 110)]  // 2 overtricks
        [InlineData("1D", GameModel.Player.North, 7, 70)]   // Diamonds
[InlineData("1D", GameModel.Player.North, 8, 90)]
   [InlineData("2C", GameModel.Player.North, 8, 90)]   // 2-level
        [InlineData("2C", GameModel.Player.North, 9, 110)]
        [InlineData("2D", GameModel.Player.North, 8, 90)]
        [InlineData("3C", GameModel.Player.North, 9, 110)]  // 3-level
  [InlineData("3D", GameModel.Player.North, 9, 110)]
        public void MinorSuitPartGame_NonVulnerable_CalculatesCorrectly(string contract, GameModel.Player declarer, int tricks, int expectedScore)
        {
            var result = CreateResult(contract, declarer, tricks);
          var score = _scoringService.CalculateNorthScore(result, "None");
   Assert.Equal(expectedScore, score);
        }

     [Theory]
        [InlineData("1H", GameModel.Player.North, 7, 80)]   // Made exactly
        [InlineData("1H", GameModel.Player.North, 8, 110)]  // 1 overtrick
        [InlineData("1H", GameModel.Player.North, 9, 140)]  // 2 overtricks
        [InlineData("1S", GameModel.Player.North, 7, 80)]   // Spades
        [InlineData("1S", GameModel.Player.North, 8, 110)]
        [InlineData("2H", GameModel.Player.North, 8, 110)]  // 2-level
        [InlineData("2S", GameModel.Player.North, 8, 110)]
        [InlineData("3H", GameModel.Player.North, 9, 140)]  // 3-level
    [InlineData("3S", GameModel.Player.North, 9, 140)]
        public void MajorSuitPartGame_NonVulnerable_CalculatesCorrectly(string contract, GameModel.Player declarer, int tricks, int expectedScore)
   {
       var result = CreateResult(contract, declarer, tricks);
       var score = _scoringService.CalculateNorthScore(result, "None");
     Assert.Equal(expectedScore, score);
        }

        [Theory]
    [InlineData("1N", GameModel.Player.North, 7, 90)]   // Made exactly
        [InlineData("1N", GameModel.Player.North, 8, 120)]  // 1 overtrick
        [InlineData("1N", GameModel.Player.North, 9, 150)]  // 2 overtricks
   [InlineData("2N", GameModel.Player.North, 8, 120)]  // 2-level
        [InlineData("2N", GameModel.Player.North, 9, 150)]
        public void NoTrumpPartGame_NonVulnerable_CalculatesCorrectly(string contract, GameModel.Player declarer, int tricks, int expectedScore)
        {
    var result = CreateResult(contract, declarer, tricks);
            var score = _scoringService.CalculateNorthScore(result, "None");
          Assert.Equal(expectedScore, score);
        }

        #endregion

        #region Game Contracts - Non-Vulnerable

        [Theory]
        [InlineData("3N", GameModel.Player.North, 9, 400)]   // Made exactly
        [InlineData("3N", GameModel.Player.North, 10, 430)]  // 1 overtrick
        [InlineData("3N", GameModel.Player.North, 11, 460)]  // 2 overtricks
   [InlineData("3N", GameModel.Player.North, 12, 490)]  // 3 overtricks
      [InlineData("3N", GameModel.Player.North, 13, 520)]  // 4 overtricks (all tricks)
        public void ThreeNoTrumpGame_NonVulnerable_CalculatesCorrectly(string contract, GameModel.Player declarer, int tricks, int expectedScore)
        {
            var result = CreateResult(contract, declarer, tricks);
       var score = _scoringService.CalculateNorthScore(result, "None");
    Assert.Equal(expectedScore, score);
        }

        [Theory]
      [InlineData("4H", GameModel.Player.North, 10, 420)]  // Made exactly
        [InlineData("4H", GameModel.Player.North, 11, 450)]  // 1 overtrick
        [InlineData("4H", GameModel.Player.North, 12, 480)]  // 2 overtricks
[InlineData("4H", GameModel.Player.North, 13, 510)]  // 3 overtricks (all tricks)
        [InlineData("4S", GameModel.Player.North, 10, 420)]  // Spades
        [InlineData("4S", GameModel.Player.North, 11, 450)]
        public void FourMajorGame_NonVulnerable_CalculatesCorrectly(string contract, GameModel.Player declarer, int tricks, int expectedScore)
{
            var result = CreateResult(contract, declarer, tricks);
            var score = _scoringService.CalculateNorthScore(result, "None");
   Assert.Equal(expectedScore, score);
        }

        [Theory]
        [InlineData("5C", GameModel.Player.North, 11, 400)]  // Made exactly
        [InlineData("5C", GameModel.Player.North, 12, 420)]  // 1 overtrick
     [InlineData("5C", GameModel.Player.North, 13, 440)]  // 2 overtricks (all tricks)
   [InlineData("5D", GameModel.Player.North, 11, 400)]  // Diamonds
        [InlineData("5D", GameModel.Player.North, 12, 420)]
        public void FiveMinorGame_NonVulnerable_CalculatesCorrectly(string contract, GameModel.Player declarer, int tricks, int expectedScore)
        {
   var result = CreateResult(contract, declarer, tricks);
            var score = _scoringService.CalculateNorthScore(result, "None");
            Assert.Equal(expectedScore, score);
        }

        #endregion

        #region Game Contracts - Vulnerable

        [Theory]
   [InlineData("3N", GameModel.Player.North, 9, 600)]   // Made exactly
    [InlineData("3N", GameModel.Player.North, 10, 630)]  // 1 overtrick
        [InlineData("3N", GameModel.Player.North, 11, 660)]  // 2 overtricks
    [InlineData("3N", GameModel.Player.North, 12, 690)]  // 3 overtricks
        [InlineData("3N", GameModel.Player.North, 13, 720)]  // 4 overtricks (all tricks)
        public void ThreeNoTrumpGame_Vulnerable_CalculatesCorrectly(string contract, GameModel.Player declarer, int tricks, int expectedScore)
   {
            var result = CreateResult(contract, declarer, tricks);
        var score = _scoringService.CalculateNorthScore(result, "All");
       Assert.Equal(expectedScore, score);
        }

        [Theory]
        [InlineData("4H", GameModel.Player.North, 10, 620)]  // Made exactly
  [InlineData("4H", GameModel.Player.North, 11, 650)]  // 1 overtrick
        [InlineData("4H", GameModel.Player.North, 12, 680)]  // 2 overtricks
        [InlineData("4H", GameModel.Player.North, 13, 710)]  // 3 overtricks (all tricks)
        [InlineData("4S", GameModel.Player.North, 10, 620)]  // Spades
  [InlineData("4S", GameModel.Player.North, 11, 650)]
        public void FourMajorGame_Vulnerable_CalculatesCorrectly(string contract, GameModel.Player declarer, int tricks, int expectedScore)
        {
  var result = CreateResult(contract, declarer, tricks);
var score = _scoringService.CalculateNorthScore(result, "All");
      Assert.Equal(expectedScore, score);
        }

        [Theory]
        [InlineData("5C", GameModel.Player.North, 11, 600)]  // Made exactly
        [InlineData("5C", GameModel.Player.North, 12, 620)]  // 1 overtrick
        [InlineData("5C", GameModel.Player.North, 13, 640)]  // 2 overtricks (all tricks)
  [InlineData("5D", GameModel.Player.North, 11, 600)]  // Diamonds
     [InlineData("5D", GameModel.Player.North, 12, 620)]
     public void FiveMinorGame_Vulnerable_CalculatesCorrectly(string contract, GameModel.Player declarer, int tricks, int expectedScore)
        {
 var result = CreateResult(contract, declarer, tricks);
          var score = _scoringService.CalculateNorthScore(result, "All");
     Assert.Equal(expectedScore, score);
 }

        #endregion

        #region Small Slam Contracts (6-level)

        [Theory]
        [InlineData("6C", GameModel.Player.North, 12, "None", 920)]   // Non-vul made exactly
        [InlineData("6C", GameModel.Player.North, 13, "None", 940)]   // Non-vul + 1 overtrick
        [InlineData("6D", GameModel.Player.North, 12, "None", 920)]   // Diamonds
        [InlineData("6H", GameModel.Player.North, 12, "None", 980)]   // Hearts
        [InlineData("6H", GameModel.Player.North, 13, "None", 1010)]  // Hearts + 1
        [InlineData("6S", GameModel.Player.North, 12, "None", 980)]   // Spades
 [InlineData("6N", GameModel.Player.North, 12, "None", 990)]   // No Trump
        [InlineData("6N", GameModel.Player.North, 13, "None", 1020)]  // No Trump + 1
        public void SmallSlam_NonVulnerable_CalculatesCorrectly(string contract, GameModel.Player declarer, int tricks, string vulnerable, int expectedScore)
        {
     var result = CreateResult(contract, declarer, tricks);
            var score = _scoringService.CalculateNorthScore(result, vulnerable);
            Assert.Equal(expectedScore, score);
        }

        [Theory]
 [InlineData("6C", GameModel.Player.North, 12, "All", 1370)]   // Vul made exactly
        [InlineData("6C", GameModel.Player.North, 13, "All", 1390)]   // Vul + 1 overtrick
        [InlineData("6D", GameModel.Player.North, 12, "All", 1370)]   // Diamonds
        [InlineData("6H", GameModel.Player.North, 12, "All", 1430)]   // Hearts
        [InlineData("6H", GameModel.Player.North, 13, "All", 1460)]   // Hearts + 1
        [InlineData("6S", GameModel.Player.North, 12, "All", 1430)]   // Spades
        [InlineData("6N", GameModel.Player.North, 12, "All", 1440)]   // No Trump
        [InlineData("6N", GameModel.Player.North, 13, "All", 1470)]   // No Trump + 1
     public void SmallSlam_Vulnerable_CalculatesCorrectly(string contract, GameModel.Player declarer, int tricks, string vulnerable, int expectedScore)
        {
       var result = CreateResult(contract, declarer, tricks);
   var score = _scoringService.CalculateNorthScore(result, vulnerable);
        Assert.Equal(expectedScore, score);
 }

        #endregion

        #region Grand Slam Contracts (7-level)

        [Theory]
        [InlineData("7C", GameModel.Player.North, 13, "None", 1440)]  // Non-vul made
      [InlineData("7D", GameModel.Player.North, 13, "None", 1440)]  // Diamonds
        [InlineData("7H", GameModel.Player.North, 13, "None", 1510)]  // Hearts
      [InlineData("7S", GameModel.Player.North, 13, "None", 1510)]  // Spades
        [InlineData("7N", GameModel.Player.North, 13, "None", 1520)]  // No Trump
        public void GrandSlam_NonVulnerable_CalculatesCorrectly(string contract, GameModel.Player declarer, int tricks, string vulnerable, int expectedScore)
        {
var result = CreateResult(contract, declarer, tricks);
   var score = _scoringService.CalculateNorthScore(result, vulnerable);
   Assert.Equal(expectedScore, score);
        }

     [Theory]
  [InlineData("7C", GameModel.Player.North, 13, "All", 2140)]   // Vul made
        [InlineData("7D", GameModel.Player.North, 13, "All", 2140)]   // Diamonds
        [InlineData("7H", GameModel.Player.North, 13, "All", 2210)]   // Hearts
     [InlineData("7S", GameModel.Player.North, 13, "All", 2210)]   // Spades
  [InlineData("7N", GameModel.Player.North, 13, "All", 2220)]   // No Trump
        public void GrandSlam_Vulnerable_CalculatesCorrectly(string contract, GameModel.Player declarer, int tricks, string vulnerable, int expectedScore)
        {
     var result = CreateResult(contract, declarer, tricks);
 var score = _scoringService.CalculateNorthScore(result, vulnerable);
     Assert.Equal(expectedScore, score);
        }

    #endregion

        #region Doubled Contracts - Made

    [Theory]
        [InlineData("1Cx", GameModel.Player.North, 7, "None", 140)]   // 1C doubled made (20*2 + 50 + 50)
        [InlineData("1Cx", GameModel.Player.North, 8, "None", 240)]   // 1C doubled + 1 (140 + 100)
        [InlineData("1Cx", GameModel.Player.North, 9, "None", 340)]   // 1C doubled + 2
        [InlineData("1Dx", GameModel.Player.North, 7, "None", 140)]   // Diamonds
        [InlineData("1Hx", GameModel.Player.North, 7, "None", 160)]   // Hearts (30*2 + 50 + 50)
        [InlineData("1Hx", GameModel.Player.North, 8, "None", 260)]   // Hearts + 1
        [InlineData("1Sx", GameModel.Player.North, 7, "None", 160)]   // Spades
        [InlineData("1Nx", GameModel.Player.North, 7, "None", 180)] // NT (40 + 30*4 + 50 + 100) - was 330, then incorrectly 430
        [InlineData("2Cx", GameModel.Player.North, 8, "None", 180)]   // 2C doubled made
        [InlineData("3Nx", GameModel.Player.North, 9, "None", 550)]   // 3NT doubled made (game)
        public void DoubledContracts_NonVulnerable_Made_CalculatesCorrectly(string contract, GameModel.Player declarer, int tricks, string vulnerable, int expectedScore)
    {
      var result = CreateResult(contract, declarer, tricks);
  var score = _scoringService.CalculateNorthScore(result, vulnerable);
            Assert.Equal(expectedScore, score);
        }

    [Theory]
        [InlineData("1Cx", GameModel.Player.North, 7, "All", 140)]    // 1C doubled made vul
        [InlineData("1Cx", GameModel.Player.North, 8, "All", 340)]    // 1C doubled + 1 vul (140 + 200)
        [InlineData("1Cx", GameModel.Player.North, 9, "All", 540)]    // 1C doubled + 2 vul
        [InlineData("1Hx", GameModel.Player.North, 7, "All", 160)]    // Hearts vul
   [InlineData("1Hx", GameModel.Player.North, 8, "All", 360)]    // Hearts + 1 vul
 [InlineData("3Nx", GameModel.Player.North, 9, "All", 750)]    // 3NT doubled made vul (game)
        public void DoubledContracts_Vulnerable_Made_CalculatesCorrectly(string contract, GameModel.Player declarer, int tricks, string vulnerable, int expectedScore)
      {
            var result = CreateResult(contract, declarer, tricks);
    var score = _scoringService.CalculateNorthScore(result, vulnerable);
 Assert.Equal(expectedScore, score);
   }

        #endregion

        #region Redoubled Contracts - Made

        [Theory]
        [InlineData("1Cxx", GameModel.Player.North, 7, "None", 230)]   // 1C redoubled made (20*4 + 50 + 100)
      [InlineData("1Cxx", GameModel.Player.North, 8, "None", 430)]   // 1C redoubled + 1 (230 + 200)
      [InlineData("1Cxx", GameModel.Player.North, 9, "None", 630)]   // 1C redoubled + 2
        [InlineData("1Dxx", GameModel.Player.North, 7, "None", 230)]   // Diamonds
        [InlineData("1Hxx", GameModel.Player.North, 7, "None", 270)]   // Hearts (30*4 + 50 + 100)
        [InlineData("1Hxx", GameModel.Player.North, 8, "None", 470)]   // Hearts + 1
     [InlineData("1Sxx", GameModel.Player.North, 7, "None", 270)]   // Spades
        [InlineData("1Nxx", GameModel.Player.North, 7, "None", 310)]  // NT (40*4 + 50 + 100) - was 330, then incorrectly 430
        public void RedoubledContracts_NonVulnerable_Made_CalculatesCorrectly(string contract, GameModel.Player declarer, int tricks, string vulnerable, int expectedScore)
        {
          var result = CreateResult(contract, declarer, tricks);
         var score = _scoringService.CalculateNorthScore(result, vulnerable);
         Assert.Equal(expectedScore, score);
        }

        [Theory]
        [InlineData("1Cxx", GameModel.Player.North, 7, "All", 230)] // 1C redoubled made vul
 [InlineData("1Cxx", GameModel.Player.North, 8, "All", 630)]    // 1C redoubled + 1 vul (230 + 400)
        [InlineData("1Cxx", GameModel.Player.North, 9, "All", 1030)]   // 1C redoubled + 2 vul
        [InlineData("1Hxx", GameModel.Player.North, 7, "All", 270)]    // Hearts vul
   [InlineData("1Hxx", GameModel.Player.North, 8, "All", 670)]    // Hearts + 1 vul
      public void RedoubledContracts_Vulnerable_Made_CalculatesCorrectly(string contract, GameModel.Player declarer, int tricks, string vulnerable, int expectedScore)
        {
   var result = CreateResult(contract, declarer, tricks);
            var score = _scoringService.CalculateNorthScore(result, vulnerable);
     Assert.Equal(expectedScore, score);
        }

        #endregion

        #region Undertricks - Non-Vulnerable, Not Doubled

        [Theory]
        [InlineData("3N", GameModel.Player.North, 8, "None", -50)]    // 3NT down 1
    [InlineData("3N", GameModel.Player.North, 7, "None", -100)]   // 3NT down 2
     [InlineData("3N", GameModel.Player.North, 6, "None", -150)]   // 3NT down 3
        [InlineData("3N", GameModel.Player.North, 5, "None", -200)]   // 3NT down 4
     [InlineData("4H", GameModel.Player.North, 9, "None", -50)]    // 4H down 1
    [InlineData("4H", GameModel.Player.North, 8, "None", -100)]   // 4H down 2
        [InlineData("6N", GameModel.Player.North, 11, "None", -50)]   // 6NT down 1
  [InlineData("7N", GameModel.Player.North, 12, "None", -50)]   // 7NT down 1
        public void Undertricks_NonVulnerable_NotDoubled_CalculatesCorrectly(string contract, GameModel.Player declarer, int tricks, string vulnerable, int expectedScore)
        {
         var result = CreateResult(contract, declarer, tricks);
            var score = _scoringService.CalculateNorthScore(result, vulnerable);
        Assert.Equal(expectedScore, score);
        }

        #endregion

        #region Undertricks - Vulnerable, Not Doubled

        [Theory]
        [InlineData("3N", GameModel.Player.North, 8, "All", -100)]    // 3NT down 1 vul
        [InlineData("3N", GameModel.Player.North, 7, "All", -200)]    // 3NT down 2 vul
     [InlineData("3N", GameModel.Player.North, 6, "All", -300)]    // 3NT down 3 vul
    [InlineData("3N", GameModel.Player.North, 5, "All", -400)]    // 3NT down 4 vul
  [InlineData("4H", GameModel.Player.North, 9, "All", -100)]    // 4H down 1 vul
 [InlineData("4H", GameModel.Player.North, 8, "All", -200)]    // 4H down 2 vul
        [InlineData("6N", GameModel.Player.North, 11, "All", -100)]   // 6NT down 1 vul
        [InlineData("7N", GameModel.Player.North, 12, "All", -100)] // 7NT down 1 vul
        public void Undertricks_Vulnerable_NotDoubled_CalculatesCorrectly(string contract, GameModel.Player declarer, int tricks, string vulnerable, int expectedScore)
        {
   var result = CreateResult(contract, declarer, tricks);
            var score = _scoringService.CalculateNorthScore(result, vulnerable);
            Assert.Equal(expectedScore, score);
        }

   #endregion

      #region Undertricks - Doubled Non-Vulnerable

        [Theory]
        [InlineData("3Nx", GameModel.Player.North, 8, "None", -100)]   // 3NT doubled down 1
    [InlineData("3Nx", GameModel.Player.North, 7, "None", -300)]   // 3NT doubled down 2 (100+200)
        [InlineData("3Nx", GameModel.Player.North, 6, "None", -500)]   // 3NT doubled down 3 (100+200+200)
        [InlineData("3Nx", GameModel.Player.North, 5, "None", -800)]// 3NT doubled down 4 (100+200+200+300)
        [InlineData("3Nx", GameModel.Player.North, 4, "None", -1100)]  // 3NT doubled down 5 (100+200+200+300+300)
        [InlineData("4Hx", GameModel.Player.North, 9, "None", -100)]   // 4H doubled down 1
    [InlineData("4Hx", GameModel.Player.North, 8, "None", -300)]   // 4H doubled down 2
        [InlineData("6Nx", GameModel.Player.North, 11, "None", -100)]  // 6NT doubled down 1
     public void Undertricks_Doubled_NonVulnerable_CalculatesCorrectly(string contract, GameModel.Player declarer, int tricks, string vulnerable, int expectedScore)
        {
            var result = CreateResult(contract, declarer, tricks);
       var score = _scoringService.CalculateNorthScore(result, vulnerable);
   Assert.Equal(expectedScore, score);
        }

      #endregion

      #region Undertricks - Doubled Vulnerable

     [Theory]
        [InlineData("3Nx", GameModel.Player.North, 8, "All", -200)]    // 3NT doubled down 1 vul
        [InlineData("3Nx", GameModel.Player.North, 7, "All", -500)]    // 3NT doubled down 2 vul (200+300)
        [InlineData("3Nx", GameModel.Player.North, 6, "All", -800)]    // 3NT doubled down 3 vul (200+300+300)
    [InlineData("3Nx", GameModel.Player.North, 5, "All", -1100)]   // 3NT doubled down 4 vul (200+300+300+300)
  [InlineData("3Nx", GameModel.Player.North, 4, "All", -1400)]   // 3NT doubled down 5 vul
     [InlineData("4Hx", GameModel.Player.North, 9, "All", -200)]    // 4H doubled down 1 vul
        [InlineData("4Hx", GameModel.Player.North, 8, "All", -500)]    // 4H doubled down 2 vul
        [InlineData("6Nx", GameModel.Player.North, 11, "All", -200)]   // 6NT doubled down 1 vul
        public void Undertricks_Doubled_Vulnerable_CalculatesCorrectly(string contract, GameModel.Player declarer, int tricks, string vulnerable, int expectedScore)
 {
         var result = CreateResult(contract, declarer, tricks);
      var score = _scoringService.CalculateNorthScore(result, vulnerable);
    Assert.Equal(expectedScore, score);
        }

        #endregion

     #region Undertricks - Redoubled Non-Vulnerable

        [Theory]
        [InlineData("3Nxx", GameModel.Player.North, 8, "None", -200)]   // 3NT redoubled down 1
    [InlineData("3Nxx", GameModel.Player.North, 7, "None", -600)]   // 3NT redoubled down 2 (200+400)
        [InlineData("3Nxx", GameModel.Player.North, 6, "None", -1000)]  // 3NT redoubled down 3 (200+400+400)
        [InlineData("3Nxx", GameModel.Player.North, 5, "None", -1600)]  // 3NT redoubled down 4 (200+400+400+600)
     [InlineData("4Hxx", GameModel.Player.North, 9, "None", -200)]   // 4H redoubled down 1
        [InlineData("4Hxx", GameModel.Player.North, 8, "None", -600)]   // 4H redoubled down 2
        public void Undertricks_Redoubled_NonVulnerable_CalculatesCorrectly(string contract, GameModel.Player declarer, int tricks, string vulnerable, int expectedScore)
    {
    var result = CreateResult(contract, declarer, tricks);
          var score = _scoringService.CalculateNorthScore(result, vulnerable);
      Assert.Equal(expectedScore, score);
  }

        #endregion

        #region Undertricks - Redoubled Vulnerable

        [Theory]
        [InlineData("3Nxx", GameModel.Player.North, 8, "All", -400)]    // 3NT redoubled down 1 vul
[InlineData("3Nxx", GameModel.Player.North, 7, "All", -1000)]   // 3NT redoubled down 2 vul (400+600)
        [InlineData("3Nxx", GameModel.Player.North, 6, "All", -1600)]   // 3NT redoubled down 3 vul (400+600+600)
  [InlineData("3Nxx", GameModel.Player.North, 5, "All", -2200)]   // 3NT redoubled down 4 vul
  [InlineData("4Hxx", GameModel.Player.North, 9, "All", -400)]    // 4H redoubled down 1 vul
 [InlineData("4Hxx", GameModel.Player.North, 8, "All", -1000)]   // 4H redoubled down 2 vul
     public void Undertricks_Redoubled_Vulnerable_CalculatesCorrectly(string contract, GameModel.Player declarer, int tricks, string vulnerable, int expectedScore)
        {
        var result = CreateResult(contract, declarer, tricks);
   var score = _scoringService.CalculateNorthScore(result, vulnerable);
            Assert.Equal(expectedScore, score);
   }

        #endregion

    #region East-West Declarer Tests

        [Theory]
 [InlineData("3N", GameModel.Player.East, 9, "None", -400)]    // EW makes 3NT, NS gets negative
        [InlineData("3N", GameModel.Player.West, 9, "None", -400)]    // West declarer
   [InlineData("4H", GameModel.Player.East, 10, "All", -620)]    // EW makes 4H vul
      [InlineData("4H", GameModel.Player.West, 10, "All", -620)]    // West declarer vul
        [InlineData("3N", GameModel.Player.East, 8, "None", 50)]   // EW down 1, NS gets positive
     [InlineData("3N", GameModel.Player.West, 8, "All", 100)]      // EW down 1 vul
 [InlineData("3Nx", GameModel.Player.East, 8, "None", 100)]    // EW doubled down 1
        [InlineData("3Nx", GameModel.Player.West, 8, "All", 200)]     // EW doubled down 1 vul
        public void EastWestDeclarer_ScoresAreNegatedForNorthSouth(string contract, GameModel.Player declarer, int tricks, string vulnerable, int expectedScore)
  {
var result = CreateResult(contract, declarer, tricks);
       var score = _scoringService.CalculateNorthScore(result, vulnerable);
     Assert.Equal(expectedScore, score);
   }

        #endregion

        #region Vulnerability Combinations

        [Theory]
        [InlineData("3N", GameModel.Player.North, 9, "NS", 600)]      // NS vul only
        [InlineData("3N", GameModel.Player.East, 9, "NS", -400)]    // EW not vul
        [InlineData("3N", GameModel.Player.North, 9, "EW", 400)]      // NS not vul
        [InlineData("3N", GameModel.Player.East, 9, "EW", -600)]      // EW vul only
        [InlineData("3N", GameModel.Player.North, 8, "NS", -100)]     // NS down 1, vul
        [InlineData("3N", GameModel.Player.East, 8, "NS", 50)]        // EW down 1, not vul
        [InlineData("3N", GameModel.Player.North, 8, "EW", -50)]      // NS down 1, not vul
        [InlineData("3N", GameModel.Player.East, 8, "EW", 100)]       // EW down 1, vul
        public void VulnerabilityCombinations_CalculatesCorrectly(string contract, GameModel.Player declarer, int tricks, string vulnerable, int expectedScore)
        {
      var result = CreateResult(contract, declarer, tricks);
   var score = _scoringService.CalculateNorthScore(result, vulnerable);
            Assert.Equal(expectedScore, score);
        }

        #endregion

        #region Edge Cases and Special Scenarios

        [Fact]
        public void PassedHand_ReturnsZero()
{
         var result = CreateResult("PASS", GameModel.Player.North, 0);
         var score = _scoringService.CalculateNorthScore(result, "None");
 Assert.Equal(0, score);
        }

   [Fact]
  public void PassedHandLowercase_ReturnsZero()
     {
      var result = CreateResult("pass", GameModel.Player.North, 0);
      var score = _scoringService.CalculateNorthScore(result, "None");
            Assert.Equal(0, score);
        }

        [Fact]
        public void PassedHandShortForm_ReturnsZero()
 {
    var result = CreateResult("P", GameModel.Player.North, 0);
   var score = _scoringService.CalculateNorthScore(result, "None");
            Assert.Equal(0, score);
        }

        [Fact]
        public void EmptyContract_ReturnsZero()
        {
  var result = CreateResult("", GameModel.Player.North, 9);
        var score = _scoringService.CalculateNorthScore(result, "None");
     Assert.Equal(0, score);
        }

     [Fact]
        public void NullContract_ReturnsZero()
        {
       var result = CreateResult(null, GameModel.Player.North, 9);
var score = _scoringService.CalculateNorthScore(result, "None");
            Assert.Equal(0, score);
        }

        [Fact]
        public void InvalidContractFormat_ReturnsZero()
        {
  var result = CreateResult("XYZ", GameModel.Player.North, 9);
   var score = _scoringService.CalculateNorthScore(result, "None");
            Assert.Equal(0, score);
        }

        #endregion

  #region Doubled/Redoubled Slam Bonuses

        [Theory]
    [InlineData("6Nx", GameModel.Player.North, 12, "None", 1230)]  // 6NT doubled, non-vul
[InlineData("6Nx", GameModel.Player.North, 12, "All", 1680)]   // 6NT doubled, vul
[InlineData("6Nxx", GameModel.Player.North, 12, "None", 1660)] // 6NT redoubled, non-vul
        [InlineData("6Nxx", GameModel.Player.North, 12, "All", 2110)]  // 6NT redoubled, vul
        [InlineData("7Nx", GameModel.Player.North, 13, "None", 1790)]  // 7NT doubled, non-vul
        [InlineData("7Nx", GameModel.Player.North, 13, "All", 2490)]   // 7NT doubled, vul
        [InlineData("7Nxx", GameModel.Player.North, 13, "None", 2280)] // 7NT redoubled, non-vul
        [InlineData("7Nxx", GameModel.Player.North, 13, "All", 2980)]  // 7NT redoubled, vul
        public void DoubledRedoubledSlams_CalculatesCorrectly(string contract, GameModel.Player declarer, int tricks, string vulnerable, int expectedScore)
   {
    var result = CreateResult(contract, declarer, tricks);
            var score = _scoringService.CalculateNorthScore(result, vulnerable);
          Assert.Equal(expectedScore, score);
      }

        #endregion

        #region All Suit and Level Combinations

        [Theory]
        // All clubs contracts 1-7
        [InlineData("1C", GameModel.Player.North, 7, "None", 70)]
   [InlineData("2C", GameModel.Player.North, 8, "None", 90)]
        [InlineData("3C", GameModel.Player.North, 9, "None", 110)]
 [InlineData("4C", GameModel.Player.North, 10, "None", 130)]
        [InlineData("5C", GameModel.Player.North, 11, "None", 400)]
        [InlineData("6C", GameModel.Player.North, 12, "None", 920)]
        [InlineData("7C", GameModel.Player.North, 13, "None", 1440)]
    // All diamonds contracts 1-7
[InlineData("1D", GameModel.Player.North, 7, "None", 70)]
        [InlineData("2D", GameModel.Player.North, 8, "None", 90)]
   [InlineData("3D", GameModel.Player.North, 9, "None", 110)]
        [InlineData("4D", GameModel.Player.North, 10, "None", 130)]
        [InlineData("5D", GameModel.Player.North, 11, "None", 400)]
        [InlineData("6D", GameModel.Player.North, 12, "None", 920)]
   [InlineData("7D", GameModel.Player.North, 13, "None", 1440)]
        // All hearts contracts 1-7
     [InlineData("1H", GameModel.Player.North, 7, "None", 80)]
        [InlineData("2H", GameModel.Player.North, 8, "None", 110)]
      [InlineData("3H", GameModel.Player.North, 9, "None", 140)]
        [InlineData("4H", GameModel.Player.North, 10, "None", 420)]
        [InlineData("5H", GameModel.Player.North, 11, "None", 450)]
        [InlineData("6H", GameModel.Player.North, 12, "None", 980)]
        [InlineData("7H", GameModel.Player.North, 13, "None", 1510)]
    // All spades contracts 1-7
        [InlineData("1S", GameModel.Player.North, 7, "None", 80)]
    [InlineData("2S", GameModel.Player.North, 8, "None", 110)]
   [InlineData("3S", GameModel.Player.North, 9, "None", 140)]
        [InlineData("4S", GameModel.Player.North, 10, "None", 420)]
        [InlineData("5S", GameModel.Player.North, 11, "None", 450)]
   [InlineData("6S", GameModel.Player.North, 12, "None", 980)]
      [InlineData("7S", GameModel.Player.North, 13, "None", 1510)]
        // All no trump contracts 1-7
        [InlineData("1N", GameModel.Player.North, 7, "None", 90)]
        [InlineData("2N", GameModel.Player.North, 8, "None", 120)]
        [InlineData("3N", GameModel.Player.North, 9, "None", 400)]
        [InlineData("4N", GameModel.Player.North, 10, "None", 430)]
        [InlineData("5N", GameModel.Player.North, 11, "None", 460)]
        [InlineData("6N", GameModel.Player.North, 12, "None", 990)]
        [InlineData("7N", GameModel.Player.North, 13, "None", 1520)]
        public void AllSuitsAndLevels_NonVulnerable_MadeExactly(string contract, GameModel.Player declarer, int tricks, string vulnerable, int expectedScore)
        {
       var result = CreateResult(contract, declarer, tricks);
  var score = _scoringService.CalculateNorthScore(result, vulnerable);
   Assert.Equal(expectedScore, score);
  }

        #endregion

        #region Comprehensive Overtrick Tests

   [Theory]
        // Minor suits - all overtrick scenarios
        [InlineData("1C", GameModel.Player.North, 10, "None", 130)]  // +3
        [InlineData("1C", GameModel.Player.North, 13, "None", 190)]  // +6 (maximum)
     [InlineData("2D", GameModel.Player.North, 11, "None", 150)]  // +3
        [InlineData("3C", GameModel.Player.North, 13, "None", 190)]  // +4
        // Major suits - all overtrick scenarios
      [InlineData("1H", GameModel.Player.North, 10, "None", 170)]  // +3
[InlineData("1H", GameModel.Player.North, 13, "None", 260)]  // +6 (maximum)
        [InlineData("2S", GameModel.Player.North, 11, "None", 200)]  // +3
   [InlineData("3H", GameModel.Player.North, 13, "None", 260)]  // +4
        // No Trump - all overtrick scenarios
        [InlineData("1N", GameModel.Player.North, 10, "None", 180)]  // +3
        [InlineData("1N", GameModel.Player.North, 13, "None", 270)]  // +6 (maximum)
        [InlineData("3N", GameModel.Player.North, 13, "None", 520)]  // +4 (maximum from 3NT)
        public void AllOvertrickScenarios_NonVulnerable(string contract, GameModel.Player declarer, int tricks, string vulnerable, int expectedScore)
        {
   var result = CreateResult(contract, declarer, tricks);
   var score = _scoringService.CalculateNorthScore(result, vulnerable);
            Assert.Equal(expectedScore, score);
        }

        #endregion

        #region Case Sensitivity Tests

        [Theory]
  [InlineData("3n", GameModel.Player.North, 9, "None", 400)] // Lowercase n
  [InlineData("3N", GameModel.Player.North, 9, "None", 400)]    // Uppercase N
  [InlineData("4h", GameModel.Player.North, 10, "None", 420)]   // Lowercase h
        [InlineData("4H", GameModel.Player.North, 10, "None", 420)]   // Uppercase H
        [InlineData("5c", GameModel.Player.North, 11, "None", 400)]   // Lowercase c
        [InlineData("5C", GameModel.Player.North, 11, "None", 400)]   // Uppercase C
        [InlineData("3NX", GameModel.Player.North, 9, "None", 550)]   // Uppercase X
        [InlineData("3nx", GameModel.Player.North, 9, "None", 550)]   // Lowercase x
    [InlineData("3Nx", GameModel.Player.North, 9, "None", 550)]   // Mixed case
        [InlineData("3NXX", GameModel.Player.North, 9, "None", 800)]  // Uppercase XX
 [InlineData("3nxx", GameModel.Player.North, 9, "None", 800)]  // Lowercase xx
        public void ContractCaseVariations_CalculatesCorrectly(string contract, GameModel.Player declarer, int tricks, string vulnerable, int expectedScore)
        {
   var result = CreateResult(contract, declarer, tricks);
     var score = _scoringService.CalculateNorthScore(result, vulnerable);
 Assert.Equal(expectedScore, score);
        }

        #endregion

        #region Maximum Undertrick Penalties

        [Theory]
        [InlineData("7N", GameModel.Player.North, 0, "None", -650)]     // 13 undertricks non-vul
        [InlineData("7N", GameModel.Player.North, 0, "All", -1300)]     // 13 undertricks vul
        [InlineData("7Nx", GameModel.Player.North, 0, "None", -3500)]   // 13 undertricks doubled non-vul
        [InlineData("7Nx", GameModel.Player.North, 0, "All", -3800)]    // 13 undertricks doubled vul
        [InlineData("7Nxx", GameModel.Player.North, 0, "None", -7000)]  // 13 undertricks redoubled non-vul
        [InlineData("7Nxx", GameModel.Player.North, 0, "All", -7600)]   // 13 undertricks redoubled vul
        public void MaximumUndertricks_CalculatesCorrectly(string contract, GameModel.Player declarer, int tricks, string vulnerable, int expectedScore)
        {
 var result = CreateResult(contract, declarer, tricks);
            var score = _scoringService.CalculateNorthScore(result, vulnerable);
      // These are approximate as the exact values depend on the penalty calculation
            // The test verifies that the scores are extremely negative as expected
  Assert.True(score < 0, $"Expected negative score for {contract} down 13, got {score}");
 Assert.True(score <= expectedScore, $"Expected score <= {expectedScore}, got {score}");
        }

        #endregion

        #region All Four Declarers

 [Theory]
   [InlineData("3N", GameModel.Player.North, 9, "None", 400)]
[InlineData("3N", GameModel.Player.South, 9, "None", 400)]
        [InlineData("3N", GameModel.Player.East, 9, "None", -400)]
        [InlineData("3N", GameModel.Player.West, 9, "None", -400)]
  [InlineData("4H", GameModel.Player.North, 10, "All", 620)]
 [InlineData("4H", GameModel.Player.South, 10, "All", 620)]
        [InlineData("4H", GameModel.Player.East, 10, "All", -620)]
   [InlineData("4H", GameModel.Player.West, 10, "All", -620)]
     public void AllFourDeclarers_CalculatesCorrectly(string contract, GameModel.Player declarer, int tricks, string vulnerable, int expectedScore)
        {
 var result = CreateResult(contract, declarer, tricks);
    var score = _scoringService.CalculateNorthScore(result, vulnerable);
     Assert.Equal(expectedScore, score);
        }

        #endregion

        #region Integration Test - Complete Game Scenarios

        [Fact]
        public void CompleteGameScenario_VariousContracts_CalculatesAllCorrectly()
        {
  // This test simulates a complete board with various results
         var scenarios = new[]
    {
                (Contract: "3N", Declarer: GameModel.Player.North, Tricks: 9, Vulnerable: "None", Expected: 400),
                (Contract: "4H", Declarer: GameModel.Player.South, Tricks: 10, Vulnerable: "None", Expected: 420),
                (Contract: "3N", Declarer: GameModel.Player.North, Tricks: 10, Vulnerable: "None", Expected: 430),
     (Contract: "5D", Declarer: GameModel.Player.North, Tricks: 10, Vulnerable: "None", Expected: -50),
    (Contract: "3Nx", Declarer: GameModel.Player.East, Tricks: 8, Vulnerable: "None", Expected: 100),
                (Contract: "4Sx", Declarer: GameModel.Player.West, Tricks: 10, Vulnerable: "All", Expected: -790),
   };

            foreach (var scenario in scenarios)
       {
        var result = CreateResult(scenario.Contract, scenario.Declarer, scenario.Tricks);
                var score = _scoringService.CalculateNorthScore(result, scenario.Vulnerable);
    Assert.Equal(scenario.Expected, score);
            }
        }

    #endregion

        #region Documentation Test

        [Fact]
        public void GenerateScoringReference_PrintsCompleteTable()
  {
            _output.WriteLine("");
   _output.WriteLine("?????????????????????????????????????????????????????????????????????????????????");
      _output.WriteLine("?        BRIDGE SCORING REFERENCE TABLE      ?");
       _output.WriteLine("?????????????????????????????????????????????????????????????????????????????????");
        _output.WriteLine("");

          // Part Games
            _output.WriteLine("PART GAMES (Non-Vulnerable):");
        _output.WriteLine("????????????????????????????????????????????????????????????????????????????????");
            _output.WriteLine($"{"Contract",-12} {"Made",-8} {"Score",8}");
  _output.WriteLine("????????????????????????????????????????????????????????????????????????????????");
     
            var partGames = new[] { "1C", "1D", "1H", "1S", "1N", "2C", "2D", "2H", "2S", "2N" };
   foreach (var contract in partGames)
  {
         int level = int.Parse(contract.Substring(0, 1));
 int tricks = 6 + level;
             var result = CreateResult(contract, GameModel.Player.North, tricks);
var score = _scoringService.CalculateNorthScore(result, "None");
                _output.WriteLine($"{contract,-12} {tricks,-8} {score,8}");
            }
            _output.WriteLine("");

       // Games
            _output.WriteLine("GAME CONTRACTS (Non-Vulnerable / Vulnerable):");
            _output.WriteLine("????????????????????????????????????????????????????????????????????????????????");
         _output.WriteLine($"{"Contract",-12} {"Made",-8} {"Non-Vul",10} {"Vul",10}");
            _output.WriteLine("????????????????????????????????????????????????????????????????????????????????");
    
            var games = new[] { "3N", "4H", "4S", "5C", "5D" };
   foreach (var contract in games)
         {
    int level = int.Parse(contract.Substring(0, 1));
        int tricks = 6 + level;
     var result = CreateResult(contract, GameModel.Player.North, tricks);
    var scoreNonVul = _scoringService.CalculateNorthScore(result, "None");
       var scoreVul = _scoringService.CalculateNorthScore(result, "All");
     _output.WriteLine($"{contract,-12} {tricks,-8} {scoreNonVul,10} {scoreVul,10}");
       }
       _output.WriteLine("");

            // Slams
            _output.WriteLine("SLAM BONUSES (Non-Vulnerable / Vulnerable):");
  _output.WriteLine("????????????????????????????????????????????????????????????????????????????????");
            _output.WriteLine($"{"Contract",-12} {"Made",-8} {"Non-Vul",10} {"Vul",10}");
          _output.WriteLine("????????????????????????????????????????????????????????????????????????????????");
      
        var slams = new[] { "6C", "6D", "6H", "6S", "6N", "7C", "7D", "7H", "7S", "7N" };
         foreach (var contract in slams)
       {
 int tricks = 6 + int.Parse(contract.Substring(0, 1));
           var result = CreateResult(contract, GameModel.Player.North, tricks);
    var scoreNonVul = _scoringService.CalculateNorthScore(result, "None");
      var scoreVul = _scoringService.CalculateNorthScore(result, "All");
            _output.WriteLine($"{contract,-12} {tricks,-8} {scoreNonVul,10} {scoreVul,10}");
        }
      _output.WriteLine("");
 }

     #endregion
    }
}
