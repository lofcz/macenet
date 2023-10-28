namespace macenet.tests;

public class MaceTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void ParseAnnotations()
    {
        /* Equivalent of:
           0,1,
           0,0,0
           ,,1
        */
        List<MaceAnnotation> annotations = new List<MaceAnnotation>
        {
            new MaceAnnotation(0, 0, 0),
            new MaceAnnotation(0, 1, 0),
            new MaceAnnotation(1, 0, 1),
            new MaceAnnotation(1, 1, 0),
            new MaceAnnotation(2, 1, 0),
            new MaceAnnotation(2, 2, 1)
        };
        
        MaceInput result = Mace.ParseInput(annotations);
        
        Assert.Multiple(() =>
        {
            Assert.That(result.WhoLabeled.Length is 3);
            Assert.That(result.WhoLabeled[0].Length is 2);
            Assert.That(result.WhoLabeled[1].Length is 3);
            Assert.That(result.WhoLabeled[2].Length is 1);
            Assert.That(result.WhoLabeled[2][0] is 2);
            Assert.That(result.WhoLabeled[1][2] is 2);
            Assert.That(result.Labels.Length is 3);
            Assert.That(result.Labels[2][0] is 1);
            Assert.That(result.Labels[1][2] is 0);
            Assert.That(result.LabelOptions.Count is 2);
            Assert.That(result.Annotators.Count is 3);
        });
    }

    [Test]
    public void TestMaceRun()
    {
        List<MaceAnnotation> annotations = new List<MaceAnnotation>
        {
            new MaceAnnotation(0, 0, 0),
            new MaceAnnotation(0, 1, 0),
            new MaceAnnotation(1, 0, 1),
            new MaceAnnotation(1, 1, 0),
            new MaceAnnotation(2, 1, 0),
            new MaceAnnotation(2, 2, 1)
        };
        
        Mace.Evaluate(annotations);
    }

    [Test]
    public void TestMaceEStep()
    {
        List<MaceAnnotation> annotations = new List<MaceAnnotation>
        {
            new MaceAnnotation(0, 0, 0),
            new MaceAnnotation(0, 1, 0),
            new MaceAnnotation(1, 0, 1),
            new MaceAnnotation(1, 1, 0),
            new MaceAnnotation(2, 1, 0),
            new MaceAnnotation(2, 2, 1)
        };
        
        MaceResult result = Mace.Evaluate(annotations, new MaceSettings
        {
            Restarts = 1,
            Test = new MaceTestSettings
            {
                FinalStep = MaceTestSettings.StepToEnd.FirstEStep,
                Init = new MaceTestInit
                {
                    Spamming = new []
                    {
                        new [] { 0.47471836852079985, 0.5252816314792003 },
                        new [] { 0.4414416076323577, 0.5585583923676424 },
                        new [] { 0.4435783695736586, 0.5564216304263414 }
                    },
                    Thetas = new []
                    {
                        new [] { 0.4722741468448161, 0.5252816314792003 },
                        new [] { 0.46347409886906277, 0.5365259011309373 },
                        new [] { 0.4636635306893129, 0.5363364693106871 }
                    },
                    StrategyPriors = new []
                    {
                        new [] { 10.0, 10.0 },
                        new [] { 10.0, 10.0 },
                        new [] { 10.0, 10.0 }
                    },
                    ThetaPriors = new []
                    {
                        new [] { 0.5, 0.5 },
                        new [] { 0.5, 0.5 },
                        new [] { 0.5, 0.5 }
                    }
                }
            }
        });
        
        if (result.TestResult is null)
        {
            Assert.Fail("testResult should not be null");
            return;
        }
        
        /*
         * log marginal likelihood = -3.8899412692745896
         * knowingExpectedCounts = 0.9643583727769215, 1.0356416272230784
         *                         0.931675875679548,  1.0683241243204522
         *                         0.7462993126909225, 1.2537006873090775
         * strategyExpectedCounts = 0.9643583727769215, 0
         *                          0.2835985079817251, 0.6480773676978229
         *                          0.2853442142498572, 0.4609550984410654
         * goldLabelMarginals = 0.08875510456111975, 0.08916359557983634
         *                      0.21794657303270415, 0.004717069169030616
         *                      0.11895362829986358, 0.39716444351303426
         */
        
        Assert.That(Utils.EqualsWithinTolerance(result.TestResult.LogMarginalLikelihood, -3.8899412692745896));
        Assert.That(Utils.EqualsWithinTolerance(result.TestResult.KnowingExpectedCounts[0][0], 0.9643583727769215));
        Assert.That(Utils.EqualsWithinTolerance(result.TestResult.KnowingExpectedCounts[0][1], 1.0356416272230784));
        Assert.That(Utils.EqualsWithinTolerance(result.TestResult.KnowingExpectedCounts[1][0], 0.931675875679548));
        Assert.That(Utils.EqualsWithinTolerance(result.TestResult.KnowingExpectedCounts[1][1], 1.0683241243204522));
        Assert.That(Utils.EqualsWithinTolerance(result.TestResult.KnowingExpectedCounts[2][0], 0.7462993126909225));
        Assert.That(Utils.EqualsWithinTolerance(result.TestResult.KnowingExpectedCounts[2][1], 1.2537006873090775));
        Assert.That(Utils.EqualsWithinTolerance(result.TestResult.StrategyExpectedCounts[0][0], 0.9643583727769215));
        Assert.That(Utils.EqualsWithinTolerance(result.TestResult.StrategyExpectedCounts[0][1], 0));
        Assert.That(Utils.EqualsWithinTolerance(result.TestResult.StrategyExpectedCounts[1][0], 0.2835985079817251));
        Assert.That(Utils.EqualsWithinTolerance(result.TestResult.StrategyExpectedCounts[1][1], 0.6480773676978229));
        Assert.That(Utils.EqualsWithinTolerance(result.TestResult.StrategyExpectedCounts[2][0], 0.2853442142498572));
        Assert.That(Utils.EqualsWithinTolerance(result.TestResult.StrategyExpectedCounts[2][1], 0.4609550984410654));
        Assert.That(Utils.EqualsWithinTolerance(result.TestResult.GoldLabelMarginals[0][0], 0.08875510456111975));
        Assert.That(Utils.EqualsWithinTolerance(result.TestResult.GoldLabelMarginals[0][1], 0.08916359557983634));
        Assert.That(Utils.EqualsWithinTolerance(result.TestResult.GoldLabelMarginals[1][0], 0.21794657303270415));
        Assert.That(Utils.EqualsWithinTolerance(result.TestResult.GoldLabelMarginals[1][1], 0.004717069169030616));
        Assert.That(Utils.EqualsWithinTolerance(result.TestResult.GoldLabelMarginals[2][0], 0.11895362829986358));
        Assert.That(Utils.EqualsWithinTolerance(result.TestResult.GoldLabelMarginals[2][1], 0.39716444351303426));
    }
}