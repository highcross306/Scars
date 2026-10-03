using UnityEngine;

namespace Scars.Data
{
    [CreateAssetMenu(menuName = "Scars/Ending")]
    public class EndingData : DataAsset
    {
        public string title;
        [TextArea] public string[] epilogue = new string[0];
    }
}
