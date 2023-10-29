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

/// <summary>
/// Represents a control, ground truth label (<see cref="Choice"/>) of a given <see cref="Item"/>
/// </summary>
/// <param name="Item"></param>
/// <param name="Choice"></param>
public record MaceControlLabel(int Item, int Choice);

public record MaceInput(int[][] WhoLabeled, int[][] Labels, HashSet<int> LabelOptions, HashSet<int> Annotators);

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
    public static readonly MaceSettings Default = new MaceSettings();
    
    public int Iterations { get; set; } = 50;
    public int Restarts { get; set; } = 10;
    public double Noise { get; set; } = 0.5;
    public double Alpha { get; set; } = 0.5;
    public double Beta { get; set; } = 0.5;
    public double Smoothing { get; set; } = 0.01;
    public double Threshold { get; set; } = 1;
    public MaceTestSettings? Test { get; set; }
}

public class MaceTestResult
{
    public double LogMarginalLikelihood { get; set; }
    public double[][] KnowingExpectedCounts { get; set; }
    public double[][] StrategyExpectedCounts { get; set; }
    public double[][] GoldLabelMarginals { get; set; }
}

public class MaceResultItem
{
    public int Item { get; set; }
    public MaceLabel GoldLabel { get; set; } = default!;
    public List<MaceLabel> Labels { get; set; } = new List<MaceLabel>();
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
    public MaceResultItem[] Items { get; internal set; } = default!;
    public MaceTestResult? TestResult { get; internal set; }
}