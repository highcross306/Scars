using System;

namespace Scars.Investigation
{
    // 한 방 안의 방향 전환 (예: 동서남북 4장). 끝에서 넘기면 반대쪽 처음으로 돈다
    public class RoomNavigator
    {
        public event Action<int> DirectionChanged;

        public int DirectionCount => 0;

        public int Current => 0;

        public void Begin(int directionCount, int startDirection) { }

        public void TurnLeft() { }

        public void TurnRight() { }

        public void GoTo(int direction) { }
    }
}
