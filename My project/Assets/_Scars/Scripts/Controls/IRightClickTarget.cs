using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Scars.Controls
{
    // 우클릭을 스스로 쓰는 UI (예: 가설 슬롯 카드 해제). 그 위에서 우클릭하면 InputRouter는 뒤로(Back)를 하지 않는다
    public interface IRightClickTarget { }

    public static class RightClick
    {
        static readonly List<RaycastResult> Hits = new List<RaycastResult>();

        public static bool IsOverTarget()
        {
            var events = EventSystem.current;
            if (events == null) return false;
            var pointer = new PointerEventData(events) { position = Input.mousePosition };
            Hits.Clear();
            events.RaycastAll(pointer, Hits);
            foreach (var hit in Hits)
                if (hit.gameObject.GetComponentInParent<IRightClickTarget>() != null) return true;
            return false;
        }
    }
}
