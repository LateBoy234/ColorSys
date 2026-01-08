// See https://aka.ms/new-console-template for more information
using NModbus;
using NModbus.Serial;
using System.IO.Ports;

class Program
{
    static void Main()
    {
        // 1. 创建串口对象
        using var sp = new SerialPort("COM4", 9600, Parity.None, 8, StopBits.One)
        {
            WriteTimeout = 500,
            ReadTimeout = 500
        };

        // 2. 打开串口
        sp.Open();
        Console.WriteLine($"COM11 open = {sp.IsOpen}");

        // 3. 裸发一条最短的 01 03 00 00 00 01 84 0A
        byte[] frame = { 0x01, 0x03, 0x00, 0x00, 0x00, 0x01, 0x84, 0x0A };
        sp.Write(frame, 0, frame.Length);
        Console.WriteLine("8 字节已发出");

        //sp.Write(frame, 0, frame.Length);
        //Console.WriteLine("8 字节已发出");

        Thread.Sleep(200);          // 给虚拟线一点转发时间
        int n = sp.BytesToRead;
        Console.WriteLine($"COM11 自己收到 {n} 字节");
        // 4. 停 300 ms 让助手显示
        Thread.Sleep(300);

        // 5. 关闭
        sp.Close();
        Console.WriteLine("Done.");
    }
}
