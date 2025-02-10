using Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Server
{

#if UNITY_EDITOR
    
    public class ServerLauncher : EditorWindow
    {
        private Label m_StatusLabel;
        private TextField m_PortTextField;
        private TextField m_BoardWidthTextField;
        private TextField m_BoardHeightTextField;
        private Button m_CreateServerButton;
        private Button m_StartServerButton;
        private Button m_StopServerButton;
        private Button m_StartGameButton;
        private Button m_RefreshButton;
        private VisualElement m_PlayersList;
        
        public void CreateGUI()
        {
            m_StatusLabel = new Label();
            m_PortTextField = new TextField("Port: ");
            m_BoardWidthTextField = new TextField("Board Width: ");
            m_BoardHeightTextField = new TextField("Board Height: ");
            
            m_CreateServerButton = new Button(OnCreateServerButtonClick)
            {
                text = "Create server"
            };

            m_StartServerButton = new Button(OnStartServerButtonClick)
            {
                text = "Start server",
                style =
                {
                    color = new StyleColor(Color.green)
                }
            };

            m_StopServerButton = new Button(OnStopServerButtonClick)
            {
                text = "Stop server",
                style =
                {
                    color = new StyleColor(Color.red)
                }
            };

            m_StartGameButton = new Button(OnStartGameButtonClick)
            {
                text = "Start game",
                style =
                {
                    color = new StyleColor(Color.white)
                }
            };

            m_RefreshButton = new Button(Refresh)
            {
                text = "Refresh",
                style =
                {
                    color = new StyleColor(Color.white)
                }
            };
            
            m_PlayersList = new VisualElement();
            
            rootVisualElement.Add(m_StatusLabel);
            rootVisualElement.Add(m_PortTextField);
            rootVisualElement.Add(m_BoardWidthTextField);
            rootVisualElement.Add(m_BoardHeightTextField);
            rootVisualElement.Add(m_CreateServerButton);
            rootVisualElement.Add(m_StartServerButton);
            rootVisualElement.Add(m_StopServerButton);
            rootVisualElement.Add(m_StartGameButton);
            rootVisualElement.Add(m_RefreshButton);
            rootVisualElement.Add(m_PlayersList);
            
            Refresh();
        }

        private void Refresh()
        {
            m_StatusLabel.text = EditorServerSingleton.ActiveServer == null 
                ? "Not Created" 
                : EditorServerSingleton.ActiveServer.IsStarted ? "Started" : "Stopped";

            var color = EditorServerSingleton.ActiveServer == null
                ? Color.gray
                : EditorServerSingleton.ActiveServer.IsStarted ? Color.green : Color.red;
            
            m_StatusLabel.style.color = new StyleColor(color);
            
            m_PortTextField.value = EditorServerSingleton.ActiveServer != null
                ? EditorServerSingleton.ActiveServer.LocalPort.ToString()
                : string.Empty;
            
            m_CreateServerButton.style.display = EditorServerSingleton.ActiveServer == null ? DisplayStyle.Flex : DisplayStyle.None;
            m_StartServerButton.style.display = EditorServerSingleton.ActiveServer != null && !EditorServerSingleton.ActiveServer.IsStarted ? DisplayStyle.Flex : DisplayStyle.None;
            m_StopServerButton.style.display = EditorServerSingleton.ActiveServer != null && EditorServerSingleton.ActiveServer.IsStarted ? DisplayStyle.Flex : DisplayStyle.None;
            m_StartGameButton.style.display = EditorServerSingleton.ActiveServer != null && EditorServerSingleton.ActiveServer.IsStarted && !EditorServerSingleton.ActiveGame.IsGameStarted ? DisplayStyle.Flex : DisplayStyle.None;

            m_PlayersList.Clear();

            if (EditorServerSingleton.ActiveGame != null)
            {
                for (int i = 0; i < EditorServerSingleton.ActiveGame.Players.Count; i++)
                {
                    var player = EditorServerSingleton.ActiveGame.Players[i];
                    m_PlayersList.Add(new Label($"{i}: {(player.Ready ? "Ready" : "Not Ready")}"));
                }
            }
        }

        private void OnCreateServerButtonClick()
        {
            int port = int.Parse(m_PortTextField.text);
            int boardWidth = int.Parse(m_BoardWidthTextField.text);
            int boardHeight = int.Parse(m_BoardHeightTextField.text);

            EditorServerSingleton.CreateServer(port, boardWidth, boardHeight);
            
            Refresh();
        }
        
        private void OnStartServerButtonClick()
        {
            EditorServerSingleton.ActiveServer.Start();
            Refresh();
        }

        private void OnStopServerButtonClick()
        {
            EditorServerSingleton.ActiveServer.Stop();
            Refresh();
        }

        private void OnStartGameButtonClick()
        {
            EditorServerSingleton.ActiveGame.StartGame();
            Refresh();
        }
        
        #region Menu Button
        
        [MenuItem("Server/Show Launcher")]
        public static void ShowServerLauncher()
        {
            GetWindow<ServerLauncher>();
        }
        
        #endregion
        
    }
    
#endif

}