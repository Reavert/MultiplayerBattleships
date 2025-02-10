using System;
using System.Linq;
using Core;
using Data.Messages;

namespace Server.MessageProcessors
{
    public class AttackProcessor : MessageProcessor
    {
        private readonly Game m_Game;
    
        public AttackProcessor(Game game)
        {
            m_Game = game;
        }
    
        public override object? Process(object parameter)
        {
            if (parameter is not AttackRequest attackRequest)
                return null;

            try
            {
                var wasHit = m_Game.InputAttack(
                    senderPlayerId: attackRequest.PlayerId,
                    targetPlayerId: attackRequest.TargetId, 
                    x: attackRequest.Position.X, 
                    y: attackRequest.Position.Y);
                
                return new AttackResponse
                {
                    Success = true,
                    WasHit = wasHit
                };
            }
            catch (Exception e)
            {
                return new AttackResponse
                {
                    Success = false,
                    Message = e.Message,
                };
            }
        }
    }
}