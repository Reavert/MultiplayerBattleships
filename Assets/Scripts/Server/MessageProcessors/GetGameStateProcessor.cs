using System;
using Core;
using Data.Messages;

namespace Server.MessageProcessors
{
    public class GetGameStateProcessor : MessageProcessor
    {
        private readonly Game m_Game;

        public GetGameStateProcessor(Game game)
        {
            m_Game = game;
        }
    
        public override object? Process(object parameter)
        {
            if (parameter is not GetGameStateRequest)
                return null;

            try
            {
                return new GetGameStateResponse
                {
                    Success = true,
                    IsGameStarted = m_Game.IsGameStarted,
                    IsGameFinished = m_Game.IsGameFinished,
                    WinnerId = m_Game.Winner != null ? m_Game.GetPlayerId(m_Game.Winner) : -1
                };
            }
            catch (Exception e)
            {
                return new GetGameStateResponse
                {
                    Success = false,
                    Message = e.Message
                };
            }
        }
    }
}