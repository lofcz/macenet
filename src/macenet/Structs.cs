namespace macenet;

/// <summary>
/// Represents an annotation given by a <see cref="Annotator"/> to an <see cref="Item"/>.
/// For example: The second annotator annotated the third item in the dataset as VERB (from options VERB = 1, ADJ = 2, NOUN = 3) 
/// would be represented as: new MaceAnnotation(1, 2, 1)
/// </summary>
/// <param name="Item">Index of the item in the dataset</param>
/// <param name="Annotator">Index of the annotator</param>
/// <param name="Choice">Index of the annotator's choice</param>
public record MaceAnnotation(int Annotator, int Item, int Choice);

/// <summary>
/// Represents a control, ground truth label (<see cref="Choice"/>) of a given <see cref="Item"/>
/// </summary>
/// <param name="Item"></param>
/// <param name="Choice"></param>
public record MaceControlLabel(int Item, int Choice);

/// <summary>
/// Represents parsed MACE input. This is only needed when manually parsing data. For most use-cases, prefer <see cref="Mace.Evaluate"/>
/// </summary>
/// <param name="WhoLabeled"></param>
/// <param name="Labels"></param>
/// <param name="LabelOptions">Key = sequential, Value = provided</param>
/// <param name="Annotators">Key = sequential, Value = provided</param>
public record MaceInput(int[][] WhoLabeled, int[][] Labels, Dictionary<int, int> LabelOptions, Dictionary<int, int> Annotators);

/// <summary>
/// Represents a final <see cref="Reliability"/> of a <see cref="Annotator"/>.
/// </summary>
/// <param name="Annotator">Index of an annotator</param>
/// <param name="Reliability">0..100%</param>
public record MaceResultAnnotatorReliability(int Annotator, double Reliability)
{
    public override string ToString()
    {
        return $"Annotator {Annotator} reliability: {Reliability:N5}%";
    }
}

/// <summary>
/// Represents difficulty of a <see cref="Item"/>. If most anotators labeled the <see cref="Item"/> with the same options, the <see cref="Entropy"/> will be low.
/// </summary>
/// <param name="Item">The item</param>
/// <param name="Entropy">The difficulty</param>
public record MaceResultItemEntropy(int Item, double Entropy)
{
    public override string ToString()
    {
        return $"Item {Item} entropy: {Entropy}";
    }
}

public class MaceTestInit
{
    public double[][] Spamming { get; set; }
    public double[][] Thetas { get; set; }
    public double[][] ThetaPriors { get; set; }
    public double[][] StrategyPriors { get; set; }
}

public class MaceTestSettings
{
    public enum StepToEnd
    {
        RunToEnd,
        FirstEStep
    }
    
    public MaceTestInit? Init { get; set; }
    public StepToEnd FinalStep { get; set; } = StepToEnd.RunToEnd;
}

public class MaceSettings
{
    public static readonly MaceSettings Default = new();
    
    public int Iterations { get; set; } = 50;
    public int Restarts { get; set; } = 10;
    public double Noise { get; set; } = 0.5;
    public double Alpha { get; set; } = 0.5;
    public double Beta { get; set; } = 0.5;
    public double Threshold { get; set; } = 1;
    public MaceTestSettings? Test { get; set; }
    public MaceCallbackSettings? Callbacks { get; set; }
}

public class MaceCallbackSettings
{
    /// <summary>
    /// First param in the iteration, second the restart index
    /// </summary>
    public Action<int, int>? OnIteration;
    public Action<int>? OnRestart;
}

public class MaceTestResult
{
    public double LogMarginalLikelihood { get; init; }
    public double[][] KnowingExpectedCounts { get; init; }
    public double[][] StrategyExpectedCounts { get; init; }
    public double[][] GoldLabelMarginals { get; init; }
}

public class MaceResultItemLabel
{
    public int Item { get; set; }
    public MaceLabel GoldLabel { get; set; } = default!;
    public List<MaceLabel> Labels { get; set; } = new();
}

public record MaceLabel(int Option, double Trust)
{
    public override string ToString()
    {
        return $"Option {Option}, trust {Trust:N5}%";
    }
}
    
public class MaceResult
{
    public double BestLogMarginalLikelihood { get; set; }
    public int BestModelAtRestart { get; set; }
    public MaceResultItemLabel[] Labels { get; internal set; } = default!;
    public MaceTestResult? TestResult { get; internal set; }
    public MaceResultAnnotatorReliability[] Annotators { get; internal set; } = default!;
    public MaceResultItemEntropy[] Entropies { get; set; }
}