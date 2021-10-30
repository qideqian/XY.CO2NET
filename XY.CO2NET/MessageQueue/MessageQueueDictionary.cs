using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XY.CO2NET.MessageQueue
{
    public class MessageQueueDictionary : ConcurrentDictionary<string, XYMessageQueueItem>
    {
        public MessageQueueDictionary() : base(StringComparer.OrdinalIgnoreCase)
        {
        }
    }
}
