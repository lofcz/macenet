namespace macenet.tests;

public static class Utils
{
    private const double Tolerance = 0.1e-8;

    public static bool EqualsWithinTolerance(double a, double b)
    {
        return a switch
        {
            double.NaN => double.IsNaN(b),
            double.NegativeInfinity => double.IsNegativeInfinity(b),
            double.PositiveInfinity => double.IsPositiveInfinity(b),
            _ => Math.Abs(a - b) < Tolerance
        };
    }
}