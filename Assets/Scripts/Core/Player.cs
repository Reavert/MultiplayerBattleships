namespace Core
{
    public class Player
    {
        private readonly Board m_Board;
    
        public Board Board => m_Board;

        public bool IsAlive => m_Board.HasAliveShips();

        public bool Ready { get; set; }
    
        public Player(Board board)
        {
            m_Board = board;
        }
    }
}