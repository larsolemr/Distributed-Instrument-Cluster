﻿using Instrument_Communicator_Library;
using Instrument_Communicator_Library.Server_Listener;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using Instrument_Communicator_Library.Information_Classes;

namespace Server_And_Demo_Project {

    /// <summary>
    /// Used for testing video part of lib
    /// </summary>
    internal class VideoTest {

        public static void Main(string[] args) {
            int portVideo = 5051;
            IPEndPoint endpointVid = new IPEndPoint(IPAddress.Parse("127.0.0.1"), portVideo);

            ListenerVideo<VideoObject> vidListener = new ListenerVideo<VideoObject>(endpointVid);

            Thread videoListenerThread = new Thread(() => vidListener.Start());
            videoListenerThread.Start();

            //Wait so server runs before connecting
            Console.WriteLine("Waiting for server");
            Thread.Sleep(1000);
            //Communicator
            InstrumentInformation info = new InstrumentInformation("Video Communicator 1", "loc", "type");
            AccessToken accessToken = new AccessToken("access");
            CancellationToken comCancellationToken = new CancellationToken(false);

            VideoCommunicator<VideoObject> vidCom = new VideoCommunicator<VideoObject>("127.0.0.1", 5051, info, accessToken, comCancellationToken);
            Thread vidComThread = new Thread(() => vidCom.Start());
            vidComThread.Start();
            Thread.Sleep(1000);

            ConcurrentQueue<VideoObject> inputQueue = vidCom.GetInputQueue();

            List<VideoConnection<VideoObject>> listListenerConnections = vidListener.GetVideoConnectionList();

            for (int i = 0; i < 300; i++) {
                
                inputQueue.Enqueue(new VideoObject("int is "+i));
                Console.WriteLine("Queueing " + "int is " + i);
            }

            var con = listListenerConnections[0];

            ConcurrentQueue<VideoObject> queueOutputQueue = con.GetOutputQueue();

            while (true) {
                if (queueOutputQueue.TryPeek(out VideoObject nahResult)) {
                    queueOutputQueue.TryDequeue(out VideoObject result);
                    Console.WriteLine("Output pushes " + result.GetName());
                }
            }
        }
    }
}