using System;
using System.Collections.Generic;
using Core;
using Data;
using Data.Messages;
using Data.Structures;

namespace Server.MessageProcessors
{
    public class PlayerCreationProcessor : MessageProcessor
    {
        private readonly Game m_Game;
        private readonly ClientInfo m_ClientInfo;
        
        public PlayerCreationProcessor(Game game, ClientInfo clientInfo)
        {
            m_Game = game;
            m_ClientInfo = clientInfo;
        }
    
        public override object? Process(object parameter)
        {
            if (parameter is not PlayerCreationRequest request)
                return null;
        
            try
            {
                var board = new Board(m_Game.BoardWidth, m_Game.BoardHeight);
                foreach (Position shipPosition in request.ShipsPositions)
                {
                    board.PlaceShip(shipPosition.X, shipPosition.Y);
                }

                int playerId = m_Game.CreatePlayer(board);
                m_ClientInfo.Id = playerId;
                
                return new PlayerCreationResponse
                {
                    Success = true,
                    PlayerId = playerId
                };
            }
            catch (Exception e)
            {
                return new PlayerCreationResponse
                {
                    Success = false,
                    Message = e.Message,
                    PlayerId = Constants.INVALID_PLAYER_ID
                };
            }
        }
    }
}