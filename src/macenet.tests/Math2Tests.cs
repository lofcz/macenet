namespace macenet.tests;

public class Math2Tests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    [TestCase(6703)]
    [TestCase(7867)]
    [TestCase(2)]
    [TestCase(3)]
    [TestCase(5569)]
    [TestCase(939391)]
    public void TestIsPrime(int n)
    {
        Assert.That(Math2.IsPrime(n));
    }
    
    [Test]
    [TestCase(6713)]
    [TestCase(30129)]
    [TestCase(1)]
    [TestCase(5868)]
    [TestCase(939392)]
    public void TestIsNotPrime(int n)
    {
        Assert.That(!Math2.IsPrime(n));
    }

    [Test]
    [TestCase(8, 11.541560327111707d)]
    [TestCase(15, 21.64042561333445d)]
    [TestCase(18, 25.968510736001342d)]
    public void TestNaturalLogToBase2Log(double n, double expected)
    {
        Assert.That(Utils.EqualsWithinTolerance(Math2.NaturalLogToBase2Log(n), expected));
    }
    
    [Test]
    [TestCase(545, 9.090112419664289d)]
    [TestCase(13, 3.700439718141092d)]
    [TestCase(18, 4.169925001442312d)]
    public void TestLog2(double n, double expected)
    {
        Assert.That(Utils.EqualsWithinTolerance(Math2.Log2(n), expected));
    }

    [Test]
    [TestCase(0.5, 1.7724538473485887d)]
    [TestCase(1, 0.9999999945130934d)]
    [TestCase(1.5, 0.8862269163782321d)]
    [TestCase(0.8, 1.164229709152094d)]
    [TestCase(1.3, 0.8974706889443992d)]
    public void TestLanczosGamma(double n, double expected)
    {
        Assert.That(Utils.EqualsWithinTolerance(Math2.LanczosGamma(n), expected));
    }
    
    [Test]
    [TestCase(0.01, 6.635646818516083d)]
    [TestCase(0.1, 3.249977377957902d)]
    [TestCase(0.5, 0.8257480618409858d)]
    [TestCase(1, -7.915932957214796e-9d)]
    [TestCase(1.5, -0.17425195003632513d)]
    [TestCase(4, 2.5849624928052233d)]
    public void TestLog2Gamma(double n, double expected)
    {
        Assert.That(Utils.EqualsWithinTolerance(Math2.Log2Gamma(n), expected));
    }

    [Test]
    [TestCase(0, double.NaN)]
    [TestCase(0.000000000001, -1.0000000000005773e12d)]
    [TestCase(-0.000000000001, 9.999312366085045e11d)]
    [TestCase(-0.5, 0.03648997397857712d)]
    [TestCase(1, -0.5772156649015329d)]
    [TestCase(2, 0.42278433509846713d)]
    [TestCase(3, 0.9227843350984671d)]
    [TestCase(4, 1.2561176684318005d)]
    [TestCase(5, 1.5061176684318003d)]
    public void TestDigamma(double n, double expected)
    {
        Assert.That(Utils.EqualsWithinTolerance(Math2.Digamma(n), expected));
    }

    [Test]
    public void TestNormalizeInPlace()
    {
        double[][] mat = 
        {
            new[] { 1.0, 0.87 },
            new[] { 3.24, 8.55 }
        };
        
        Math2.NormalizeInPlace(mat, 0);
        
        Assert.Multiple(() =>
        {
            Assert.That(Utils.EqualsWithinTolerance(mat[0][0], 0.53475935828877d));
            Assert.That(Utils.EqualsWithinTolerance(mat[0][1], 0.4652406417112299d));
            Assert.That(Utils.EqualsWithinTolerance(mat[1][0], 0.2748091603053435d));
            Assert.That(Utils.EqualsWithinTolerance(mat[1][1], 0.7251908396946565));
        });
    }
    
    [Test]
    public void TestVariationalNormalize()
    {
        double[][] mat =
        {
            new[] { 0.9643583727769215, 1.0356416272230784 },
            new[] { 0.931675875679548, 1.0683241243204522 },
            new [] { 0.7462993126909225, 1.2537006873090775 }
        };

        double[][] hyperparameters =
        {
            new [] { 0.5, 0.5 },
            new [] { 0.5, 0.5 },
            new [] { 0.5, 0.5 }
        };

        double[][] result = Math2.VariationalNormalize(mat, hyperparameters);
        
        Assert.That(Utils.EqualsWithinTolerance(result[0][0], 0.39845945672412214d));
        Assert.That(Utils.EqualsWithinTolerance(result[0][1], 0.42592478657324556d));
        Assert.That(Utils.EqualsWithinTolerance(result[1][0], 0.3859001602730679d));
        Assert.That(Utils.EqualsWithinTolerance(result[1][1], 0.438547378824097d));
        Assert.That(Utils.EqualsWithinTolerance(result[2][0], 0.31515902552688185d));
        Assert.That(Utils.EqualsWithinTolerance(result[2][1], 0.5104370189482136d));
    }
}