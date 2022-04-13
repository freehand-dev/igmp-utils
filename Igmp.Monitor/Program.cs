using FreeHand.Net.Packets;
using Serilog;
using Serilog.Events;
using System;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Igmp.Momitor
{
    class Program
    {


        static string? listenInterface;
        private const int PacketSize = 1380;

        static async Task Main(string[] args)
        {

            Log.Logger = new LoggerConfiguration()
                  .MinimumLevel.Debug()
                  .WriteTo.File("log.txt")
                  .WriteTo.Console(restrictedToMinimumLevel: LogEventLevel.Information)
                  .CreateLogger();


            Console.WriteLine(" 1 = ReceiveAllIgmpMulticast");
            Console.WriteLine(" 2 = ReceiveAll");
            Console.Write("IOControl mode (ReceiveAllIgmpMulticast): ");
            string IOControlMode = Console.ReadLine();

            Console.Write("Listen Interface: ");
            listenInterface = Console.ReadLine();


            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Raw, ProtocolType.Igmp);// filter out non IGMP

            // Get a cancel source that cancels when the user presses CTRL+C.
            var userExitSource = GetUserConsoleCancellationSource();

            var cancelToken = userExitSource.Token;

            // Discard our socket when the user cancels.
            using var cancelReg = cancelToken.Register(() => socket.Dispose());


            //
            Log.Information($"Socket bind: { listenInterface }");

            socket.Bind(new IPEndPoint(IPAddress.Parse(listenInterface), 0)); // Which interface to listen on

            //Set the socket  options
            socket.SetSocketOption(SocketOptionLevel.IP,            //Applies only to IP packets
                                       SocketOptionName.HeaderIncluded, //Set the include the header
                                       true);


            byte[] byTrue = new byte[4] { 1, 0, 0, 0 };
            byte[] byOut = new byte[4] { 1, 0, 0, 0 }; //Capture outgoing packets

            // enter promiscuous mode
            if (string.IsNullOrEmpty(IOControlMode) && IOControlMode.Equals("1"))
            {
                socket.IOControl(IOControlCode.ReceiveAllIgmpMulticast, byTrue, byOut);
            }
            else
            {
                socket.IOControl(IOControlCode.ReceiveAll, byTrue, byOut);
            }

            await DoReceiveAsync(socket, cancelToken);

        }



        private static CancellationTokenSource GetUserConsoleCancellationSource()
        {
            var cancellationSource = new CancellationTokenSource();

            Console.CancelKeyPress += (sender, args) =>
            {
                Log.Information("Clossing...");
                Log.CloseAndFlush();
                args.Cancel = true;
                cancellationSource.Cancel();
            };

            return cancellationSource;
        }



        private static async Task DoReceiveAsync(Socket socket, CancellationToken cancelToken)
        {
            // Taking advantage of pre-pinned memory here using the .NET5 POH (pinned object heap).
            var buffer = GC.AllocateArray<byte>(PacketSize, true);
            var bufferMem = buffer.AsMemory();

            while (!cancelToken.IsCancellationRequested)
            {
                try
                {
                    int size = await socket.ReceiveAsync(bufferMem, SocketFlags.None, cancelToken);

                    IPv4Header header = new IPv4Header(buffer, size);
                    IgmpPacket? igmp = IgmpPacket.Parse(header.Data, 0, header.MessageLength);

                    Log.Information($"*** Receive Packet: { DateTimeOffset.Now.ToString() }");
                    Log.Information($"\tIP Header: \n\t\tProtocol: { Enum.GetName(typeof(ProtocolType), header.ProtocolType)  }\n\t\tSourceAddress: { header.SourceAddress.ToString() }\n\t\tDestinationAddress: { header.DestinationAddress.ToString() }\n\t\tData: { BitConverter.ToString(header.Data, 0, header.MessageLength) }");


                    if (header.ProtocolType == ProtocolType.Igmp)
                    {
                        Log.Information($"\tIGMP:");
                        Log.Information($"\t\tType: { Enum.GetName(typeof(IgmpMessageType), igmp.MessageType)  }");
                        Log.Information($"\t\tVersion: { Enum.GetName(typeof(IgmpVersion), igmp.Version)  }");
                        switch (igmp.Version)
                        {
                            case IgmpVersion.Version0:
                                Log.Information($"\t\tGroupAddress: {((IGMPv0Packet)igmp).GroupAddress.ToString() }");
                                break;
                            case IgmpVersion.Version1:
                                Log.Information($"\t\tGroupAddress: {((IGMPv1Packet)igmp).GroupAddress.ToString() }");
                                break;
                            case IgmpVersion.Version2:
                                Log.Information($"\t\tGroupAddress: {((IGMPv2Packet)igmp).GroupAddress.ToString() }");
                                break;
                            case IgmpVersion.Version3:

                                if (igmp.MessageType == IgmpMessageType.MembershipReportVersion3)
                                {
                                    IGMPv3ReportPacket reportPacket = (IGMPv3ReportPacket)igmp;
                                    Log.Information($"\t\tIGMPv3ReportPacket:");
                                    foreach (IGMPv3GroupRecord record in reportPacket.GroupRecord)
                                    {
                                        Console.WriteLine($"\t\t\tGroupRecord: {  Enum.GetName(typeof(IGMPv3GroupRecordType), record.RecordType) }, { record.MulticastAddress.ToString() }");
                                    }
                                }
                                else if (igmp.MessageType == IgmpMessageType.MembershipQuery)
                                {
                                    IGMPv3QueryPacket queryPacket = (IGMPv3QueryPacket)igmp;
                                    Log.Information($"\t\tIGMPv3QueryPacket:");
                                    Log.Information($"\t\t\tGroupAddress: { queryPacket.GroupAddress.ToString() }");
                                }

                                break;
                        }
                    }

                    Log.Information("********\n");
                }
                catch (System.OperationCanceledException)
                {
                    break;
                }
                catch (SocketException)
                {
                    break;
                }
            }
        }

        public static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }

    }

}
