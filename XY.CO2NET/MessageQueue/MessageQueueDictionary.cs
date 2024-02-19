using System;
using System.Collections.Concurrent;

namespace XY.CO2NET.MessageQueue
{
    public class MessageQueueDictionary : ConcurrentDictionary<string, XYMessageQueueItem>
    {
        public MessageQueueDictionary() : base(StringComparer.OrdinalIgnoreCase)
        {
        }
    }
}
