using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FreeHand.Net.Packets
{

    /// <summary>
    /// RFC 988.
    /// Version 0:
    /// <pre>
    /// +-----+------+------+----------+
    /// | Bit | 0-7  | 8-15 | 16-31    |
    /// +-----+------+------+----------+
    /// | 0   | Type | Code | Checksum |
    /// +-----+------+------+----------+
    /// | 32  | Identifier             |
    /// +-----+------------------------+
    /// | 64  | Group Address          |
    /// +-----+------------------------+
    /// | 96  | Access Key             |
    /// |     |                        |
    /// +-----+------------------------+
    /// </pre>
    /// </summary>
    public class IGMPv0Packet: IgmpPacket
    {

        private byte _code;          
        private short _checksum;
        private uint _identifier;
        private uint _groupAddress;   
        private uint _accessKey;


        public IGMPv0Packet(byte[] byBuffer, int index, int nReceived) : base()
        {
            this.Version = IgmpVersion.Version0;

            try
            {
                using (MemoryStream memoryStream = new MemoryStream(byBuffer, 0, nReceived))
                {
                    using (BinaryReader binaryReader = new BinaryReader(memoryStream))
                    {
                        this.MessageType = (IgmpMessageType)binaryReader.ReadByte();
                        this._code = binaryReader.ReadByte();
                        this._checksum = IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());
                        this._identifier = (uint)(binaryReader.ReadInt32());
                        this._groupAddress = (uint)(binaryReader.ReadInt32());
                        this._accessKey = (uint)(binaryReader.ReadInt32());
                    }
                }
              
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }


        public byte Code
        {
            get
            {
                return this._code;
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

        public uint Identifier
        {
            get
            {
                return this._identifier;
            }
        }

        public IPAddress GroupAddress
        {
            get
            {
                return new IPAddress(this._groupAddress);
            }
        }

        public IPAddress AccessKey
        {
            get
            {
                return new IPAddress(this._accessKey);
            }
        }
    }
}