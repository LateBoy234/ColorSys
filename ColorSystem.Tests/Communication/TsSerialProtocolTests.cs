using ColorSys.HardwareImplementation.Communication.SeriaPort.TsSerial;
using FluentAssertions;

namespace ColorSystem.Tests.Communication;

[TestClass]
public class TsSerialProtocolTests
{
    [TestMethod]
    public void Crc16Ccitt_ForCommand22_ShouldMatchSampleFrame()
    {
        // From PDF example (TS串口 获取仪器状态 0x22):
        // 55 aa a6 01 00 00 00 00 03 00 22 d0 e5
        // CRC on wire is little-endian: 0xE5D0
        ushort crc = TsCrc16Ccitt.Compute(new byte[] { 0x22 }, 0xFFFF);
        crc.Should().Be(0xE5D0);
    }

    [TestMethod]
    public void StreamParser_ShouldParseA6DataPacket_AndValidateCrc()
    {
        var bytes = new byte[]
        {
            0x55, 0xAA, 0xA6, 0x01,
            0x00, 0x00, 0x00, 0x00,
            0x03, 0x00,
            0x22,
            0xD0, 0xE5
        };

        var parser = new TsSerialStreamParser();
        parser.Append(bytes);

        parser.TryDequeue(out var pkt).Should().BeTrue();
        pkt!.Type.Should().Be(TsSerialPacketType.Data);
        pkt.SessionId.Should().Be(0x01);
        pkt.SequenceRaw.Should().Be(0u);
        pkt.LengthField.Should().Be(3);
        pkt.Data.Should().Equal(new byte[] { 0x22 });
        pkt.DataCrc.Should().Be(0xE5D0);
    }

    [TestMethod]
    public void StreamParser_ShouldParseHandshakeResponseA2()
    {
        // From PDF example:
        // aa 55 a2 01 00 00 01 01 01 00 00 31 a3
        var bytes = new byte[]
        {
            0xAA, 0x55, 0xA2, 0x01,
            0x00,
            0x00, 0x01,
            0x01, 0x01, 0x00, 0x00,
            0x31, 0xA3
        };

        var parser = new TsSerialStreamParser();
        parser.Append(bytes);

        parser.TryDequeue(out var pkt).Should().BeTrue();
        pkt!.Type.Should().Be(TsSerialPacketType.HandshakeResponse);
        pkt.SessionId.Should().Be(0x01);
        pkt.EndianMode.Should().Be(0x00);
        pkt.DeviceRxBufferSize.Should().Be(256);
        pkt.ProtocolVersion.Should().Be(1);
        pkt.ProductType.Should().Be(1);
        pkt.DataCrc.Should().Be(0xA331);
    }

    [TestMethod]
    public void PacketBuilder_ShouldBuildCommand22FrameMatchingSample()
    {
        var built = TsSerialPacketBuilder.BuildData(
            sessionId: 0x01,
            sequenceRaw: TsSerialPacketBuilder.MakeSequence(0, TsSerialSegFlag.Last),
            data: new byte[] { 0x22 },
            hostToDevice: true);

        built.Should().Equal(new byte[]
        {
            0x55, 0xAA, 0xA6, 0x01,
            0x00, 0x00, 0x00, 0x00,
            0x03, 0x00,
            0x22,
            0xD0, 0xE5
        });
    }
}

