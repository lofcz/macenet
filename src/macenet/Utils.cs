namespace macenet;

internal static class Utils
{
    public static double[][] CreateJaggedArray2D(int len1, int len2)
    {
        double[][] array = new double[len1][];

        for (int i = 0; i < len1; i++)
        {
            array[i] = new double[len2];
        }

        return array;
    }
}