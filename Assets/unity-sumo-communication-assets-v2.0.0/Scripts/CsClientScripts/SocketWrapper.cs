using System.Collections;
using System.Text;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using Google.Protobuf;
using System.Linq;

namespace Tomis.ClientSide
{
    public class SocketWrapper
    {
        private const int MSGLEN = 11, PREFIX_SIZE = 4;
        private Socket _sock;
        private NetworkStream _stream;
        public SocketWrapper(Socket s)
        {
            _sock = s;
            _stream = new NetworkStream(_sock);
        }


        public void connect(string host, int port)
        {
            _sock.Connect(host, port);
        }


        public void Send(string msg)
        {
            string sizPrefix = msg.Length.ToString("D4");
            string newMessage = sizPrefix + msg;
            Debug.Log("Message send is: " + newMessage);
            int msglen = newMessage.Length;
            int bytesSend = 0;
            while (bytesSend < msglen)
            {
                bytesSend += _sock.Send(Encoding.ASCII.GetBytes(newMessage));
            }
        }

        public void Send(IMessage c)
        {
            // var msgSize = c.CalculateSize();

            using (var output = _stream)
            {
                /* Writes the message delimited with the length first */
                c.WriteDelimitedTo(output);
            }
        }

        public byte[] Receive()
        {
            byte[] firstChunk = new byte[4];
            int bytesReceived = 0;
            while (bytesReceived < 4)
                bytesReceived += _sock.Receive(firstChunk);
            int bytesToRead = int.Parse(System.Text.Encoding.Default.GetString(firstChunk));
            List<byte[]> chunks = new List<byte[]>
            {
                firstChunk
            };
            bytesReceived = 0;

            byte[] chunk = new byte[bytesToRead];
            while (bytesReceived < bytesToRead)
            {
                bytesReceived += _sock.Receive(chunk);
                chunks.Add(chunk);
            }
            byte[] finalMessageReceived = chunks.SelectMany(x => x).ToArray();
            return finalMessageReceived;
        }

        public string ReceiveAsString()
        {
            return Encoding.ASCII.GetString(Receive());
        }
    }
}