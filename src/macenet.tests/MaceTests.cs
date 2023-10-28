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
        });
    }
}