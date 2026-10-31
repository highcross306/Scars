using Scars.Core;
using UnityEngine;

namespace Scars.UI
{
    // 시작 화면. 플레이 버튼 하나
    public class TitleView : MonoBehaviour
    {
        // 플레이 버튼에 연결
        public void OnPlay()
        {
            if (GameManager.Instance != null) GameManager.Instance.NewGame();
        }
    }
}
