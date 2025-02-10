using System.Net;
using System.Net.Sockets;
using Data;
using Data.Messages;
using Data.Structures;

namespace Client.Common
{
    public class GameClient
    {
        private readonly JsonNetworkCommunicator m_JsonNetworkCommunicator = new();
        
        private TcpClient m_TcpClient;
        private IPEndPoint m_ServerAddress;
        
        private int m_PlayerId = Constants.INVALID_PLAYER_ID;
        private bool m_PlayerRegistered;
        private bool m_Connected;

        public int Id => m_PlayerId;
        
        public bool Connect(IPEndPoint serverAddress)
        {
            try
            {
                m_TcpClient?.Dispose();

                m_TcpClient = new TcpClient();
                
                m_TcpClient.Connect(serverAddress);
                
                m_Connected = true;
                m_ServerAddress = serverAddress;
                
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void Disconnect()
        {
            m_TcpClient.Dispose();
            
            m_ServerAddress = null;
            m_Connected = false;
        }

        public (int width, int height) GetBoardSize()
        {
            var request = new GetGameBoardSizeRequest();
        
            var response = Request<GetGameBoardSizeResponse, GetGameBoardSizeRequest>(request);
            if (response.Success)
                return (response.Width, response.Height);

            return (-1, -1);
        }

        public void Register(Position[] shipPositions)
        {
            var request = new PlayerCreationRequest
            {
                ShipsPositions = shipPositions
            };

            var response = Request<PlayerCreationResponse, PlayerCreationRequest>(request);
            if (response.Success)
            {
                m_PlayerRegistered = true;
                m_PlayerId = response.PlayerId;
            }
        }

        public void SetReady(bool ready)
        {
            var request = new SetReadyRequest
            {
                PlayerId = m_PlayerId,
                Ready = ready
            };

            Request<SetReadyResponse, SetReadyRequest>(request);
        }

        public bool Attack(int targetPlayerId, int x, int y)
        {
            var request = new AttackRequest
            {
                PlayerId = m_PlayerId,
                TargetId = targetPlayerId,
                Position = new Position(x, y)
            };

            var response = Request<AttackResponse, AttackRequest>(request);
            if (!response.Success)
                return false;
        
            return response.WasHit;
        }

        public object[] FetchEvents()
        {
            var request = new FetchEventRequest
            {
                PlayerId = m_PlayerId
            };

            var response = Request<FetchEventsResponse, FetchEventRequest>(request);
            if (!response.Success)
                return null;

            return response.Events;
        }
        
        private TResponse Request<TResponse, TRequest>(TRequest request)
        {
            var networkStream = m_TcpClient.GetStream();
        
            m_JsonNetworkCommunicator.Write(networkStream, request);
            return (TResponse) m_JsonNetworkCommunicator.Read(networkStream);
        }
    }
}