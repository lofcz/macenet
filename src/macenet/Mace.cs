using System.ComponentModel;

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

public class MaceResult 
{
    
    public MaceTestResult? TestResult { get; set; }
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
    
    public static MaceResult Evaluate(IEnumerable<MaceAnnotation> annotations, MaceSettings? settings = null)
    {
        MaceResult result = new MaceResult();
        settings ??= MaceSettings.Default;
        double[][] spamming, thetas, thetaPriors, strategyPriors;
        double logMarginalLikeliHood = 0;
        double[] entropies;
        List<MaceAnnotation> annotationsList = annotations.ToList();
        MaceInput input = ParseInput(annotationsList);
        int nInstances = input.Labels.Length;
        int nLabels = input.LabelOptions.Count;
        int nAnnotators = input.Annotators.Count;
        double[][] goldLabelMarginals = Utils.CreateJaggedArray<double[][]>(nInstances, nLabels);
        double[][] strategyExpectedCounts = Utils.CreateJaggedArray<double[][]>(nAnnotators, nLabels);
        double[][] knowingExpectedCounts = Utils.CreateJaggedArray<double[][]>(nAnnotators, 2);
        double smoothing = settings.Smoothing / nLabels;
        Dictionary<int, int> controlLabels = new Dictionary<int, int>();
        double[][] bestThetas = Utils.CreateJaggedArray<double[][]>(nAnnotators, 2);
        double[][] bestStrategies = Utils.CreateJaggedArray<double[][]>(nAnnotators, nLabels);
        double bestLogMarginalLikelihood = double.NegativeInfinity;
        int bestModelAtRestart = 0;

        for (int i = 0; i < settings.Restarts; i++)
        {
            Run(i + 1);
        }

        if (settings.Test is null || settings.Test.FinalStep is MaceTestSettings.StepToEnd.RunToEnd)
        {
            logMarginalLikeliHood = bestLogMarginalLikelihood;
            spamming = bestThetas;
            thetas = bestStrategies;
        
            EStep();
            string[] predictions = Decode(settings.Threshold);

            int n = 0;
        }

        if (settings.Test is not null)
        {
            result.TestResult = new MaceTestResult
            {
                GoldLabelMarginals = goldLabelMarginals,
                LogMarginalLikelihood = logMarginalLikeliHood,
                KnowingExpectedCounts = knowingExpectedCounts,
                StrategyExpectedCounts = strategyExpectedCounts
            };
        }
        
        return result;

        void Run(int index)
        {
            InitializeRun();

            if (settings.Test?.Init is not null)
            {
                spamming = settings.Test.Init.Spamming;
                thetas = settings.Test.Init.Thetas;
                thetaPriors = settings.Test.Init.ThetaPriors;
                strategyPriors = settings.Test.Init.StrategyPriors;
            }
            
            EStep();

            if (settings.Test?.FinalStep is MaceTestSettings.StepToEnd.FirstEStep)
            {
                return;
            }

            for (int i = 0; i < settings.Iterations; ++i)
            {
                MStep();
                EStep();
            }

            if (logMarginalLikeliHood > bestLogMarginalLikelihood)
            {
                bestModelAtRestart = index;
                bestLogMarginalLikelihood = logMarginalLikeliHood;
                Array.Copy(spamming, bestThetas, spamming.Length);
                Array.Copy(thetas, bestStrategies, thetas.Length);
            }
        }
        
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
            }
            
            Math2.NormalizeInPlace(spamming, 0);
            Math2.NormalizeInPlace(thetas, 0);

            thetaPriors = Utils.CreateJaggedArray<double[][]>(nAnnotators, 2);
            strategyPriors = Utils.CreateJaggedArray<double[][]>(nAnnotators, nLabels);

