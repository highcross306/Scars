using System;

namespace Scars.Investigation
{
    // 한 방 안의 방향 전환 (예: 동서남북 4장). 끝에서 넘기면 반대쪽 처음으로 돈다
    public class RoomNavigator
    {
        int _count;
        int _current;

        // 방향이 실제로 바뀌었을 때 (새 방향)
        public event Action<int> DirectionChanged;

        public int DirectionCount => _count;

        public int Current => _current;

        // 알림 없이 시작 방향만 정한다. 화면은 Current를 보고 처음 한 번 그린다
        public void Begin(int directionCount, int startDirection)
        {
            _count = Math.Max(0, directionCount);
            _current = _count > 0 && startDirection >= 0 && startDirection < _count ? startDirection : 0;
        }

        public void TurnLeft() { Set(_current - 1); }

        public void TurnRight() { Set(_current + 1); }

        // 범위 밖이면 무시
        public void GoTo(int direction)
        {
            if (direction >= 0 && direction < _count) Set(direction);
        }

        void Set(int direction)
        {
            if (_count == 0) return;
            int next = ((direction % _count) + _count) % _count;
            if (next == _current) return;
            _current = next;
            DirectionChanged?.Invoke(_current);
        }
    }
}
