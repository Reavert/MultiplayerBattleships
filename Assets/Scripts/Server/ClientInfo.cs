using System.Net.Sockets;
using Data;

namespace Server
{
    public class ClientInfo
    {
        public int Id = Constants.INVALID_PLAYER_ID;
        public TcpClient TcpClient;
    }
}