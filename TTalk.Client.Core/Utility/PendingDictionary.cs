using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TTalk.Client.Core.Utility
{
    //Created by https://github.com/Seb-stian
    public sealed class PendingDictionary<TKey, TValue>
    {
        private readonly ConcurrentDictionary<TKey, TaskCompletionSource<TValue>> _dictionary = new();

        public bool Add(TKey key, TValue value)
        {
            if (_dictionary.TryRemove(key, out var completionSource))
            {
                completionSource.SetResult(value);
                return true;
            }
            return false;
        }

        public async Task<TValue> WaitForValueAsync(TKey key, int timeoutMilliseconds)
        {
            var completionSource = new TaskCompletionSource<TValue>();
            _dictionary.TryAdd(key, completionSource);
            await Task.WhenAny(completionSource.Task, Task.Delay(timeoutMilliseconds));
            if (completionSource.TrySetCanceled())
            {
                _dictionary.TryRemove(key, out _);
                return default;
            }
            return await completionSource.Task;
        }
    }
}