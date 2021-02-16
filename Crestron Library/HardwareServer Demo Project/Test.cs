﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Threading;
using Instrument_Communicator_Library;
using System.Collections.Immutable;
using System.Collections.Concurrent;
/// <summary>
/// Class for testing server/client communication
/// <author>Mikael Nilssen</author>
/// </summary>

namespace Server_And_Demo_Project {

    internal class Test {

        public static void Main(string[] args) {


            int portCrestron = 5050;
            int portVideo = 5051;
            IPEndPoint endpointCrestron = new IPEndPoint(IPAddress.Parse("127.0.0.1"), portCrestron);
            IPEndPoint endpointVideo = new IPEndPoint(IPAddress.Parse("127.0.0.1"), portVideo);
            InstrumentServer instumentServer = new InstrumentServer(endpointCrestron,endpointVideo);
            Thread serverThread = new Thread(() => instumentServer.StartListener());
            serverThread.IsBackground = false;
            serverThread.Start();
            Thread.Sleep(10000);
            //instumentServer.StopServer();
            string ip = "127.0.0.1";
            AccessToken accessToken = new AccessToken("access");
            InstrumentInformation info = new InstrumentInformation("Device 1","Location 1", "sample type");

            CrestronCommunicator client = new CrestronCommunicator(ip,portCrestron, info, accessToken);
            Thread clientThread = new Thread(() => client.start());
            clientThread.Start();

            AccessToken accessToken2 = new AccessToken("access");
            InstrumentInformation info2 = new InstrumentInformation("Device 2", "Location 2", "sample type 2");

            CrestronCommunicator client2 = new CrestronCommunicator(ip, portCrestron, info2, accessToken2);
            Thread clientThread2 = new Thread(() => client2.start());
            clientThread2.Start();

            AccessToken accessToken3 = new AccessToken("acess");
            InstrumentInformation info3 = new InstrumentInformation("Device 3", "Location 3", "sample type 3");

            CrestronCommunicator client3 = new CrestronCommunicator(ip, portCrestron, info3, accessToken3);
            Thread clientThread3 = new Thread(() => client3.start());
            clientThread3.Start();

            Thread.Sleep(20000);
            List<CrestronConnection> crestronConnection = instumentServer.getCrestronConnectionList();
            Thread.Sleep(1000);

            Console.WriteLine("populating messages");
            for (int i = 0; i < crestronConnection.Count; i++) {
                CrestronConnection connection = crestronConnection[i];
                ConcurrentQueue<Message> queue = connection.getInputQueue();
                string[] strings = new string[] { "Hello", "this", "is", "a", "test" };
                Message newMessage = new Message(protocolOption.message, strings);

                queue.Enqueue(newMessage);
            }
            Console.WriteLine("populating messages");
            for (int i = 0; i < crestronConnection.Count; i++) {
                CrestronConnection connection = crestronConnection[i];
                ConcurrentQueue<Message> queue = connection.getInputQueue();
                string[] strings = new string[] { "wow", "i", "dont", "like", "greens" };
                Message newMessage = new Message(protocolOption.message, strings);

                queue.Enqueue(newMessage);
            }

            Console.ReadLine();

        }
    }
}