using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CoinDash.UI
{
    public class LoginView : MonoBehaviour
    {
        [SerializeField] private TMP_InputField     _nameInputField;
        [SerializeField] private Button             _connectButton;
        public TMP_InputField                       NameInputField => _nameInputField;
        public Button                               ConnectButton => _connectButton;
    }
}