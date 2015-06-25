using Simple.Aspects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Tests
{
    public class MemoryCacheAspectAttribute : CacheAspectAttribute
    {
        private Dictionary<string, object> cache = new Dictionary<string, object>();

        protected override void StoreObject(string key, object value)
        {

            if (cache.ContainsKey(key))
                cache.Remove(key);

            cache.Add(key, value);
        }

        protected override object GetObject(string key)
        {
            object ret;
            cache.TryGetValue(key, out ret);
            return ret;
        }
    }
}
