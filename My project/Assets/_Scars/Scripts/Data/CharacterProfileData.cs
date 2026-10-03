using System;
using System.Collections.Generic;
using Scars.Core;
using UnityEngine;

namespace Scars.Data
{
    // 인물 일지 한 명. 프로필 카드로 인벤토리에 지급되고, 가설의 행위자 카드와 연결된다
    [CreateAssetMenu(menuName = "Scars/Character Profile")]
    public class CharacterProfileData : DataAsset
    {
        public string displayName;
        public string role;
        [TextArea] public string identity;
        [TextArea] public string impression;
        public List<ProfileMemo> memos = new List<ProfileMemo>();
    }

    // 조건이 비어 있으면 처음부터 보인다 (조사 중 갱신 여부는 미결이라 조건으로 열 수 있게만 둠)
    [Serializable]
    public class ProfileMemo
    {
        [TextArea] public string text;
        public Condition condition = new Condition();
    }
}
