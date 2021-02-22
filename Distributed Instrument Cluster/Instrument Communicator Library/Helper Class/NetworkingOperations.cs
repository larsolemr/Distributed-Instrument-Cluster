﻿using System;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace Instrument_Communicator_Library.Helper_Class {

    public static class NetworkingOperations {
        /// <summary>
        /// Send an object with the socket
        /// </summary>
        /// <param name="inObj"></param>
        /// <param name="connectionSocket"></param>
        public static void SendObjectWithSocket<U>(U input, Socket connectionSocket) {
            object obj = (object) input;
            //Get bytes of object T
            NetworkStream networkStream = new NetworkStream(connectionSocket);
            BinaryFormatter br = new BinaryFormatter();
            br.Serialize(networkStream,obj);
            networkStream.Flush();
            
        }

        /// <summary>
        /// Receive an object with socket
        /// </summary>
        /// <param name="connectionSocket"></param>
        /// <returns></returns>
        public static TU ReceiveObjectWithSocket<TU>(Socket connectionSocket) {
            //Create network stream
            NetworkStream networkStream = new NetworkStream(connectionSocket);
            IFormatter br = new BinaryFormatter();
            //Deserialize object from stream
            TU deserialized = (TU) br.Deserialize(networkStream);
            networkStream.Flush();
            return (TU) deserialized;
        }

        /// <summary>
        /// Receive an string with the given socket
        /// </summary>
        /// <param name="connectionSocket">Connected socket</param>
        /// <returns>string</returns>
        public static string ReceiveStringWithSocket(Socket connectionSocket) {
            //Get size of incoming object
            byte[] sizeOfIncomingBuffer = new byte[sizeof(int)];
            connectionSocket.Blocking = true;
            connectionSocket.Receive(sizeOfIncomingBuffer, 0, sizeof(int), SocketFlags.None);
            //extract int
            int sizeOfIncoming = BitConverter.ToInt32(sizeOfIncomingBuffer);
            //receive main object
            byte[] incomingObjectBuffer = new byte[sizeOfIncoming];
            connectionSocket.Receive(incomingObjectBuffer, 0, sizeOfIncoming, SocketFlags.None);
            //get string from object
            string receivedObj = Encoding.ASCII.GetString(incomingObjectBuffer);
            return receivedObj;
        }

        /// <summary>
        /// Send a string with socket
        /// </summary>
        /// <param name="str">String to send</param>
        /// <param name="connectionSocket">Connected socket</param>
        public static void SendStringWithSocket(string str, Socket connectionSocket) {
            //Send name
            string encodingTarget = str;
            byte[] stringBuffer = Encoding.ASCII.GetBytes(encodingTarget);
            byte[] sizeBuffer = new byte[sizeof(int)];
            sizeBuffer = BitConverter.GetBytes(stringBuffer.Length);
            connectionSocket.Blocking = true;
            connectionSocket.Send(sizeBuffer, sizeof(int), SocketFlags.None);

            //Send message string to client
            connectionSocket.Send(stringBuffer, stringBuffer.Length, SocketFlags.None);
        }

        /// <summary>
        /// https://stackoverflow.com/questions/1318933/c-sharp-int-to-byte
        /// </summary>
        /// <param name="i">int to convert</param>
        /// <returns>byte array</returns>
        public static byte[] GetBytesFromInt(int i) {
            byte[] intBytes = BitConverter.GetBytes(i);
            if (BitConverter.IsLittleEndian) Array.Reverse(intBytes);
            return intBytes;
        }
        
    }
}