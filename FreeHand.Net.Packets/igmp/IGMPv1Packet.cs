using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FreeHand.Net.Packets
{
    /// <summary>
    /// RFC 1112.
    /// Version 1 (query or report):
    /// <pre>
    /// +-----+---------+------+--------+----------+
    /// | Bit | 0-3     | 4-7  | 8-15   | 16-31    |
    /// +-----+---------+------+--------+----------+
    /// | 0   | Version | Type | Unused | Checksum |
    /// +-----+---------+------+--------+----------+
    /// | 32  | Group Address                      |
    /// +-----+------------------------------------+
    /// </pre>
    /// </summary>
    public class IGMPv1Packet: IgmpPacket
    {
        private byte _version;
        private byte _unused;             
        private byte _checksum;                 
        private uint _groupAddress;           

        public IGMPv1Packet(byte[] byBuffer, int index, int nReceived) : base()
        {
            this.Version = IgmpVersion.Version1;

            try
            {
                using (MemoryStream memoryStream = new MemoryStream(byBuffer, 0, nReceived))
                {
                    using (BinaryReader binaryReader = new BinaryReader(memoryStream))
                    {
                        this.MessageType = (IgmpMessageType)binaryReader.ReadByte();
                        this._unused = binaryReader.ReadByte();
                        this._checksum = binaryReader.ReadByte();
                        this._groupAddress = (uint)(binaryReader.ReadInt32());
                    }
                }
              
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public string Checksum
        {
            get
            {
                //Returns the checksum in hexadecimal format
                return string.Format("0x{0:x1}", this._checksum);
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
}