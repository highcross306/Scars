using System;
using System.Collections.Generic;
using Scars.Core;
using Scars.Data;
using UnityEngine;

namespace Scars.Dialogue
{
    // 대화·심문 한 묶음. 대사 안의 {player}는 플레이어 이름으로 바뀐다
    [CreateAssetMenu(menuName = "Scars/Dialogue")]
    public class DialogueData : DataAsset
    {
        public string startNodeId;
        public List<DialogueNode> nodes = new List<DialogueNode>();
    }

    [Serializable]
    public class DialogueNode
    {
        public string id;
        [Tooltip("CharacterProfileData id. 비우면 내레이션·독백")]
        public string speakerId;
        [TextArea] public string text;
        public GameEffect onEnter = new GameEffect();
        [Tooltip("선택지가 없을 때 다음 대사")]
        public string nextId;
        public List<DialogueChoice> choices = new List<DialogueChoice>();
    }

    [Serializable]
    public class DialogueChoice
    {
        [TextArea] public string text;
        [Tooltip("이 조건을 만족해야 선택지가 보인다 (예: 진실 가설일 때만)")]
        public Condition condition = new Condition();
        [Tooltip("증거 제시 선택지면 내야 하는 카드")]
        public string[] presentClueIds = new string[0];
        public GameEffect effect = new GameEffect();
        public string nextId;
    }
}
