using System;

namespace Common
{
    public interface ILogger
    {
        public void LogError(Exception exception);

        public void LogError(Exception exception, string message);
    }
}
