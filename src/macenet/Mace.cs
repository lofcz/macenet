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

public record MaceInput(int[][] WhoLabeled, int[][] Labels, HashSet<int> LabelOptions, HashSet<int> Annotators);

public class MaceSettings
{
    public static readonly MaceSettings Default = new MaceSettings();
    
    public int Iterations { get; set; } = 50;
    public int Restarts { get; set; } = 10;
    public double Noise { get; set; } = 0.5;
    public double Alpha { get; set; } = 0.5;
    public double Beta { get; set; } = 0.5;
    public double Smoothing { get; set; } = 0.01;
    public double Threshold { get; set; } = 1;
}

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
        HashSet<int> options = new HashSet<int>();
        HashSet<int> annotators = new HashSet<int>();

        for (int i = 0; i < items; i++)
        {
            whoLabeled.Add(new List<int>());
            labels.Add(new List<int>());
        }
            
        foreach (MaceAnnotation annotation in annotations)
        {
            whoLabeled[annotation.Item].Add(annotation.Annotator);
            labels[annotation.Item].Add(annotation.Choice);
            options.Add(annotation.Choice);
            annotators.Add(annotation.Annotator);
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
        
        return new MaceInput(whoLabeledArr, labelsArr, options, annotators);
    }
    
    public static void Evaluate(IEnumerable<MaceAnnotation> annotations, MaceSettings? settings = null)
    {
        settings ??= MaceSettings.Default;
        double[][] spamming, thetas, strategyExpectedCounts, knowingExpectedCounts, thetaPriors, strategyPriors, goldLabelMarginals;
        double logMarginalLikelyHood;
        double[] entropies;
        List<MaceAnnotation> annotationsList = annotations.ToList();
        MaceInput input = ParseInput(annotationsList);
        int nInstances = input.Labels.Length;
        int nLabels = input.LabelOptions.Count;
        int nAnnotators = input.Annotators.Count;
        goldLabelMarginals = Utils.CreateJaggedArray<double[][]>(nInstances, nLabels);
        strategyExpectedCounts = Utils.CreateJaggedArray<double[][]>(nAnnotators, nLabels);
        knowingExpectedCounts = Utils.CreateJaggedArray<double[][]>(nAnnotators, 2);
        double smoothing = settings.Smoothing / nLabels;

        Dictionary<int, int> controlLabels = new Dictionary<int, int>();

        double[][] bestThetas = Utils.CreateJaggedArray<double[][]>(nAnnotators, 2);
        double[][] bestStrategies = Utils.CreateJaggedArray<double[][]>(nAnnotators, nLabels);
        double bestLogMarginalLikelihood = 0;
        int bestModelAtRestart = 0;

        for (int i = 0; i < settings.Restarts; i++)
        {
            InitializeRun();
        }
        
        return;

        void InitializeRun()
        {
            spamming = Utils.CreateJaggedArray<double[][]>(nAnnotators, 2);
            thetas = Utils.CreateJaggedArray<double[][]>(nAnnotators, nLabels);

            for (int i = 0; i < nAnnotators; ++i)
            {
                Array.Fill(spamming[i], 1.0);
                Array.Fill(thetas[i], 1.0);
                spamming[i][0] += settings.Noise * Random.Shared.NextDouble();
                spamming[i][1] += settings.Noise * Random.Shared.NextDouble();

                for (int j = 0; j < nLabels; ++j)
                {
                    thetas[i][j] += settings.Noise * Random.Shared.NextDouble();
                }
                
                Math2.NormalizeInPlace(spamming, 0);
                Math2.NormalizeInPlace(thetas, 0);
            }
        }
    }
}