using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Net.Sockets;
using System.IO;
using System.Linq;
using System;
using Google.Protobuf;
using CodingConnected.TraCI.NET.Helpers;


namespace  Tomis.ClientSide
{
    public class Client : MonoBehaviour
    {
        void Start()
        {
            ConnectAsync();
            string firstMsg = "hello from unity";
            SocketWrapper sw = new SocketWrapper(_client.Client);
            print("Sending " + firstMsg + " to server ");

            sw.Send("This is a message from c#");
            string msgReceived = sw.ReceiveAsString();
            car newCar = new car()
            {
                Id = "01",
                Speed = 500
            };

            sw.Send(newCar);
            msgReceived = sw.ReceiveAsString();
            Debug.Log("Message received from server");
            Debug.Log(msgReceived);
        }

        string serverResponse = null;

        #region Default Constant Values
        private const string DEFAULT_HOST = "localhost";
        private const int DEFAULT_PORT = 9999;
        private const int DEFAULT_RECEIVE_BUFFER_SIZE= 8000;
        private const int DEFAULT_SEND_BUFFER_SIZE = 8000;
        #endregion

        #region Connection Settings
        public string Host = DEFAULT_HOST;
        public int Port = DEFAULT_PORT;
        public int ReceiveBufferSize = DEFAULT_SEND_BUFFER_SIZE;
        public int SendBufferSize = DEFAULT_RECEIVE_BUFFER_SIZE;
        #endregion

        private TcpClient _client;
        private NetworkStream _stream;

        private readonly byte[] _receiveBuffer = new byte[32768];

        public void ConnectAsync()
        {
            print("Connection to host " + Host + " on port " + Port);
            _client = new TcpClient
            {
                ReceiveBufferSize = ReceiveBufferSize,
                SendBufferSize = SendBufferSize
            };
            _client.Connect(Host, Port);
            _stream = _client.GetStream();
        }

        public static byte[] ReadFully(Stream input)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                input.CopyTo(ms);
                return ms.ToArray();
            }
        }

        public string SendMessage(car c)
        {
            byte[] byteStream;
            MemoryStream stream = new MemoryStream();
            using (var output = stream)
            {
                c.WriteTo(output);
                byteStream = ReadFully(output);
            };

            _client.Client.Send(byteStream);
            try
            {
                int bytesRead = _stream.Read(_receiveBuffer, 0, ReceiveBufferSize);
                if (bytesRead < 0)
                {
                    // Read returns 0 if the client closes the connection
                    throw new IOException();
                }
                byte[] response = _receiveBuffer.Take(bytesRead).ToArray();

                serverResponse = System.Text.Encoding.UTF8.GetString(response);
            }
            catch (Exception e)
            {
                throw e;
            }
            return serverResponse;
        }

        public string SendMessageToPythonServer(string msg)
        {
            return SendMessageAux(msg);
        }
        private string SendMessageAux(string msg)
        {
            if (!_client.Connected)
            {
                return null;
            }

            byte[] sendData = System.Text.Encoding.ASCII.GetBytes(msg);
            _client.Client.Send(sendData);
            try
            {
                int  bytesRead = _stream.Read(_receiveBuffer, 0, ReceiveBufferSize);
                if (bytesRead < 0)
                {
                    // Read returns 0 if the client closes the connection
                    throw new IOException();
                }
                byte[] response = _receiveBuffer.Take(bytesRead).ToArray();
                _stream.Flush();
                 serverResponse = System.Text.Encoding.UTF8.GetString(response);
            }
            catch (Exception e)
            {
                throw e;
            }
            return serverResponse;
        }
    }

   
}

