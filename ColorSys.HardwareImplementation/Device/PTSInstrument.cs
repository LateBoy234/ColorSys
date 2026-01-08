using ColorSys.HardwareContract;
using ColorSys.HardwareContract.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Runtime.InteropServices.JavaScript;
using System.Text;
using System.Threading.Tasks;

namespace ColorSys.HardwareImplementation.Device
{
    public sealed class PTSInstrument:IDevice 
    {

        private readonly Subject<TestModel> _subject = new();
        public IObservable<TestModel> TestStream => _subject;

        private readonly CancellationTokenSource _cts = new();
        public PTSInstrument(string id, string model, ICommunication comm)
        {
            Comm = comm;
            InstrumentId = id;
            Model=model;
        }

        public string InstrumentId { get; }

        public string Model { get; }

        public ICommunication Comm { get; }

        public void Dispose()
        {
            _cts.Cancel();
            _subject.OnCompleted();
            _subject.Dispose();
            Comm.Dispose();
        }

      
       public async  Task  RunTestAsync(CancellationToken token)
        {
            await Comm.ConnectAsync(token);
            _ = Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    await Task.Delay(100, token);
                    var raw = await Comm.SendAsync(new byte[] { 0x01 }, token);
                    var data = new TestModel(new TestModel()
                    {
                        ID = 1,
                        Name = "Samp1",
                        DateTime = DateTime.Now,
                        Material = "1",
                        OpticalStruct = "SCI",
                        InstrumentSN = "PTS001",
                        DataValues = new[] { BitConverter.ToDouble(raw, 0) }
                    });
                    _subject.OnNext(data);
                }
            }, token);
            
        }
    }
}
