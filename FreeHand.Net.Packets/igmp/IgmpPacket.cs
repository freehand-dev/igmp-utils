using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FreeHand.Net.Packets
{

    public enum IgmpVersion
    {
        Unknown,
        Version0,
        Version1,
        Version2,
        Version3,
    }

    public enum IgmpMessageType : byte
    {
        /// <summary>
        /// Illegal type.
        /// </summary>
        None = 0x00,

        /// <summary>
        /// Create Group Request (RFC988).
        /// </summary>
        CreateGroupRequestVersion0 = 0x01,

        /// <summary>
        /// Create Group Reply (RFC988).
        /// </summary>
        CreateGroupReplyVersion0 = 0x02,

        /// <summary>
        /// Join Group Request (RFC988).
        /// </summary>
        JoinGroupRequestVersion0 = 0x03,

        /// <summary>
        /// Join Group Reply (RFC988).
        /// </summary>
        JoinGroupReplyVersion0 = 0x04,

        /// <summary>
        /// Leave Group Request (RFC988).
        /// </summary>
        LeaveGroupRequestVersion0 = 0x05,

        /// <summary>
        /// Leave Group Reply (RFC988).
        /// </summary>
        LeaveGroupReplyVersion0 = 0x06,

        /// <summary>
        /// Confirm Group Request (RFC988).
        /// </summary>
        ConfirmGroupRequestVersion0 = 0x07,

        /// <summary>
        /// Confirm Group Reply (RFC988).
        /// </summary>
        ConfirmGroupReplyVersion0 = 0x08,

        /// <summary>
        /// Membership Query (RFC3376).
        /// </summary>
        MembershipQuery = 0x11,

        /// <summary>
        /// Version 3 Membership Report (RFC3376).
        /// </summary>
        MembershipReportVersion3 = 0x22,

        /// <summary>
        /// Version 1 Membership Report (RFC1112).
        /// </summary>
        MembershipReportVersion1 = 0x12,

        /// <summary>
        /// Version 2 Membership Report (RFC2236).
        /// </summary>
        MembershipReportVersion2 = 0x16,

        /// <summary>
        /// Version 2 Leave Group (RFC2236).
        /// </summary>
        LeaveGroupVersion2 = 0x17,

        /// <summary>
        /// Multicast Traceroute Response.
        /// </summary>
        MulticastTraceRouteResponse = 0x1E,

        /// <summary>
        /// Multicast Traceroute.
        /// </summary>
        MulticastTraceRoute = 0x1F,
    }

    public class IgmpPacket
    {
        /// <summary>
        /// 
        /// </summary>
        public IgmpVersion Version { get; set; } = IgmpVersion.Unknown;

        /// <summary>
        /// The type of the IGMP message of concern to the host-router interaction.
        /// </summary>
        public IgmpMessageType MessageType { get; set; } = IgmpMessageType.None;

        static public IgmpVersion ParseVarsion(byte[] byBuffer, int index, int nReceived)
        {
            using (MemoryStream memoryStream = new MemoryStream(byBuffer, 0, nReceived))
            {
                using (BinaryReader binaryReader = new BinaryReader(memoryStream))
                {                       
                    IgmpMessageType messageType = (IgmpMessageType)binaryReader.ReadByte();
                    switch (messageType)
                    {
                        case IgmpMessageType.CreateGroupRequestVersion0:
                        case IgmpMessageType.CreateGroupReplyVersion0:
                        case IgmpMessageType.JoinGroupRequestVersion0:
                        case IgmpMessageType.JoinGroupReplyVersion0:
                        case IgmpMessageType.LeaveGroupRequestVersion0:
                        case IgmpMessageType.LeaveGroupReplyVersion0:
                        case IgmpMessageType.ConfirmGroupRequestVersion0:
                        case IgmpMessageType.ConfirmGroupReplyVersion0:
                            return IgmpVersion.Version0;

                        /// <summary>
                        /// The IGMP version of a Membership Query message is determined as follows:
                        /// <list type="bullet">
                        ///   <item>IGMPv1 Query: length = 8 octets AND Max Resp Code field is zero.</item>
                        ///   <item>IGMPv2 Query: length = 8 octets AND Max Resp Code field is non-zero.</item>
                        ///   <item>IGMPv3 Query: length >= 12 octets.</item>
                        /// </list>
                        /// If the query message do not match any of the above conditions (e.g., a Query of length 10 octets) Unknown will be returned.
                        /// </summary>
                        case IgmpMessageType.MembershipQuery:
                            if (memoryStream.Length >= 12)
                            {
                                return IgmpVersion.Version3;
  
                            }

                            if (memoryStream.Length == 8)
                            {
                                int maxResponseCode = binaryReader.ReadByte();
                                return maxResponseCode == 0 ? IgmpVersion.Version1 : IgmpVersion.Version2;
                            }

                            return IgmpVersion.Unknown;

                        case IgmpMessageType.MembershipReportVersion1:
                            return IgmpVersion.Version1;

                        case IgmpMessageType.MembershipReportVersion2:
                            return IgmpVersion.Version2;
                            
                        case IgmpMessageType.MembershipReportVersion3:
                            return IgmpVersion.Version3;
                            
                        case IgmpMessageType.LeaveGroupVersion2:
                            return IgmpVersion.Version2;
                            
                        default:
                            return IgmpVersion.Unknown;
                            
                    }
                }
            }
        }


        static public IgmpPacket? Parse(byte[] byBuffer, int index, int nReceived)
        {
            IgmpPacket? packet = default;

                switch (ParseVarsion(byBuffer, index, nReceived))
                {
                    case IgmpVersion.Version1:
                        packet = new IGMPv1Packet(byBuffer, index, nReceived);
                        break;
                    case IgmpVersion.Version2:
                        packet = new IGMPv2Packet(byBuffer, index, nReceived);
                        break;
                    case IgmpVersion.Version3:
                        IgmpMessageType messageType = (IgmpMessageType)byBuffer[0];
                        if (messageType == IgmpMessageType.MembershipReportVersion3)
                        {
                            packet = new IGMPv3ReportPacket(byBuffer, index, nReceived);
                        }
                        else if (messageType == IgmpMessageType.MembershipQuery)
                        {
                            packet = new IGMPv3QueryPacket(byBuffer, index, nReceived);
                        }                       
                        break;
                    default:
                        break;
                }

            return packet;
        }

    }
}