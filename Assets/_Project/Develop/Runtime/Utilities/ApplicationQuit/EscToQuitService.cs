using UnityEngine;

namespace Assets._Project.Develop.Runtime.Utilities.ApplicationQuit
{
    public class EscToQuitService : MonoBehaviour
    {
        private IQuitConfirmationOpener _opener;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        public void Bind(IQuitConfirmationOpener opener)
        {
            _opener = opener;
        }

        public void Unbind(IQuitConfirmationOpener opener)
        {
            if (_opener == opener)
                _opener = null;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape) == false)
                return;

            if (_opener == null)
                return;

            _opener.OpenQuitConfirmation();
        }
    }
}
