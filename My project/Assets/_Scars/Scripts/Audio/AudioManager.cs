using UnityEngine;

namespace Scars.Audio
{
    // GameManager 오브젝트의 자식으로 두어 씬이 바뀌어도 유지한다
    public class AudioManager : MonoBehaviour
    {
        [SerializeField] AudioSource bgmSource;
        [SerializeField] AudioSource sfxSource;

        public static AudioManager Instance { get; private set; }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        public void Play(string sound) { }
    }
}
