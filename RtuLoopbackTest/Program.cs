// See https://aka.ms/new-console-template for more information
using NModbus;
using NModbus.Serial;
using System.IO.Ports;

class Program
{
    static void Main()
    {
        using var port = new SerialPort("COM11", 9600, Parity.None, 8, StopBits.One);
        port.Open();

        var factory = new ModbusFactory();
        IModbusMaster master = factory.CreateRtuMaster(port);
        ushort n = 0;

        while (true)
        {
            try
            {
                ushort[] data = master.ReadHoldingRegisters(1, 0, 4); //从站地址1，起始地址0，读4个寄存器
                Console.WriteLine($"40001-40004 = {string.Join(", ", data)}");
                n++;
                master.WriteSingleRegister(1, 0, n);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            System.Threading.Thread.Sleep(1000);
        }
    }
}
