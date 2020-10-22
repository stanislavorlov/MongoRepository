using System;

namespace Common
{
    public static class Ensure
    {
        public static void NotNull(object argument, string argumentName = null)
        {
            _ = argument ?? throw new ArgumentNullException(argumentName ?? string.Empty);
        }
    }
}