using ColorSys.HardwareContract.SystemConfig;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorSys.HardwareImplementation.SystemConfig
{
    public sealed class ConfigManager : INotifyPropertyChanged
    {
        private static readonly Lazy<ConfigManager> _instance =
        new(() => new ConfigManager());

        public static ConfigManager Instance
        {
            get
            {
                return _instance.Value;
            }
        }
        private readonly IConfigRepository _repository;
        private readonly ConcurrentDictionary<string, string> _cache ;
        public ConfigManager()
        {
            _repository = new JsonConfigRepository();
            _cache=new ConcurrentDictionary<string, string>(_repository .LoadAsync().Result);
        }
       
        public T? Get<T>(string key, T? defaultValue = default)
        {
            return _cache.TryGetValue(key, out var val)
            ? (T)Convert.ChangeType(val, typeof(T))
            : defaultValue;
        }

        public void Set<T>(string key, T value)
        {
            _cache[key] = value?.ToString()??string.Empty;
            _repository.SaveAsync(_cache).ConfigureAwait(false).GetAwaiter().GetResult();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(key));
        }

        public TEnum GetEnum<TEnum>(string key, TEnum defaultValue = default)
    where TEnum : struct, Enum
        {
            var str = Get(key, defaultValue.ToString());
            return Enum.TryParse<TEnum>(str, out var val) ? val : defaultValue;
        }

        public void SetEnum<TEnum>(string key, TEnum value)
            where TEnum : struct, Enum
        {
            Set(key, value.ToString());   // 存字符串
        }

        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
