using System;
using System.Collections.Generic;
using System.Linq;
using Core;
using Data;
using Data.Events;
using Data.Structures;

namespace Server
{
    public class EventsHandler
    {
        private readonly Game m_Game;

        private readonly Dictionary<int, Queue<object>> m_EventQueues = new();

        private readonly IReadOnlyCollection<ClientInfo> m_Clients;
        
        public EventsHandler(Game game, IReadOnlyCollection<ClientInfo> clientsCollection)
        {
            m_Game = game;
            m_Clients = clientsCollection;
        }

        public void Start()
        {
            m_Game.GameStarted += OnGameStart;
            m_Game.Attacked += OnAttack;
            m_Game.CurrentPlayerChanged += OnCurrentPlayerChanged;
        }

        public void Stop()
        {
            m_Game.GameStarted -= OnGameStart;
            m_Game.Attacked -= OnAttack;
            m_Game.CurrentPlayerChanged -= OnCurrentPlayerChanged;
        }

        public IEnumerable<object> FetchEvents(int playerId)
        {
            if (!m_EventQueues.TryGetValue(playerId, out var eventsQueue))
                yield break;
            
            while (eventsQueue.TryDequeue(out object eventData))
                yield return eventData;
        }

        private void OnGameStart(int playersCount)
        {
            var gameStartEvent = new GameStartEvent
            {
                PlayersCount = playersCount
            };
            
            BroadcastEvent(gameStartEvent);
        }
        
        private void OnAttack(int attackerId, int victimId, int x, int y, bool hit)
        {
            var attackEvent = new AttackEvent
            {
                AttackerId = attackerId,
                VictimId = victimId,
                AttackPosition = new Position(x, y),
                Hit = hit
            };
            
            BroadcastEvent(attackEvent, excludePlayer: attackerId);
        }

        private void OnCurrentPlayerChanged(int newCurrentPlayer)
        {
            var currentPlayerChangedEvent = new CurrentPlayerChangedEvent
            {
                CurrentPlayerId = newCurrentPlayer
            };

            BroadcastEvent(currentPlayerChangedEvent);
        }
        
        private void BroadcastEvent(object e, int excludePlayer = Constants.INVALID_PLAYER_ID)
        {
            foreach (var client in m_Clients)
            {
                int playerId = client.Id;
                if (playerId == excludePlayer || playerId == Constants.INVALID_PLAYER_ID)
                    continue;
                
                EnqueueEvent(playerId, e);
            }
        }
        
        private void EnqueueEvent(int playerId, object e)
        {
            if (!m_EventQueues.TryGetValue(playerId, out var queue))
            {
                queue = new Queue<object>();
                m_EventQueues[playerId] = queue;
            }

            queue.Enqueue(e);
        }
    }
}