using System.Net;
using Client.Common;
using Client.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Client.Scenes
{
    public class MenuScene : MonoBehaviour
    {
        [SerializeField] private ConnectionPanel m_ConnectionPanel;

        private bool m_ConnectionResultReceived;
        
        private void Awake()
        {
            m_ConnectionPanel.ConnectClicked += OnConnectClicked;
        }

        private void OnDestroy()
        {
            m_ConnectionPanel.ConnectClicked -= OnConnectClicked;
        }

        private void OnConnectClicked()
        {
            if (!m_ConnectionPanel.IsIPAddressValid)
                return;
            
            if (!m_ConnectionPanel.IsPortValid)
                return;
            
            var serverIpEndPoint = new IPEndPoint(m_ConnectionPanel.IPAddress, m_ConnectionPanel.Port.Value);

            bool success = GameClientManager.Client.Connect(serverIpEndPoint);
            if (success)
                SceneManager.LoadScene(SceneNames.GAMEPLAY);
            else
                Debug.LogError("Can not connect");
        }
    }
}