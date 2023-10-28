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
}