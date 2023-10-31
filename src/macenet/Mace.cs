namespace macenet;

public static class Mace
{
    public static MaceInput ParseInput(List<MaceAnnotation> annotations)
    {
        int items = annotations.Max(x => x.Item) + 1;
        int nOptions = 0;
        int nAnnotators = 0;
        List<List<int>> whoLabeled = new();
        List<List<int>> labels = new();
        Dictionary<int, int> options = new();
        Dictionary<int, int> annotators = new();
        Dictionary<int, int> optionsParse = new();
        Dictionary<int, int> annotatorsParse = new();

        for (int i = 0; i < items; i++)
        {
            whoLabeled.Add(new List<int>());
            labels.Add(new List<int>());
        }
            
        foreach (MaceAnnotation annotation in annotations)
        {
            if (!options.ContainsValue(annotation.Choice))
            {
                options.Add(nOptions, annotation.Choice);
                optionsParse.Add(annotation.Choice, nOptions);
                nOptions++;
            }
            
            if (!annotators.ContainsValue(annotation.Annotator))
            {
                annotators.Add(nAnnotators, annotation.Annotator);
                annotatorsParse.Add(annotation.Annotator, nAnnotators);
                nAnnotators++;
            }

            if (annotatorsParse.TryGetValue(annotation.Annotator, out int aIndex))
            {
                whoLabeled[annotation.Item].Add(aIndex);    
            }

            if (optionsParse.TryGetValue(annotation.Choice, out int cIndex))
            {
                labels[annotation.Item].Add(cIndex);   
            }
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
    
    public static MaceResult Evaluate(IEnumerable<MaceAnnotation> annotations, MaceSettings? settings = null, IEnumerable<MaceControlLabel>? controlLabels = null)
    {
        MaceResult result = new();
        settings ??= MaceSettings.Default;
        double[][] spamming, thetas, thetaPriors, strategyPriors;
        double logMarginalLikeliHood = 0;
        double[] entropies;
        List<MaceAnnotation> annotationsList = annotations.ToList();
        MaceInput input = ParseInput(annotationsList);
        int nInstances = input.Labels.Length;
        int nLabels = input.LabelOptions.Count;
        int nAnnotators = input.Annotators.Count;
        double[][] goldLabelMarginals = Utils.CreateJaggedArray2D(nInstances, nLabels);
        double[][] strategyExpectedCounts = Utils.CreateJaggedArray2D(nAnnotators, nLabels);
        double[][] knowingExpectedCounts = Utils.CreateJaggedArray2D(nAnnotators, 2);
        Dictionary<int, int> controlLabelsDict = new();
        double[][] bestThetas = Utils.CreateJaggedArray2D(nAnnotators, 2);
        double[][] bestStrategies = Utils.CreateJaggedArray2D(nAnnotators, nLabels);
        double bestLogMarginalLikelihood = double.NegativeInfinity;
        int bestModelAtRestart = 0;

        if (controlLabels is not null)
        {
            foreach (MaceControlLabel label in controlLabels)
            {
                controlLabelsDict.TryAdd(label.Item, label.Choice);
            }
        }

        for (int i = 0; i < settings.Restarts; i++)
        {
            Run(i + 1);
        }

        if (settings.Test is null || settings.Test.FinalStep is MaceTestSettings.StepToEnd.RunToEnd)
        {
            spamming = bestThetas;
            thetas = bestStrategies;
        
            EStep();
            MaceResultItemLabel[]? predictions = Decode(settings.Threshold);

            result.BestLogMarginalLikelihood = bestLogMarginalLikelihood;
            result.BestModelAtRestart = bestModelAtRestart;
            result.Labels = predictions;
            result.Annotators = new MaceResultAnnotatorReliability[nAnnotators];

            for (int i = 0; i < nAnnotators; i++)
            {
                if (input.Annotators.TryGetValue(i, out int aI))
                {
                    result.Annotators[i] = new MaceResultAnnotatorReliability(aI, spamming[i][1] * 100);   
                }
            }

            result.Entropies = new MaceResultItemEntropy[nInstances];
            entropies = GetLabelEntropies();
            
            for (int i = 0; i < nInstances; i++)
            {
                result.Entropies[i] = new MaceResultItemEntropy(i, entropies[i]);
            }
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

            if (!(logMarginalLikeliHood > bestLogMarginalLikelihood))
            {
                return;
            }
            
            bestModelAtRestart = index;
            bestLogMarginalLikelihood = logMarginalLikeliHood;
            Array.Copy(spamming, bestThetas, spamming.Length);
            Array.Copy(thetas, bestStrategies, thetas.Length);
        }
        
        void InitializeRun()
        {
            spamming = Utils.CreateJaggedArray2D(nAnnotators, 2);
            thetas = Utils.CreateJaggedArray2D(nAnnotators, nLabels);

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

            thetaPriors = Utils.CreateJaggedArray2D(nAnnotators, 2);
            strategyPriors = Utils.CreateJaggedArray2D(nAnnotators, nLabels);

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

                    if (controlLabelsDict.ContainsKey(i) && (!controlLabelsDict.TryGetValue(i, out int val) || val != j))
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
       
                    if (controlLabelsDict.ContainsKey(i))
                    {
                        if (controlLabelsDict.TryGetValue(i, out int val) && val == input.Labels[i][j])
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

        MaceResultItemLabel[] Decode(double threshold)
        {
            entropies = GetLabelEntropies();
            double[] slice = new double[entropies.Length];
            Array.Copy(entropies, 0, slice, 0, nInstances);
            MaceResultItemLabel[] items = new MaceResultItemLabel[nInstances];
            
            for (int i = 0; i < nInstances; ++i)
            {
                double bestProb = double.NegativeInfinity;
                int bestLabel = -1;

                items[i] = new MaceResultItemLabel { Item = i };
                
                if (entropies[i] is not double.NegativeInfinity)
                {
                    double distribSum = 0;

                    for (int j = 0; j < nLabels; ++j)
                    {
                        distribSum += goldLabelMarginals[i][j];
                    }

                    for (int j = 0; j < nLabels; ++j)
                    {
                        items[i].Labels.Add(new MaceLabel(j, goldLabelMarginals[i][j] / distribSum * 100));

                        if (!(goldLabelMarginals[i][j] > bestProb))
                        {
                            continue;
                        }
                        
                        bestProb = goldLabelMarginals[i][j];
                        bestLabel = j;
                    }

                    items[i].GoldLabel = new MaceLabel(bestLabel, bestProb / distribSum * 100);
                    items[i].Labels = items[i].Labels.OrderByDescending(x => x.Trust).ToList();
                }
            }

            return items;
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