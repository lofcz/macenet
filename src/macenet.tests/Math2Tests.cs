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
}