            for (int i = 0; i < nAnnotators; ++i)
            {
                thetaPriors[i][0] = settings.Alpha;
                thetaPriors[i][1] = settings.Beta;
                Array.Fill(strategyPriors[i], 10.0);
            }
        }

        void EStep()
        {
            for (int i = 0; i < nInstances; ++i)
            {
                for (int j = 0; j < nLabels; ++j)
                {
                    goldLabelMarginals[i][j] = 0.0;
                }
            }

            for (int i = 0; i < nAnnotators; ++i)
            {
                knowingExpectedCounts[i][0] = 0.0;
                knowingExpectedCounts[i][1] = 0.0;

                for (int j = 0; j < nLabels; ++j)
                {
                    strategyExpectedCounts[i][j] = 0.0;
                }
            }

            logMarginalLikeliHood = 0.0;

            for (int i = 0; i < nInstances; ++i)
            {
                double instanceMarginal = 0.0;

                for (int j = 0; j < nLabels; ++j)
                {
                    double goldLabelMarginal = 1.0 / nLabels;

                    if (input.Labels.Length <= i)
                    {
                        continue;
                    }
                    
                    for (int k = 0; k < input.Labels[i].Length; ++k)
                    {
                        int a = input.WhoLabeled[i][k];
                        goldLabelMarginal *= spamming[a][0] * thetas[a][input.Labels[i][k]] + (j == input.Labels[i][k] ? spamming[a][1] : 0.0);
                    }

                    if (controlLabels.ContainsKey(i) && (!controlLabels.TryGetValue(i, out int val) || val != j))
                    {
                        continue;
                    }
                    
                    instanceMarginal += goldLabelMarginal;
                    goldLabelMarginals[i][j] = goldLabelMarginal;
                }

                if (input.Labels.Length < i)
                {
                    continue;
                }

                logMarginalLikeliHood += Math.Log(instanceMarginal);

                for (int j = 0; j < input.Labels[i].Length; ++j)
                {
                    int a = input.WhoLabeled[i][j];
                    double strategyMarginal = 0.0;
       
                    if (controlLabels.ContainsKey(i))
                    {
                        if (controlLabels.TryGetValue(i, out int val) && val == input.Labels[i][j])
                        {
                            strategyMarginal += goldLabelMarginals[i][val] / (spamming[a][0] * thetas[a][input.Labels[i][j]] + (val == input.Labels[i][j] ? spamming[a][1] : 0.0));
                            strategyMarginal *= spamming[a][0] * thetas[a][input.Labels[i][j]];
                            double strategyMarginalOverInstanceMarginal = strategyMarginal / instanceMarginal;
                            strategyExpectedCounts[a][input.Labels[i][j]] += strategyMarginalOverInstanceMarginal;
                            knowingExpectedCounts[a][0] += strategyMarginalOverInstanceMarginal;
                            knowingExpectedCounts[a][1] += goldLabelMarginals[i][input.Labels[i][j]] * spamming[a][1] / (spamming[a][0] * thetas[a][input.Labels[i][j]] + spamming[a][1]) / instanceMarginal;
                        }
                        else
                        {
                            strategyExpectedCounts[a][input.Labels[i][j]]++;
                            knowingExpectedCounts[a][0]++;
                        }
                    }
                    else
                    {
                        for (int k = 0; k < nLabels; ++k)
                        {
                            strategyMarginal += goldLabelMarginals[i][k] / (spamming[a][0] * thetas[a][input.Labels[i][j]] + (k == input.Labels[i][j] ? spamming[a][1] : 0.0));
                        }

                        strategyMarginal *= spamming[a][0] * thetas[a][input.Labels[i][j]];
                        double strategyMarginalOverInstanceMarginal = strategyMarginal / instanceMarginal;
                        strategyExpectedCounts[a][input.Labels[i][j]] += strategyMarginalOverInstanceMarginal;
                        knowingExpectedCounts[a][0] += strategyMarginalOverInstanceMarginal;
                        knowingExpectedCounts[a][1] += goldLabelMarginals[i][input.Labels[i][j]] * spamming[a][1] / (spamming[a][0] * thetas[a][input.Labels[i][j]] + spamming[a][1]) / instanceMarginal;
                    }
                }
            }
        }

        void MStep()
        {
            spamming = Math2.VariationalNormalize(knowingExpectedCounts, thetaPriors);
            thetas = Math2.VariationalNormalize(strategyExpectedCounts, strategyPriors);
        }

        string[] Decode(double threshold)
        {
            entropies = GetLabelEntropies();
            double[] slice = Utils.CreateJaggedArray<double[]>(entropies.Length);
            Array.Copy(entropies, 0, slice, 0, nInstances);
            double entropyThreshold = GetEntropyForThreshold(threshold, slice);

            string[] r = new string[nInstances];

            for (int i = 0; i < nInstances; ++i)
            {
                double bestProb = double.NegativeInfinity;
                int bestLabel = -1;

                if (entropies[i] <= entropyThreshold)
                {
                    if (entropies[i] is double.NegativeInfinity)
                    {
                        r[i] = string.Empty;
                    }
                    else
                    {
                        for (int j = 0; j < nLabels; ++j)
                        {
                            if (goldLabelMarginals[i][j] > bestProb)
                            {
                                bestProb = goldLabelMarginals[i][j];
                                bestLabel = j;
                            }

                            r[i] = bestLabel.ToString();
                        }
                    }
                }
                else
                {
                    r[i] = string.Empty;
                }
            }

            return r;
        }

        double[] GetLabelEntropies()
        {
            double[] r = new double[nInstances];

            for (int i = 0; i < nInstances; ++i)
            {
                if (input.Labels.Length < i)
                {
                    r[i] = double.NegativeInfinity;
                    continue;
                }

                double norm = 0.0;
                double entropy = 0.0;

                for (int j = 0; j < nLabels; ++j)
                {
                    norm += goldLabelMarginals[i][j];
                }

                for (int j = 0; j < nLabels; ++j)
                {
                    double p = goldLabelMarginals[i][j] / norm;

                    if (p > 0)
                    {
                        entropy += -p * Math.Log(p);
                    }
                }

                r[i] = entropy;
            }

            return r;
        }

        double GetEntropyForThreshold(double threshold, double[] entropyArray)
        {
            int pivot = threshold switch
            {
                0 => 0,
                1 => nInstances - 1,
                _ => (int)(nInstances * threshold)
            };

            Array.Sort(entropyArray);
            return entropyArray[pivot];
        }
    }
}