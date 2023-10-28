namespace macenet;

public static class Utils
{
    public static T CreateJaggedArray<T>(params int[] lengths)
    {
        return (T)InitializeJaggedArray(typeof(T).GetElementType(), 0, lengths)!;
    }

    private static object? InitializeJaggedArray(Type? type, int index, int[] lengths)
    {
        if (type != null)
        {
            Array array = Array.CreateInstance(type, lengths[index]);
            Type? elementType = type.GetElementType();

            if (elementType != null)
            {
                for (int i = 0; i < lengths[index]; i++)
                {
                    array.SetValue(InitializeJaggedArray(elementType, index + 1, lengths), i);
                }
            }

            return array;
        }

        return null;
    }
}