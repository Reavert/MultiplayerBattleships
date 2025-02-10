using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Client.Common;
using Client.Gameplay;
using Data.Events;
using Data.Structures;
using UnityEngine;
using UnityEngine.UI;

namespace Client.Scenes
{
    public class GameplayScene : MonoBehaviour
    {
        [SerializeField] private Camera m_MainCamera;
        
        [SerializeField] private Ship m_ShipPrefab;
        [SerializeField] private Rocket m_RocketPrefab;
        [SerializeField] private Battleground m_BattlegroundPrefab;
        
        [SerializeField] private Battleground m_SelfBattleground;
        [SerializeField] private Transform m_OtherBattlegroundRoot;
        
        [SerializeField] private Toggle m_ReadyToggle;
        [SerializeField] private Button m_RegisterButton;

        [SerializeField] private float m_FetchEventsInterval;

        private Dictionary<int, Battleground> m_Battlegrounds;
        
        private float m_FetchEventsTimer;
        private bool m_FetchEvents;

        private int m_BoardWidth;
        private int m_BoardHeight;

        private GameMode m_GameMode;
        
        private void Awake()
        {
            m_RegisterButton.onClick.AddListener(Register);
            m_ReadyToggle.onValueChanged.AddListener(OnReadyToggleChanged);
            
            (m_BoardWidth, m_BoardHeight) = GameClientManager.Client.GetBoardSize();
            
            m_SelfBattleground.Create(m_BoardWidth, m_BoardHeight);
            m_SelfBattleground.Interactive = true;
        }

        private void OnDestroy()
        {
            m_RegisterButton.onClick.RemoveListener(Register);
            m_ReadyToggle.onValueChanged.RemoveListener(OnReadyToggleChanged);
        }

        private void Update()
        {
            if (m_GameMode == GameMode.ShipsPlacement)
            {
                Ray ray = m_MainCamera.ScreenPointToRay(Input.mousePosition);
                bool selective = Input.GetMouseButtonDown(0);
            
                m_SelfBattleground.SendInteractionRay(ray, selective);
            }
            else if (m_GameMode == GameMode.Attack)
            {
                Ray ray = m_MainCamera.ScreenPointToRay(Input.mousePosition);
                bool selective = Input.GetMouseButtonDown(0);

                for (int i = 0; i < m_Battlegrounds.Count; i++)
                {
                    if (i == GameClientManager.Client.Id)
                        continue;

                    m_Battlegrounds[i].SendInteractionRay(ray, selective);
                }
            }
            
            if (m_FetchEvents)
            {
                m_FetchEventsTimer += Time.deltaTime;
                if (m_FetchEventsTimer > m_FetchEventsInterval)
                {
                    m_FetchEventsTimer = 0.0f;
                    ProcessEvents();
                }
            }
        }

        private void Register()
        {
            m_SelfBattleground.Interactive = false;
            var cells = m_SelfBattleground.GetCells();
            
            var selectedPositions = cells
                .Where(cell => cell.Selected)
                .Select(selectedCell => new Position(selectedCell.Position.x, selectedCell.Position.y))
                .ToArray();

            GameClientManager.Client.Register(selectedPositions);

            foreach (Position selectedPosition in selectedPositions) 
                m_SelfBattleground.PlaceShip(new Vector2Int(selectedPosition.X, selectedPosition.Y));

            m_FetchEvents = true;
        }
        
        private void OnReadyToggleChanged(bool toggle)
        {
            GameClientManager.Client.SetReady(toggle);
        }

        private void ProcessEvents()
        {
            object[] events = GameClientManager.Client.FetchEvents();

            foreach (object e in events)
                ProcessEvent(e);
        }

        private void ProcessEvent(object e)
        {
            switch (e)
            {
                case GameStartEvent gameStartEvent: HandleGameStartEvent(gameStartEvent); break;
                case AttackEvent attackEvent: HandleAttackEvent(attackEvent); break;
                case CurrentPlayerChangedEvent currentPlayerChangedEvent: HandleCurrentPlayerChangedEvent(currentPlayerChangedEvent); break;
            }
        }

