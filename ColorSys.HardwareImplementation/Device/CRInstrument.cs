using ColorSys.Domain.Model;
using ColorSys.HardwareContract;
using ColorSys.HardwareContract.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ColorSys.HardwareImplementation.Device
{
    public class CRInstrument : IDevice, IMeasureMent
    {
        private readonly ICommunication _comm;
        
        public event EventHandler<TestModel> DataReceived;
        
        public CRInstrument(ICommunication communication)
        {
            _comm = communication;
        }
        public DeviceType DeviceType => DeviceType.CR;

        public bool IsConnected => Comm.IsConnected;

        public ICommunication Comm => _comm;

       

        public async Task<bool> ConnectAsync()
        {
            await _comm.ConnectAsync();
            if (!_comm.IsConnected)
            {
                return false;
            }
            return true;
        }

        public void Dispose()
        {
            Comm.Dispose(); ;
        }

        public async Task<TestModel> RunTestAsync(CancellationToken token = default)
        {
            //var response = await SendRequestAsync(0xA6);

            //if (response != null && response.IsSuccess)
            //{
            //    // 这里根据 response.Data 解析测量数据
            //    // 例如：解析 L*, a*, b* 等
            //    return ParseTestData(response.Data);
            //}
            await Task.Delay(100); // 模拟异步操作
            return new TestModel(); // 或者抛出异常/返回错误状态
        }
    }
}
