using System.Net;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Client.UI
{
    public class ConnectionPanel : MonoBehaviour
    {
        [SerializeField] private TMP_InputField m_IpAddressInputField;
        [SerializeField] private TMP_InputField m_PortInputField;
        [SerializeField] private Button m_ConnectButton;
        
        public event UnityAction ConnectClicked
        {
            add => m_ConnectButton.onClick.AddListener(value);
            remove => m_ConnectButton.onClick.RemoveListener(value);
        }
        
        public bool IsIPAddressValid => CheckIPAddressValidity(m_IpAddressInputField.text);
        public IPAddress IPAddress => CheckIPAddressValidity(m_IpAddressInputField.text)
            ? IPAddress.Parse(m_IpAddressInputField.text)
            : null;

        public bool IsPortValid => CheckPortValidity(m_PortInputField.text);
        public int? Port => CheckPortValidity(m_PortInputField.text) 
            ? int.Parse(m_PortInputField.text) 
            : null;

        private bool CheckIPAddressValidity(string ipAddressString)
        {
            if (!IPAddress.TryParse(ipAddressString, out IPAddress _))
                return false;

            return true;
        }
        
        private bool CheckPortValidity(string portString)
        {
            if (!int.TryParse(portString, out int port))
                return false;

            if (port <= 1024 || port >= 65535)
                return false;

            return true;
        }
    }
}