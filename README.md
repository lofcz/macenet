# MACE .NET (Multi-Annotator Competence Estimation)
Unofficial port of the [MACE Reference Implementation](https://github.com/dirkhovy/MACE) for the .NET platform. Unlike the reference implementation, MACE .NET is targeted as a library. Not every option from the reference implementation is supported, the input format was changed for better ergonomy, and a few edits to the math used were done.

Please see:

Dirk Hovy, Taylor Berg-Kirkpatrick, Ashish Vaswani, and Eduard Hovy (2013): **Learning Whom to Trust With MACE.**  
[PDF](https://aclanthology.org/N13-1132/)

A high-level overview of MACE is available [here](https://toloka.ai/docs/crowd-kit/reference/crowdkit.aggregation.classification.mace.MACE/).

## Getting started

Create a dataset for labeling, consisting of N items, where each item will be labeled from the same pool of labels by at least one annotator. Each annotator can assign only one label from the pool (no multi-labeling).
For example, here is a dataset of three pictures. Each of them we need to classify as a dog or cat. We will assign each possible label a unique `int32` value. Let `0 = dog`, `1 = cat`.  

![fatcat](https://github.com/lofcz/macenet/assets/10260230/e55b43ed-57fb-46a1-8b46-938fdd0dbdcd)
![bingus](https://github.com/lofcz/macenet/assets/10260230/1b343f36-0d0e-4ef3-afe7-1a659d630d69)
![boi](https://github.com/lofcz/macenet/assets/10260230/8261ba69-5c1a-4053-9771-c54ea437a343)

_Note that we could classify anything else, another example would be whether a sentence is positive or not._

We have three annotators available but not all of them for labelling of all the items. This represents a crowdsourced solution where annotators might be available only for a brief time.
Each annotator needs a unique identifier, represented as `int32`. Let's say we have the following labels:

| Data | Annotator 1 | Annotator 2 | Annotator 3 |
|:------|:---:|:---:|:---:|
| Item 1    | 0 | 1 |   |
| Item 2    | 0 | 1 | 1 |
| Item 3    |   |   | 0 |

_Annotator 3 skipped the first image, annotated the second image as a cat and the third image as a dog._

In code, we can do this as:

```cs
using macenet;

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
```

Observe the first item, we have one vote for it being a cat and one for it being a dog. In majority voting, we would have to flip a coin.
Instead, we can use MACE and the power of Bayesian statistics, to get much better results:

```cs
MaceResult result = Mace.Evaluate(annotations);
```

`MaceResult` is a static, thread-safe routine, that gives us three sets of output information:

1. `result.Labels` - percentual probability of each possible label for each item of the dataset. For each item, the choices sum to 100%.
2. `result.Annotators` - percentual reliability of each annotator. This is in the range of 0..100% for each annotator.
3. `result.Entropies` - difficulty of each item. In case of unanimous consensus, this will be very low.

Complete code to run:

```cs
using macenet;

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

MaceResult result = Mace.Evaluate(annotations);
```

We can print the results:

```cs

foreach (MaceResultItemLabel prediction in result.Labels)
{
    Console.WriteLine($"Predictions for item {prediction.Item}");

    foreach (MaceLabel label in prediction.Labels)
    {
        Console.WriteLine($"-- {label}");
    }
}

/*Predictions for item 0
-- Option 1, trust 90.16728%
-- Option 0, trust 9.83272%
Predictions for item 1
-- Option 1, trust 99.08392%
-- Option 0, trust 0.91608%
Predictions for item 2
-- Option 0, trust 92.14050%
-- Option 1, trust 7.85950%*/
```

Note that the options are sorted by default in descending order of trust. So here, instead of flipping a coin, we are pretty sure the correct label for the first item is `1 (cat)`.

## Options

The `Mace.Evaluate` routine can take a second argument with various settings. Each option is set to a sane default, so we can modify just the properties we want to touch:

- `Alpha` and `Beta` - two hyperparameters representing how much the annotators are guessing.
- `Noise` - represents default bias toward randomly considering an annotator more or less reliable.
- `Restarts` - the amount of times we run the algorithm. As this is an EM-based technique, when oscillating efficiency and maximization steps we can diverge more and more.
- `Iterations` - the amount of times EM steps will be run, both steps are considered as one iteration.

## Control Labels

We can help the algorithm by mixing a few items with known (ground truth) labels. These are sometimes called "control". This can be done by providing a third parameter to the `Mace.Evaluate` routine:

```cs
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
    new MaceControlLabel(0, 1) // in the example above, this means the first item is a cat
};

MaceResult result = Mace.Evaluate(annotations, null, controls);
```

## Acknowledgments

[Dirk Hovy](https://github.com/dirkhovy) et. al for creating the reference implementation.  
[Aneta Kahleová](https://github.com/anetakahle) for help with understanding the original manuscript.
