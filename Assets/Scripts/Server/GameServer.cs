using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Core;

namespace Server
{
    public class GameServer
    {
        private readonly TcpListener m_Listener;

        private bool m_IsConnectionsAccepting;
        private CancellationTokenSource m_ConnectionsAcceptCancellationTokenSource;
        private Thread m_ConnectionsAcceptThread;

        private readonly ConcurrentBag<ClientInfo> m_Clients = new();

        private readonly Game m_Game;
        private readonly int m_LocalPort;

        private readonly EventsHandler m_EventsHandler;
        
        public bool IsStarted => m_IsConnectionsAccepting;
        public int LocalPort => m_LocalPort;
        
        public GameServer(int localPort, Game game)
        {
            m_Listener = new TcpListener(IPAddress.Any, localPort);
            m_Game = game;
            m_LocalPort = localPort;
            
            m_EventsHandler = new EventsHandler(m_Game, m_Clients);
        }

        public void Start()
        {
            if (m_IsConnectionsAccepting)
                return;
        
            m_Listener.Start();
            m_EventsHandler.Start();
            
            m_ConnectionsAcceptThread = new Thread(ConnectionAcceptHandler);
            m_ConnectionsAcceptThread.Start();
            m_ConnectionsAcceptCancellationTokenSource = new CancellationTokenSource();
            m_IsConnectionsAccepting = true;
        }

        public void Stop()
        {
            if (!m_IsConnectionsAccepting)
                return;
        
            m_Listener.Stop();
            m_EventsHandler.Stop();
            
            m_ConnectionsAcceptCancellationTokenSource.Cancel();
            m_IsConnectionsAccepting = false;

            m_ConnectionsAcceptThread.Join();
        }

        private async void ConnectionAcceptHandler()
        {
            while (!m_ConnectionsAcceptCancellationTokenSource.IsCancellationRequested)
            {
                try
                {
                    TcpClient client = await m_Listener.AcceptTcpClientAsync();
                    ClientInfo clientInfo = new ClientInfo
                    {
                        TcpClient = client
                    };
                    
                    m_Clients.Add(clientInfo);
                    
                    MessagesHandler messagesHandler = new MessagesHandler(m_Game, clientInfo, m_EventsHandler, client.GetStream());
                    messagesHandler.Start();
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }
        }
    }
}