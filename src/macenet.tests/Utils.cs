namespace macenet.tests;

public static class Utils
{
    private const double Tolerance = 0.1e-8;
    public static bool EqualsWithinTolerance(double a, double b) => Math.Abs(a - b) < Tolerance;
}