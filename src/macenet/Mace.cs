namespace macenet;

/// <summary>
/// Represents an annotation given by a <see cref="Annotator"/> to an <see cref="Item"/>.
/// For example: The second annotator annotated the third item in the dataset as VERB (from options VERB = 1, ADJ = 2, NOUN = 3) 
/// would be represented as: new MaceAnnotation(1, 2, 1)
/// </summary>
/// <param name="Item">Index of the item in the dataset, this must start from 0 and be sequential with no gaps</param>
/// <param name="Annotator">Index of the annotator, this must start from 0 and be sequential with no gaps</param>
/// <param name="Choice">Index of the annotator's choice, this must start from 1 and be sequential with no gaps</param>
public record MaceAnnotation(int Annotator, int Item, int Choice);

public record MaceInput(int[][] WhoLabeled, int[][] Labels);

public static class Mace
{
    private const int DefaultRestarts = 10;
    private const int DefaultIterations = 50;
    private const double DefaultNoise = 0.5;
    private const double DefaultAlpha = 0.5;
    private const double DefaultBeta = 0.5;

    public static MaceInput ParseInput(List<MaceAnnotation> annotations)
    {
        int items = annotations.Max(x => x.Item) + 1;
        List<List<int>> whoLabeled = new();
        List<List<int>> labels = new List<List<int>>();

        for (int i = 0; i < items; i++)
        {
            whoLabeled.Add(new List<int>());
            labels.Add(new List<int>());
        }
            
        foreach (MaceAnnotation annotation in annotations)
        {
            whoLabeled[annotation.Item].Add(annotation.Annotator);
            labels[annotation.Item].Add(annotation.Choice);
        }

        int[][] whoLabeledArr = new int[items][];
        int[][] labelsArr = new int[items][];

        for (int i = 0; i < whoLabeled.Count; i++)
        {
            whoLabeledArr[i] = whoLabeled[i].ToArray();
        }

        for (int i = 0; i < labels.Count; i++)
        {
            labelsArr[i] = labels[i].ToArray();
        }
        
        return new MaceInput(whoLabeledArr, labelsArr);
    }
    
    public static void Evaluate(IEnumerable<MaceAnnotation> annotations)
    {
        MaceInput input;
        int nInstances, nAnnotators, nLables;
        double[][] spamming, thetas, goldLabelMarginals, strategyExpectedCounts, knowingExpectedCounts, thetaPriors, strategyPriors;
        double logMarginalLikelyHood;
        double[] entropies;
    
        List<MaceAnnotation> annotationsList = annotations.ToList();

        input = ParseInput(annotationsList);
    }
}