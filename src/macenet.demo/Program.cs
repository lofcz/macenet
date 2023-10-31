namespace macenet.demo;

class Program
{
    public static void Main()
    {
        List<MaceAnnotation> annotations = new List<MaceAnnotation>
        {
            // params: annotator, item, chosen label 
            new MaceAnnotation(1, 0, 0),
            new MaceAnnotation(1, 1, 0),
            new MaceAnnotation(2, 0, 1),
            new MaceAnnotation(2, 1, 1),
            new MaceAnnotation(3, 1, 1),
            new MaceAnnotation(3, 2, 0)
        };

        List<MaceControlLabel> controls = new List<MaceControlLabel>
        {
            new MaceControlLabel(0, 1)
        };

        MaceResult result = Mace.Evaluate(annotations, null, controls);

        Console.WriteLine("Starting MACE .NET");
        Console.WriteLine("----------------------------");

        Console.WriteLine();
        Console.WriteLine("Predictions for items");
        Console.WriteLine("----------------------------");

        foreach (MaceResultItemLabel prediction in result.Labels)
        {
            Console.WriteLine($"Predictions for item {prediction.Item}");

            foreach (MaceLabel label in prediction.Labels)
            {
                Console.WriteLine($"-- {label}");
            }
        }

        Console.WriteLine();
        Console.WriteLine("Annotators reliability");
        Console.WriteLine("----------------------------");

        foreach (MaceResultAnnotatorReliability annotator in result.Annotators)
        {
            Console.WriteLine(annotator);
        }

        Console.WriteLine();
        Console.WriteLine("Item entropies");
        Console.WriteLine("----------------------------");

        foreach (MaceResultItemEntropy entropy in result.Entropies)
        {
            Console.WriteLine(entropy);
        }

        Console.WriteLine();
        Console.Write("Press any key to close the program.");
        Console.ReadKey();
    }
}