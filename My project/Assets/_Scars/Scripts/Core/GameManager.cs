using UnityEngine;

namespace Scars.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        void Awake() { }

        public void NewGame() { }

        public void GoTo(string scene) { }
    }
}
