using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace FreeHand.Net.Packets
{
    public class IPv4Header
    {
        #region IP Header fields 

        /// <summary>
        /// Eight bits for version and header length
        /// </summary>
        private byte _versionAndHeaderLength;

        /// <summary>
        /// Eight bits for differentiated services
        /// </summary>
        private byte _differentiatedServices;

        /// <summary>
        /// Sixteen bits for total length 
        /// </summary>
        private ushort _totalLength;

        /// <summary>
        /// Sixteen bits for identification
        /// </summary>
        private ushort _identification;

        /// <summary>
        /// Eight bits for flags and frag. offset
        /// </summary>
        private ushort _flagsAndOffset;

        /// <summary>
        /// Eight bits for TTL (Time To Live) 
        /// </summary>
        private byte _ttl;

        /// <summary>
        /// Eight bits for the underlying protocol
        /// </summary>
        private byte _protocol;

        /// <summary>
        /// Sixteen bits for checksum of the header
        /// </summary>
        private short _checksum;    

        /// <summary>
        /// Thirty two bit source IP Address
        /// </summary>
        private uint _sourceIPAddress;         

        /// <summary>
        /// Thirty two bit destination IP Address 
        /// </summary>
        private uint _destinationIPAddress;   

        #endregion

        /// <summary>
        /// Header length 
        /// </summary>
        private byte _headerLength;             

        /// <summary>
        /// Data carried by the datagram
        /// </summary>
        private byte[] _data = new byte[4096];


        public IPv4Header(byte[] byBuffer, int nReceived)
        {
            try
            {
                //Create MemoryStream out of the received bytes
                MemoryStream memoryStream = new MemoryStream(byBuffer, 0, nReceived);

                //Next we create a BinaryReader out of the MemoryStream
                BinaryReader binaryReader = new BinaryReader(memoryStream);

                //The first eight bits of the IP header contain the version and
                //header length so we read them
                this._versionAndHeaderLength = binaryReader.ReadByte();

                //The next eight bits contain the Differentiated services
                this._differentiatedServices = binaryReader.ReadByte();

                //Next eight bits hold the total length of the datagram
                this._totalLength =
                         (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());

                //Next sixteen have the identification bytes
                this._identification =
                          (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());

                //Next sixteen bits contain the flags and fragmentation offset
                this._flagsAndOffset =
                          (ushort)IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());

                //Next eight bits have the TTL value
                this._ttl = binaryReader.ReadByte();

                //Next eight represent the protocol encapsulated in the datagram
                this._protocol = binaryReader.ReadByte();

                //Next sixteen bits contain the checksum of the header
                this._checksum = IPAddress.NetworkToHostOrder(binaryReader.ReadInt16());

                //Next thirty two bits have the source IP address
                this._sourceIPAddress = (uint)(binaryReader.ReadInt32());

                //Next thirty two hold the destination IP address
                this._destinationIPAddress = (uint)(binaryReader.ReadInt32());

                //Now we calculate the header length
                this._headerLength = this._versionAndHeaderLength;

                //The last four bits of the version and header length field contain the
                //header length, we perform some simple binary arithmetic operations to
                //extract them
                this._headerLength <<= 4;
                this._headerLength >>= 4;

                //Multiply by four to get the exact header length
                this._headerLength *= 4;

                             //Copy the data carried by the datagram into another array so that
                //according to the protocol being carried in the IP datagram
                Array.Copy(byBuffer,
                           this._headerLength, //start copying from the end of the header
                           this.Data, 0, this._totalLength - this._headerLength);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }


        public AddressFamily Version
        {
            get
            {
                //Calculate the IP version

                //The four bits of the IP header contain the IP version
                if ((this._versionAndHeaderLength >> 4) == 4)
                {
                    return AddressFamily.InterNetwork;
                }
                else if ((this._versionAndHeaderLength >> 4) == 6)
                {
                    return AddressFamily.InterNetworkV6;
                }
                else
                {
                    return AddressFamily.Unknown;
                }
            }
        }

        public int HeaderLength
        {
            get
            {
                return this._headerLength;
            }
        }

        public ushort MessageLength
        {
            get
            {
                //MessageLength = Total length of the datagram - Header length
                return (ushort)(this._totalLength - this._headerLength);
            }
        }

        public string DifferentiatedServices
        {
            get
            {
                //Returns the differentiated services in hexadecimal format
                return string.Format("0x{0:x2} ({1})", this._differentiatedServices,
                    this._differentiatedServices);
            }
        }

        public string Flags
        {
            get
            {
                //The first three bits of the flags and fragmentation field 
                //represent the flags (which indicate whether the data is 
                //fragmented or not)
                int nFlags = this._flagsAndOffset >> 13;
                if (nFlags == 2)
                {
                    return "Don't fragment";
                }
                else if (nFlags == 1)
                {
                    return "More fragments to come";
                }
                else
                {
                    return nFlags.ToString();
                }
            }
        }

        public string FragmentationOffset
        {
            get
            {
                //The last thirteen bits of the flags and fragmentation field 
                //contain the fragmentation offset
                int nOffset = this._flagsAndOffset << 3;
                nOffset >>= 3;

                return nOffset.ToString();
            }
        }

        public int TTL
        {
            get
            {
                return this._ttl;
            }
        }

        /// <summary>
        /// The protocol field represents the protocol in the data portion of the datagram
        /// </summary>
        public ProtocolType ProtocolType
        {
            get
            {
                return (ProtocolType)this._protocol;
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

        public IPAddress SourceAddress
        {
            get
            {
                return new IPAddress(this._sourceIPAddress);
            }
        }

        public IPAddress DestinationAddress
        {
            get
            {
                return new IPAddress(this._destinationIPAddress);
            }
        }

        public ushort TotalLength
        {
            get
            {
                return this._totalLength;
            }
        }

        public ushort Identification
        {
            get
            {
                return this._identification;
            }
        }


        public byte[] Data
        {
            get
            {
                return this._data;
            }
        }

    }
}