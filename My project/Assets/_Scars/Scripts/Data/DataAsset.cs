using UnityEngine;

namespace Scars.Data
{
    // 모든 게임 데이터의 공통 부모. 다른 기능은 id로 데이터를 찾는다.
    public abstract class DataAsset : ScriptableObject
    {
        [SerializeField] string id;

        public string Id => id;
    }
}
