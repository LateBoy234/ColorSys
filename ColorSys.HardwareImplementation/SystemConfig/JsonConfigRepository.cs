using ColorSys.HardwareContract.SystemConfig;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ColorSys.HardwareImplementation.SystemConfig
{
    internal class JsonConfigRepository : IConfigRepository
    {
        private readonly string _path;
        public JsonConfigRepository(string fileName = "SystemConfig.json")
        {
            var path = Environment.CurrentDirectory;

            _path = Path.Combine(path,"ColorSys",fileName);
            Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
        }
        public async Task<Dictionary<string, string>> LoadAsync()
        {
            if(File.Exists(_path))
            {
                var json =await File.ReadAllTextAsync(_path);
                return JsonSerializer.Deserialize<Dictionary<string, string>>(json)??new();
            }
            return new();
        }

        public Task SaveAsync(IReadOnlyDictionary<string, string> dict)
        {
            var json=JsonSerializer.Serialize(dict);
            return File.WriteAllTextAsync(_path,json);
        }
    }
}
