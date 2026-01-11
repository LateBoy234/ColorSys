using ColorSys.Domain.DbHandle;
using ColorSys.HardwareContract.SystemConfig;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorSys.HardwareImplementation.SystemConfig
{
    internal class DbConfigRepository: IConfigRepository
    {
        private readonly ConfigDbContext _ctx;
        public DbConfigRepository(ConfigDbContext ctx) => _ctx = ctx;

        public async Task<Dictionary<string, string>> LoadAsync() =>
            await _ctx.Configs.ToDictionaryAsync(x => x.Key, x => x.Value);

        public async Task SaveAsync(IReadOnlyDictionary<string, string> dict)
        {
            _ctx.Configs.RemoveRange(_ctx.Configs);          // 全量覆盖
            foreach (var kv in dict)
                _ctx.Configs.Add(new Domain.Config.SystemConfig { Key = kv.Key, Value = kv.Value });
            await _ctx.SaveChangesAsync();
        }
    }
}
