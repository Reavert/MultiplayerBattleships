using System;
using System.Collections.Generic;
using System.Linq;

namespace Core
{
    public class Game
    {
        private readonly int m_BoardWidth;
        private readonly int m_BoardHeight;
    
        private readonly List<Player> m_Players = new(2);
        private int m_CurrentPlayerIndex = -1;
    
        private bool m_IsGameStarted;
        private bool m_IsGameFinished;
    
        public int BoardWidth => m_BoardWidth;
        public int BoardHeight => m_BoardHeight;

        public IReadOnlyList<Player> Players => m_Players;
        public Player? Winner { get; private set; }

        public bool IsGameStarted => m_IsGameStarted;
        public bool IsGameFinished => m_IsGameFinished;
    
        public int AlivePlayersCount => m_Players.Count(player => player.IsAlive);

        public int CurrentPlayer => m_CurrentPlayerIndex;

        public event Action<int, int, int, int, bool> Attacked;
        public event Action<int> GameStarted;
        public event Action<int> CurrentPlayerChanged;
        
        public Game(int boardWidth, int boardHeight)
        {
            m_BoardWidth = boardWidth;
            m_BoardHeight = boardHeight;
        }
    
        public int CreatePlayer(Board board)
        {
            if (m_IsGameStarted)
                throw new InvalidOperationException("Can't create player in game that has already started");

            if (board.Width != m_BoardWidth || board.Height != m_BoardHeight)
                throw new InvalidOperationException("Can't create player with different board size");
        
            Player newPlayer = new Player(board);
        
            m_Players.Add(newPlayer);

            int id = m_Players.IndexOf(newPlayer);
            return id;
        }

        public int GetPlayerId(Player player)
        {
            return m_Players.IndexOf(player);
        }
    
        public void StartGame()
        {
            if (m_Players.Count < 2)
                throw new InvalidOperationException("Can't start game with single player");

            if (m_Players.Any(player => !player.Ready))
                throw new InvalidOperationException("Can't start game until all players are ready");
        
            m_IsGameStarted = true;
            m_CurrentPlayerIndex = 0;
            
            GameStarted?.Invoke(m_Players.Count);
        }

        public void SetPlayerReady(int playerId, bool ready)
        {
            Player player = GetPlayer(playerId);
            player.Ready = ready;
        }
    
        public bool InputAttack(int senderPlayerId, int targetPlayerId, int x, int y)
        {
            if (m_CurrentPlayerIndex != senderPlayerId)
            {
                throw new InvalidOperationException(
                    $"Current player is {m_CurrentPlayerIndex}. But player with index {senderPlayerId} is trying to move");
            }

            var currentPlayer = GetPlayer(senderPlayerId);
            if (!currentPlayer.IsAlive)
                throw new InvalidOperationException("Not alive player can't move");
        
            var wasHit = Attack(senderPlayerId, targetPlayerId, x, y);
            
            
            if (AlivePlayersCount > 1)
            {
                NextMove();
            }
            else
            {
                m_IsGameFinished = true;
                Winner = m_Players.First(player => player.IsAlive);
            }

            return wasHit;
        }
    
        private bool Attack(int attackerIndex, int targetPlayerIndex, int x, int y)
        {
            if (attackerIndex == targetPlayerIndex)
                throw new InvalidOperationException("Can't attack yourself");

            Player targetPlayer = GetPlayer(targetPlayerIndex);
            bool hit = targetPlayer.Board.AttackCell(x, y);

            Attacked?.Invoke(attackerIndex, targetPlayerIndex, x, y, hit);
            return hit;
        }
    
        public Player GetPlayer(int index)
        {
            if (index < 0 || index >= m_Players.Count)
                throw new IndexOutOfRangeException($"No player with index {index}");

            return m_Players[index];
        }

        private Player GetCurrentPlayer()
        {
            if (!m_IsGameStarted)
                throw new InvalidOperationException("No current player until game started");

            return m_Players[m_CurrentPlayerIndex];
        }
    
        private void NextMove()
        {
            do
            {
                m_CurrentPlayerIndex++;
                if (m_CurrentPlayerIndex >= m_Players.Count)
                    m_CurrentPlayerIndex = 0;
            } while (!m_Players[m_CurrentPlayerIndex].IsAlive);
            
            CurrentPlayerChanged?.Invoke(m_CurrentPlayerIndex);
        }
    }
}