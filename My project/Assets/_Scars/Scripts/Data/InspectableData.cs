using UnityEngine;

namespace Scars.Data
{
    // 조사 대상 하나. 획득 단계(클릭)에서 기본 카드, 인터랙션 및 심층 검식(도구 → 조작 순서 → 판별)에서 업그레이드 카드
    [CreateAssetMenu(menuName = "Scars/Inspectable")]
    public class InspectableData : DataAsset
    {
        [Tooltip("획득 단계에서 받는 기본 카드(겉보기 해석). 비우면 기본 카드 없이 심층 검식만 (예: 숨겨진 장치)")]
        public string grantClueId;

        [Header("인터랙션 및 심층 검식 (업그레이드 카드를 비우면 없음)")]
        [Tooltip("필요한 도구 카드")]
        public string requiredItemId;
        [Tooltip("확대 화면에서 눌러야 하는 대상, 순서대로. 다른 대상을 눌러도 처음으로 돌아가지 않는다")]
        public string[] steps = new string[0];
        [Tooltip("판별 질문. 판별은 한 번: 심층 진실을 고르면 업그레이드 카드 획득, 겉보기 해석을 고르면 기본 카드 유지. 비우면 조작 순서를 마치는 즉시 업그레이드 카드 획득")]
        public QuizData judgment;
        [Tooltip("업그레이드 카드(심층 진실). 기본 카드를 가졌으면 그 자리에서 바뀌고, 기본 카드가 없으면 새로 획득")]
        public string upgradeClueId;
    }
}
