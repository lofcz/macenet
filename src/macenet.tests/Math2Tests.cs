namespace macenet.tests;

public class Tests
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
    [TestCase(1, -7.915932957214796e-9)]
    [TestCase(1.5, -0.17425195003632513d)]
    [TestCase(4, 2.5849624928052233d)]
    public void TestLog2Gamma(double n, double expected)
    {
        Assert.That(Utils.EqualsWithinTolerance(Math2.Log2Gamma(n), expected));
    }
}