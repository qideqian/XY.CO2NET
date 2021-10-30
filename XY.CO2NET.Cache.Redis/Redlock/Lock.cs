using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Text;

namespace Redlock.CSharp
{
    /// <summary>
    /// Redis 锁
    /// </summary>
    public class Lock
    {
        public Lock(RedisKey resource, RedisValue val, TimeSpan validity)
        {
            this.resource = resource;
            this.val = val;
            this.validity_time = validity;
        }

        private RedisKey resource;

        private RedisValue val;

        private TimeSpan validity_time;

        public RedisKey Resource { get { return resource; } }

        public RedisValue Value { get { return val; } }

        public TimeSpan Validity { get { return validity_time; } }
    }
}
