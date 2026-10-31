using UnityEngine;
using UnityEngine.UI;

namespace Scars.Investigation
{
    // 방향마다 배경 그림과 Hotspot을 묶은 오브젝트를 하나씩 두고, 현재 방향만 켠다
    public class RoomView : MonoBehaviour
    {
        [Tooltip("방향 순서대로 (오른쪽으로 돌 때 다음 칸)")]
        [SerializeField] GameObject[] directions = new GameObject[0];
        [SerializeField] int startDirection;
        [Tooltip("지금 보는 방향을 띄울 글자 (비워도 된다)")]
        [SerializeField] Text directionLabel;
        [Tooltip("directions와 같은 순서. 비면 오브젝트 이름")]
        [SerializeField] string[] directionNames = new string[0];

        readonly RoomNavigator _navigator = new RoomNavigator();

        void Start()
        {
            _navigator.Begin(directions.Length, startDirection);
            _navigator.DirectionChanged += Show;
            Show(_navigator.Current);
        }

        void OnDestroy()
        {
            _navigator.DirectionChanged -= Show;
        }

        // 화면 좌우 화살표 버튼에 연결
        public void OnLeftArrow() { _navigator.TurnLeft(); }

        public void OnRightArrow() { _navigator.TurnRight(); }

        void Show(int direction)
        {
            for (int i = 0; i < directions.Length; i++)
                if (directions[i] != null) directions[i].SetActive(i == direction);
            if (directionLabel != null && direction >= 0 && direction < directions.Length)
                directionLabel.text = direction < directionNames.Length ? directionNames[direction] : directions[direction].name;
        }
    }
}
