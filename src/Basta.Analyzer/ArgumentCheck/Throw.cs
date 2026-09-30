namespace Basta.Analyzer
{
    internal static class Throw
    {
        internal static T IfNull<T>(T value,
                                    string name)
        {
            if (value is null)
            {
                throw new ArgumentNullException(name);
            }

            return value;
        }
    }
}
