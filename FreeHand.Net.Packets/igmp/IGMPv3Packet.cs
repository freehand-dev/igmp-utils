using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FreeHand.Net.Packets
{

   public enum IGMPv3GroupRecordType : byte
    {
        MODE_IS_INCLUDE = 0x01,
        MODE_IS_EXCLUDE = 0x02,
        CHANGE_TO_INCLUDE_MODE = 0x03,
        CHANGE_TO_EXCLUDE_MODE = 0x04,
        ALLOW_NEW_SOURCES = 0x05,
        BLOCK_OLD_SOURCES = 0x06,
    }


    public class IGMPv3Packet: IgmpPacket
    {

        private byte _maxResponseTime;        
        private short _checksum;               


        public IGMPv3Packet(byte[] byBuffer, int index, int nReceived) : base()
        {

            this.Version = IgmpVersion.Version3;

            try
            {
                using (MemoryStream memoryStream = new MemoryStream(byBuffer, 0, nReceived))
                {
                    using (BinaryReader binaryReader = new BinaryReader(memoryStream))
                    {
                        this.MessageType = (IgmpMessageType)binaryReader.ReadByte();
                        this._maxResponseTime = binaryReader.ReadByte();
                        this._checksum = IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());
                    }
                }
              
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public TimeSpan MaxResponseTime
        {
            get
            {
                return TimeSpan.FromMilliseconds(this._maxResponseTime);
            }
        }

        public string Checksum
        {
            get
            {
                //Returns the checksum in hexadecimal format
                return string.Format("0x{0:x2}", this._checksum);
            }
        }

    }


    /// <summary>
    /// RFC 3376.
    /// Version 3 query:
    /// <pre>
    /// +-----+------+---+-----+---------------+-----------------------+
    /// | Bit | 0-3  | 4 | 5-7 | 8-15          | 16-31                 |
    /// +-----+------+---+-----+---------------+-----------------------+
    /// | 0   | Type = 0x11    | Max Resp Code | Checksum              |
    /// +-----+----------------+---------------+-----------------------+
    /// | 32  | Group Address                                          |
    /// +-----+------+---+-----+---------------+-----------------------+
    /// | 64  | Resv | S | QRV | QQIC          | Number of Sources (N) |
    /// +-----+------+---+-----+---------------+-----------------------+
    /// | 96  | Source Address [1]                                     |
    /// +-----+--------------------------------------------------------+
    /// | 128 | Source Address [2]                                     |
    /// +-----+--------------------------------------------------------+
    /// .     .                         .                              .
    /// .     .                         .                              .
    /// +-----+--------------------------------------------------------+
    /// | 64  | Source Address [N]                                     |
    /// | +   |                                                        |
    /// | 32N |                                                        |
    /// +-----+--------------------------------------------------------+
    /// </pre>
    /// 
    /// </summary>
    public class IGMPv3QueryPacket : IGMPv3Packet
    {

        private uint _groupAddress;

        public IGMPv3QueryPacket(byte[] byBuffer, int index, int nReceived) : base(byBuffer, index, nReceived)
        {
            try
            {
                using (MemoryStream memoryStream = new MemoryStream(byBuffer, 4, nReceived - 4))
                {
                    using (BinaryReader binaryReader = new BinaryReader(memoryStream))
                    {
                        this._groupAddress = (uint)(binaryReader.ReadInt32());
                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public IPAddress GroupAddress
        {
            get
            {
                return new IPAddress(this._groupAddress);
            }
        }
    }

    /// <summary>
    /// 
    /// Group Record
    /// +-----+-------------+---------------+-----------------------------+
    /// | Bit | 0-7         | 8-15          | 16-31                       |
    /// +-----+-------------+---------------+-----------------------------+
    /// | 0   | Type        | Aux data len  | Num Source                  |
    /// +-----+-------------+---------------+-----------------------------+
    /// | 32  | Multicast Address                                         |
    /// +-----+-----------------------------------------------------------+
    /// | 64  | Source Address                                            |
    /// +-----+-----------------------------------------------------------+
    /// </summary>
    public class IGMPv3GroupRecord
    {
        public IGMPv3GroupRecordType RecordType;
        public byte AuxDataLength;
        public IPAddress MulticastAddress;
        public List<IPAddress> SourceAddress = new List<IPAddress>();
    }


    /// <summary>
    /// 
    /// RFC 3376.
    /// Version 3 report:
    /// <pre>
    /// +-----+-------------+----------+-----------------------------+
    /// | Bit | 0-7         | 8-15     | 16-31                       |
    /// +-----+-------------+----------+-----------------------------+
    /// | 0   | Type = 0x22 | Reserved | Checksum                    |
    /// +-----+-------------+----------+-----------------------------+
    /// | 32  | Reserved               | Number of Group Records (M) |
    /// +-----+------------------------+-----------------------------+
    /// | 64  | Group Record [1]                                     |
    /// .     .                                                      .
    /// .     .                                                      .
    /// .     .                                                      .
    /// |     |                                                      |
    /// +-----+------------------------------------------------------+
    /// |     | Group Record [2]                                     |
    /// .     .                                                      .
    /// .     .                                                      .
    /// .     .                                                      .
    /// |     |                                                      |
    /// +-----+------------------------------------------------------+
    /// |     |                         .                            |
    /// .     .                         .                            .
    /// |     |                         .                            |
    /// +-----+------------------------------------------------------+
    /// |     | Group Record [M]                                     |
    /// .     .                                                      .
    /// .     .                                                      .
    /// .     .                                                      .
    /// |     |                                                      |
    /// +-----+------------------------------------------------------+
    /// 
    /// </summary>
    public class IGMPv3ReportPacket : IGMPv3Packet
    {

        private short _reserved1;

        private List<IGMPv3GroupRecord> _groupRecord = new List<IGMPv3GroupRecord>();


        public IGMPv3ReportPacket(byte[] byBuffer, int index, int nReceived) : base(byBuffer, index, nReceived)
        {
            try
            {
                using (MemoryStream memoryStream = new MemoryStream(byBuffer, 4, nReceived - 4))
                {
                    using (BinaryReader binaryReader = new BinaryReader(memoryStream))
                    {
                        this._reserved1 = IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());

                        short numberOfGroupRecords = IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());

                        for (int i = 0; i < numberOfGroupRecords; i++)
                        {
                            IGMPv3GroupRecord record = new IGMPv3GroupRecord();

                            record.RecordType = (IGMPv3GroupRecordType)binaryReader.ReadByte();
                            record.AuxDataLength = binaryReader.ReadByte();
                            short numSource = IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());
                            record.MulticastAddress = new IPAddress((uint)(binaryReader.ReadInt32()));

                            for (int j = 0; j < numSource; j++)
                            {
                                record.SourceAddress.Add(
                                    new IPAddress((uint)(binaryReader.ReadInt32())));
                            }

                            this._groupRecord.Add(record);
                        }

                    }
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public List<IGMPv3GroupRecord> GroupRecord
        {
            get
            {
                return this._groupRecord;
            }
        }
    }
}


