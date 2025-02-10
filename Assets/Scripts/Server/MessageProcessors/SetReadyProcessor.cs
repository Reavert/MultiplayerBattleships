using System;
using Core;
using Data.Messages;

namespace Server.MessageProcessors
{
    public class SetReadyProcessor : MessageProcessor
    {
        private readonly Game m_Game;
    
        public SetReadyProcessor(Game game)
        {
            m_Game = game;
        }
    
        public override object? Process(object parameter)
        {
            if (parameter is not SetReadyRequest setReadyRequest)
                return null;

            try
            {
                m_Game.SetPlayerReady(setReadyRequest.PlayerId, setReadyRequest.Ready);
                return new SetReadyResponse
                {
                    Success = true
                };
            }
            catch (Exception e)
            {
                return new SetReadyResponse
                {
                    Success = false,
                    Message = e.Message
                };
            }
        
        }
    }
}