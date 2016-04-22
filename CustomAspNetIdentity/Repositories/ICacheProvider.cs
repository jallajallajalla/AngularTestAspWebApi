using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;

namespace MyDoc.Repositories.Providers
{
    public interface ICacheProvider
    {
        T GetOrAdd<T>(string key, Func<T> resolver, int expiresMinutes = 30);
        void Remove(string key);
    }

    class CacheProvider : ICacheProvider
    {
        private void Add<T>(string key, T value, int expiresMinutes)
        {
            MemoryCache.Default.Add(key, value, DateTime.Now.AddMinutes(expiresMinutes));
        }

        public T GetOrAdd<T>(string key, Func<T> resolver, int expiresMinutes = 30)
        {
            var item = MemoryCache.Default.Get(key);
            if (item is T)
                return (T)item;

            var value = resolver();

            Add(key, value, expiresMinutes);

            return value;
        }

        public void Remove(string key)
        {
            MemoryCache.Default.Remove(key);
        }
    }

}
