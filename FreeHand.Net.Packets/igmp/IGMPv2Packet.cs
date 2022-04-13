using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace FreeHand.Net.Packets
{
    /// <summary>
    /// RFC 2236.
    /// Version 2 (query, report or leave group):
    /// <pre>
    /// +-----+------+---------------+----------+
    /// | Bit | 0-7  | 8-15          | 16-31    |
    /// +-----+------+---------------+----------+
    /// | 0   | Type | Max Resp Time | Checksum |
    /// +-----+------+---------------+----------+
    /// | 32  | Group Address                   |
    /// +-----+---------------------------------+
    /// </pre>
    /// </summary>
    public class IGMPv2Packet: IgmpPacket
    {

        /// <summary>
        /// Eight bits
        /// </summary>
        private byte _maxResponseTime;

        /// <summary>
        /// Sixteen bits for checksum of the 
        /// </summary>
        private short _checksum;                                

        /// <summary>
        /// Thirty two bit Group Address
        /// </summary>
        private uint _groupAddress;                             

        public IGMPv2Packet(byte[] byBuffer, int index, int nReceived) : base()
        {

            this.Version = IgmpVersion.Version2;

            try
            {
                using (MemoryStream memoryStream = new MemoryStream(byBuffer, 0, nReceived))
                {
                    using (BinaryReader binaryReader = new BinaryReader(memoryStream))
                    {
                        this.MessageType = (IgmpMessageType)binaryReader.ReadByte();
                        this._maxResponseTime = binaryReader.ReadByte();
                        this._checksum = IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());
                        this._groupAddress = (uint)(binaryReader.ReadInt32());
                    }
                }
              
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }


        /// <summary>
        /// 
        /// </summary>
        public TimeSpan MaxResponseTime
        {
            get
            {
                return TimeSpan.FromMilliseconds(this._maxResponseTime);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public string Checksum
        {
            get
            {
                //Returns the checksum in hexadecimal format
                return string.Format("0x{0:x2}", this._checksum);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public IPAddress GroupAddress
        {
            get
            {
                return new IPAddress(this._groupAddress);
            }
        }
    }
}