namespace macenet;

public static class Math2
{
    public const double GoldenRatio = 1.618033988749895d; // (1 + Math.Sqrt(5)) / 2.0
    public const double Ln2 = 0.6931471805599453d; // Math.Log(2)
    public const double Pi = 3.141592653589793d; // Math.PI
    public const double Sqrt2Pi = 2.5066282746310002d; // Math.Sqrt(2.0 * Pi)

    // g=7, n=9
    // high precision: https://www.mrob.com/pub/ries/lanczos-gamma.html (ctrl+f 676.520368121885098567009190444019)
    public static readonly double[] LanczosCoeffs = new double[]
    {
        0.99999999999980993d,
        676.5203681218851d,
        -1259.1392167224028d,
        771.32342877765313d,
        -176.61502916214059d,
        12.507343278686905d,
        -0.13857109526572012d,
        9.9843695780195716e-6d,
        1.5056327351493116e-7d
    };
    
    public static readonly long[] FibSequence = new long[]
    {
        1L,
        2L,
        3L,
        5L,
        8L,
        13L,
        21L,
        34L,
        55L,
        89L,
        144L,
        233L,
        377L,
        610L,
        987L,
        1597L,
        2584L,
        4181L,
        6765L,
        10946L,
        17711L,
        28657L,
        46368L,
        75025L,
        121393L,
        196418L,
        317811L,
        514229L,
        832040L,
        1346269L,
        2178309L,
        3524578L,
        5702887L,
        9227465L,
        14930352L,
        24157817L,
        39088169L,
        63245986L,
        102334155L,
        165580141L,
        267914296L,
        433494437L,
        701408733L,
        1134903170L,
        1836311903L,
        2971215073L,
        4807526976L,
        7778742049L,
        12586269025L,
        20365011074L,
        32951280099L,
        53316291173L,
        86267571272L,
        139583862445L,
        225851433717L,
        365435296162L,
        591286729879L,
        956722026041L,
        1548008755920L,
        2504730781961L,
        4052739537881L,
        6557470319842L,
        10610209857723L,
        17167680177565L,
        27777890035288L,
        44945570212853L,
        72723460248141L,
        117669030460994L,
        190392490709135L,
        308061521170129L,
        498454011879264L,
        806515533049393L,
        1304969544928657L,
        2111485077978050L,
        3416454622906707L,
        5527939700884757L,
        8944394323791464L,
        14472334024676221L,
        23416728348467685L,
        37889062373143906L,
        61305790721611591L,
        99194853094755497L,
        160500643816367088L,
        259695496911122585L,
        420196140727489673L,
        679891637638612258L,
        1100087778366101931L,
        1779979416004714189L,
        2880067194370816120L,
        4660046610375530309L,
        7540113804746346429L
    };

    public static bool IsPrime(int n)
    {
        switch (n)
        {
            case <= 1:
                return false;
            case <= 3:
                return true;
        }

        if (n % 2 is 0 || n % 3 is 0)
        {
            return false;
        }

        if ((n - 1) % 6 is not 0 && (n + 1) % 6 is not 0)
        {
            return false;
        }

        for (int i = 5; i * i <= n; i += 6)
        {
            if (n % i is 0 || n % (i + 2) is 0)
            {
                return false;
            }
        }

        return true;
    }

    public static double NaturalLogToBase2Log(double n)
    {
        return n / Ln2;
    }

    public static double Log2(double n)
    {
        return NaturalLogToBase2Log(Math.Log(n));
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="n">In range of [0.5, 1.5]</param>
    /// <returns></returns>
    public static double LanczosGamma(double n)
    {
        double nMinus1 = n - 1;
        double x = LanczosCoeffs[0];

        for (int i = 1; i < LanczosCoeffs.Length - 2; ++i)
        {
            x += LanczosCoeffs[i] / (nMinus1 + i);
        }

        double t = nMinus1 + (LanczosCoeffs.Length - 2) + 0.5;
        return Sqrt2Pi * Math.Pow(t, nMinus1 + 0.5) * Math.Exp(-t) * x;
    }

    public static double Log2Gamma(double n)
    {
        if (n < 0.5)
        {
            return Log2(Pi) - Log2(double.Sin(Pi * n)) - Log2Gamma(1.0 - n);
        }

        double result = 0;

        while (n > 1.5)
        {
            result += Log2(n - 1);
            n--;
        }

        return result + Log2(LanczosGamma(n));
    }
}