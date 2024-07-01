using PcapDotNet.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;

namespace MCS_Packet_Server
{
    class Program
    {
        public static IList<LivePacketDevice> packets;
        public static PacketCommunicator connectdevice;

        private const string InfluxUrl = "http://localhost:8086"; // InfluxDB v2 URL
        private const string Token = "FLUqoKe6aGbT0A9rzz0gULEUhs3lxv37nhRVgxfScI_DX3-PaZ_eyS6avngMm1wBHRBItifABO15mWX1-P4ADA=="; // InfluxDB v2 access token
        private const string Bucket = "MCS"; // InfluxDB v2 bucket name
        private const string Org = "MCS"; // InfluxDB v2 organization name

        static void Main(string[] args)
        {
            Connect();
            ReadPacket();
        }

        public static void Connect()
        {


            var client = InfluxDBClientFactory.Create(InfluxUrl, Token.ToCharArray());

            try
            {             
                // 데이터 삽입을 위한 Point 데이터 구성
                var point = PointData.Measurement("satellite_traffic")
                    .Tag("satellite_name", "Sat2")
                    .Tag("source_ip", "192.168.1.2")
                    .Tag("destination_ip", "192.168.1.100")
                    .Field("protocol", "TCP")
                    .Field("length", 1500)
                    .Field("packet_info", "Payload data")
                    .Timestamp(DateTime.UtcNow, WritePrecision.Ns); // 현재 시간을 사용하거나 필요에 따라 다른 타임스탬프 사용 가능

                // 데이터 삽입 요청
                var writeApi = client.GetWriteApi();
                writeApi.WritePoint(Bucket, Org, point);
                writeApi.Flush();

                Console.WriteLine("데이터가 성공적으로 InfluxDB에 삽입되었습니다.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"데이터 삽입 중 오류 발생: {ex.Message}");
            }
            finally
            {
                // 클라이언트 종료
                client.Dispose();
            }
        }
        public static void ReadPacket()
        {
            


            packets = LivePacketDevice.AllLocalMachine;
            if (packets.Count == 0)
            {
                Console.WriteLine("디바이스를 찾지 못했습니다.");
                return;
            }
            LivePacketDevice selectedDev = packets[1];
            connectdevice = selectedDev.Open(65536, PacketDeviceOpenAttributes.Promiscuous, 1000);
            connectdevice.ReceivePackets(0, packet =>
            {
                // 수신된 패킷의 타임스탬프와 길이를 출력
                Console.WriteLine(packet.Timestamp.ToString("yyyy-MM-dd hh:mm:ss.fff") + " length:" + packet.Length);
                // 패킷의 내용을 출력 (문자열로 변환된 형식)
                Console.WriteLine(packet[0].ToString());
                Console.WriteLine(packet[1].ToString());
                Console.WriteLine(packet[2].ToString());
                Console.WriteLine(packet[3].ToString());
                Console.WriteLine(packet[4].ToString());
                Thread.Sleep(5000);
              
            });
        }
    }
}
