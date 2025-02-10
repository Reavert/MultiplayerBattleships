using System;
using Core;
using Data.Messages;

namespace Server.MessageProcessors
{
    public class GetGameBoardSizeProcessor : MessageProcessor
    {
        private readonly Game m_Game;

        public GetGameBoardSizeProcessor(Game game)
        {
            m_Game = game;
        }
    
        public override object? Process(object parameter)
        {
            if (parameter is not GetGameBoardSizeRequest)
                return null;
        
            try
            {
                int width = m_Game.BoardWidth;
                int height = m_Game.BoardHeight;

                return new GetGameBoardSizeResponse
                {
                    Success = true,
                    Width = width,
                    Height = height,
                };
            }
            catch (Exception e)
            {
                return new GetGameBoardSizeResponse
                {
                    Success = false,
                    Message = e.Message,
                    Width = -1,
                    Height = -1
                };
            }
        }
    }
}