using Client.Common;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Client.Scenes
{
    public class BootstrapScene : MonoBehaviour
    {
        private void Awake()
        {
            SceneManager.LoadScene(SceneNames.MENU);
        }
    }
}