using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace PartyHelper
{
    public class LogHelper
    {
        private readonly ILogger _logger;
        private Guid Guid;
        private DateTime LastTime;

        public LogHelper(ILogger logger)
        {
            _logger = logger;
            Guid = Guid.NewGuid();
            LastTime = DateTime.Now;
            _logger.LogInformation($"Initialize controller, guid: { Guid}, time: { LastTime.ToLongTimeString()}");
        }

        public void LogInformation(string message)
        {
            DateTime now = DateTime.Now;
            _logger.LogInformation($"{Guid}: {message}, time taken: {(now - LastTime).TotalMilliseconds}ms.");
            LastTime = now;
        }
        public void LogError(string message)
        {
            DateTime now = DateTime.Now;
            _logger.LogError($"[ERROR]{Guid}: {message}, time taken: {(now - LastTime).TotalMilliseconds}ms.");
            LastTime = now;
        }
    }
}