        private void HandleGameStartEvent(GameStartEvent gameStartEvent)
        {
            m_Battlegrounds = new Dictionary<int, Battleground>(gameStartEvent.PlayersCount);
                    
            float totalBoardWidth = 0.0f;
            for (int i = 0; i < gameStartEvent.PlayersCount; i++)
            {
                if (i == GameClientManager.Client.Id)
                    m_Battlegrounds[i] = m_SelfBattleground;
                else
                {
                    var otherPlayerBattleground = Instantiate(m_BattlegroundPrefab, m_OtherBattlegroundRoot);
                    otherPlayerBattleground.Create(m_BoardWidth, m_BoardHeight);
                    otherPlayerBattleground.Interactive = true;
                    
                    otherPlayerBattleground.CellSelected += OnOtherBattlegroundCellSelected;
                    
                    m_Battlegrounds[i] = otherPlayerBattleground;

                    totalBoardWidth += otherPlayerBattleground.TotalSize.x;
                }
            }

            float battlegroundPositionX = -totalBoardWidth / 2.0f;
            for (int i = 0; i < gameStartEvent.PlayersCount; i++)
            {
                if (i == GameClientManager.Client.Id)
                    continue;

                float battlegroundWidth = m_Battlegrounds[i].TotalSize.x;
                float battlegroundHalfWidth = battlegroundWidth / 2.0f;

                battlegroundPositionX += battlegroundHalfWidth;
                m_Battlegrounds[i].transform.localPosition = new Vector3(battlegroundPositionX, 0.0f, 0.0f);
                battlegroundPositionX += battlegroundHalfWidth;
            }

            if (GameClientManager.Client.Id == 0)
                m_GameMode = GameMode.Attack;
        }
        
        private void HandleAttackEvent(AttackEvent attackEvent)
        {
            int sourceBattlegroundId = attackEvent.AttackerId;
            int destinationBattlegroundId = attackEvent.VictimId;
            Vector2Int destinationPosition = new Vector2Int(attackEvent.AttackPosition.X, attackEvent.AttackPosition.Y);
            bool hit = attackEvent.Hit;
                
            StartCoroutine(LaunchRocket(sourceBattlegroundId, destinationBattlegroundId, destinationPosition, hit, 3.0f));
        }

        private void HandleCurrentPlayerChangedEvent(CurrentPlayerChangedEvent currentPlayerChangedEvent)
        {
            m_GameMode = GameClientManager.Client.Id == currentPlayerChangedEvent.CurrentPlayerId 
                ? GameMode.Attack 
                : GameMode.Await;
            
            for (int i = 0; i < m_Battlegrounds.Count; i++)
            {
                if (i == GameClientManager.Client.Id)
                    continue;

                m_Battlegrounds[i].Interactive = m_GameMode == GameMode.Attack;
            }
        }
        
        private IEnumerator LaunchRocket(int sourceBattlegroundId, int destinationBattlegroundId, Vector2Int destinationPosition, bool hit, float duration)
        {
            var sourceBattleground = m_Battlegrounds[sourceBattlegroundId];
            var destinationBattleground = m_Battlegrounds[destinationBattlegroundId];

            var source = sourceBattleground.transform.position;
            var destination = destinationBattleground.GetCellWorldCenter(destinationPosition);
            
            var rocketInstance = Instantiate(m_RocketPrefab);
            rocketInstance.SetPath(source, destination, 10.0f);

            var progress = 0.0f;
            while (progress < 1.0f)
            {
                progress += Time.deltaTime / duration;
                rocketInstance.SetProgress(progress);
                
                yield return null;
            }
            
            Destroy(rocketInstance.gameObject);

            if (hit)
            {
                var ship = destinationBattleground.PlaceShip(destinationPosition);
                
                ship.IsSmoking = true;
                ship.Explode();
            }
            else
                destinationBattleground.WaterSplash(destinationPosition);
        }

        private void OnOtherBattlegroundCellSelected(Cell cell)
        {
            if (m_GameMode != GameMode.Attack)
                return;
            
            var targetPlayerId = m_Battlegrounds.First(battleground => battleground.Value == cell.Owner).Key;
            
            bool hit = GameClientManager.Client.Attack(targetPlayerId, cell.Position.x, cell.Position.y);
            StartCoroutine(LaunchRocket(GameClientManager.Client.Id, targetPlayerId, cell.Position, hit, 3.0f));

            m_GameMode = GameMode.Await;
        }
    }
}