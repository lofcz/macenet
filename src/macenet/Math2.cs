namespace macenet;

public static class Math2
{
    public const double Ln2 = 0.6931471805599453d; // Math.Log(2)
    public const double Pi = 3.141592653589793d; // Math.PI
    public const double Sqrt2Pi = 2.5066282746310002d; // Math.Sqrt(2.0 * Pi)
    public const double NegativeDigamma1 = 0.5772156649015328606065120900824024d; // digamma(-1)

    // g=7, n=9
    // high precision: https://www.mrob.com/pub/ries/lanczos-gamma.html (ctrl+f 676.520368121885098567009190444019)
    public static readonly double[] LanczosCoeffs =
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
    
    public static readonly double[] DigammaCoeffs = 
    { 
        .30459198558715155634315638246624251d,
        .72037977439182833573548891941219706d, 
        -.12454959243861367729528855995001087d,
        .27769457331927827002810119567456810e-1d, 
        -.67762371439822456447373550186163070e-2d,
        .17238755142247705209823876688592170e-2d, 
        -.44817699064252933515310345718960928e-3d,
        .11793660000155572716272710617753373e-3d, 
        -.31253894280980134452125172274246963e-4d,
        .83173997012173283398932708991137488e-5d, 
        -.22191427643780045431149221890172210e-5d,
        .59302266729329346291029599913617915e-6d, 
        -.15863051191470655433559920279603632e-6d,
        .42459203983193603241777510648681429e-7d, 
        -.11369129616951114238848106591780146e-7d,
        .304502217295931698401459168423403510e-8d, 
        -.81568455080753152802915013641723686e-9d,
        .21852324749975455125936715817306383e-9d, 
        -.58546491441689515680751900276454407e-10d,
        .15686348450871204869813586459513648e-10d, 
        -.42029496273143231373796179302482033e-11d,
        .11261435719264907097227520956710754e-11d, 
        -.30174353636860279765375177200637590e-12d,
        .80850955256389526647406571868193768e-13d, 
        -.21663779809421233144009565199997351e-13d,
        .58047634271339391495076374966835526e-14d, 
        -.15553767189204733561108869588173845e-14d,
        .41676108598040807753707828039353330e-15d, 
        -.11167065064221317094734023242188463e-15d 
    };
    
    public static readonly long[] FibSequence =
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
    
    public static double Digamma(double n)
    {
        if (n is 0)
        {
            return double.NaN;
        }

        double acc = 0;

        if (n < 0)
        {
            acc += Pi / Math.Tan(Pi * (1 - n));
            n = 1.0 - n;
        }

        if (n < 1)
        {
            while (n < 1)
            {
                acc -= 1 / n++;
            }
        }

        switch (n)
        {
            case 1:
                return acc - NegativeDigamma1;
            case 2:
                return acc + 1 - NegativeDigamma1;
            case 3:
                return acc + 1.5 - NegativeDigamma1;
            case > 3:
            {
                while (n > 3)
                {
                    acc += 1 / --n;
                }

                return acc + Digamma(n);
            }
        }

        n -= 2.0;
        double tNMinus1 = 1.0;
        double tN = n;
        double digamma = DigammaCoeffs[0] + DigammaCoeffs[1] * tN;

        for (int i = 2; i < DigammaCoeffs.Length; i++)
        {
            double tN1 = 2.0 * n * tN - tNMinus1;
            digamma += DigammaCoeffs[i] * tN1;
            tNMinus1 = tN;
            tN = tN1;
        }

        return acc + digamma;
    }
}