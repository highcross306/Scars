using System.Collections.Generic;
using UnityEngine;

namespace Scars.Data
{
    // 게임에 쓰는 데이터를 한곳에 모아 id로 찾는다.
    [CreateAssetMenu(menuName = "Scars/Game Database")]
    public class GameDatabase : ScriptableObject
    {
        [SerializeField] List<DataAsset> assets = new List<DataAsset>();

        public IReadOnlyList<DataAsset> Assets => assets;

        public void Register(DataAsset asset)
        {
            if (asset != null && !assets.Contains(asset)) assets.Add(asset);
        }

        // 해당 타입에서 id가 같은 첫 데이터. 없으면 null
        public T Find<T>(string id) where T : DataAsset
        {
            if (string.IsNullOrEmpty(id)) return null;
            foreach (var asset in assets)
                if (asset is T match && match.Id == id) return match;
            return null;
        }

        public IEnumerable<T> All<T>() where T : DataAsset
        {
            foreach (var asset in assets)
                if (asset is T match) yield return match;
        }

        // 같은 타입 안에서 겹치는 id와 비어 있는 id를 찾는다 (데이터 점검용)
        public List<string> Validate()
        {
            var problems = new List<string>();
            var seen = new HashSet<(System.Type, string)>();
            foreach (var asset in assets)
            {
                if (asset == null) { problems.Add("빈 슬롯"); continue; }
                if (string.IsNullOrEmpty(asset.Id)) { problems.Add($"id 없음: {asset.name}"); continue; }
                if (!seen.Add((asset.GetType(), asset.Id))) problems.Add($"id 중복: {asset.GetType().Name} '{asset.Id}'");
            }
            return problems;
        }
    }
}
