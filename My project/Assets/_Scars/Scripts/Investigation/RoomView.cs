using UnityEngine;

namespace Scars.Investigation
{
    // 방향마다 배경 그림과 Hotspot을 묶은 오브젝트를 하나씩 두고, 현재 방향만 켠다
    public class RoomView : MonoBehaviour
    {
        [Tooltip("방향 순서대로 (오른쪽으로 돌 때 다음 칸)")]
        [SerializeField] GameObject[] directions = new GameObject[0];
        [SerializeField] int startDirection;

        void Start() { }

        // 화면 좌우 화살표 버튼에 연결
        public void OnLeftArrow() { }

        public void OnRightArrow() { }

        void Show(int direction) { }
    }
}
