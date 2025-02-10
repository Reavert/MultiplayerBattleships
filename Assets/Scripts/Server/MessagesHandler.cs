using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using Core;
using Data;
using Data.Messages;
using Server.MessageProcessors;

namespace Server
{
    public class MessagesHandler
    {
        private readonly JsonNetworkCommunicator m_JsonNetworkCommunicator = new();
        private readonly NetworkStream m_NetworkStream;
    
        private Thread m_MessagesLoopThread;
        private bool m_IsWorking;

        private readonly Dictionary<Type, MessageProcessor> m_MessageProcessors = new();
        
        public MessagesHandler(Game game, ClientInfo client, EventsHandler eventsHandler, NetworkStream networkStream)
        {
            m_NetworkStream = networkStream;

            m_MessageProcessors[typeof(PlayerCreationRequest)] = new PlayerCreationProcessor(game, client);
            m_MessageProcessors[typeof(GetGameBoardSizeRequest)] = new GetGameBoardSizeProcessor(game);
            m_MessageProcessors[typeof(SetReadyRequest)] = new SetReadyProcessor(game);
            m_MessageProcessors[typeof(AttackRequest)] = new AttackProcessor(game);
            m_MessageProcessors[typeof(GetGameStateRequest)] = new GetGameStateProcessor(game);
            m_MessageProcessors[typeof(FetchEventRequest)] = new FetchEventsProcessor(eventsHandler);
        }

        public void Start()
        {
            if (m_IsWorking)
                return;

            m_IsWorking = true;
            
            m_MessagesLoopThread = new Thread(MessagesLoop);
            m_MessagesLoopThread.Start();
        }

        public void Stop()
        {
            if (!m_IsWorking)
                return;

            m_IsWorking = false;
            
            m_MessagesLoopThread.Join();
        }

        private void MessagesLoop()
        {
            while (m_IsWorking)
            {
                try
                {
                    object? message = m_JsonNetworkCommunicator.Read(m_NetworkStream);
                    if (message == null)
                        continue;

                    var response = ProcessMessage(message);
                    if (response == null)
                        continue;
                
                    m_JsonNetworkCommunicator.Write(m_NetworkStream, response);
                }
                catch
                {
                    break;
                }
            }
        }
    
        private object? ProcessMessage(object message)
        {
            Type messageType = message.GetType();
            Console.WriteLine($"Processing message: {message}");

            if (m_MessageProcessors.TryGetValue(messageType, out MessageProcessor? processor))
                return processor.Process(message);

            return null;
        }
    }
}