using Scars.Core;
using Scars.Data;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Scars.Investigation
{
    // 방 그림 위의 조사 대상. 클릭하면 획득 단계(기본 카드)를 진행한다
    // 임시: 확대 조사(ZoomView)가 생기기 전까지는 기본 카드를 얻으면 사라진다
    // 클릭을 받으려면 Collider2D가 필요하다. 일지·메뉴 같은 UI가 위에 있으면 무시한다
    [RequireComponent(typeof(Collider2D))]
    public class Hotspot : MonoBehaviour
    {
        [SerializeField] InspectableData inspectable;

        readonly InspectionSession _session = new InspectionSession();

        void OnEnable()
        {
            if (IsCollected()) gameObject.SetActive(false);
        }

        void OnMouseDown()
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;
            OnClick();
        }

        public void OnClick()
        {
            var manager = GameManager.Instance;
            if (manager == null || !manager.Flow.CanInvestigate || inspectable == null) return;
            _session.Begin(inspectable, manager.Flow.State);
            if (_session.Collect()) gameObject.SetActive(false);
        }

        bool IsCollected()
        {
            var manager = GameManager.Instance;
            return manager != null && inspectable != null && manager.Flow.State.Clues.Has(inspectable.grantClueId);
        }
    }
}
