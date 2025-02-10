using Core;

namespace Server
{
    public class EditorServerSingleton
    {
        public static Game ActiveGame;
        public static GameServer ActiveServer;
        
        public static void CreateServer(int localPort, int boardWidth, int boardHeight)
        {
            ActiveServer?.Stop();
            
            ActiveGame = new Game(boardWidth, boardHeight);
            ActiveServer = new GameServer(localPort, ActiveGame);
        }
    }
}