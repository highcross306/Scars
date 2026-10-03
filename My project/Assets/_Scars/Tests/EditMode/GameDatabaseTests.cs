using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Scars.Data;
using UnityEditor;
using UnityEngine;

namespace Scars.Tests
{
    public class GameDatabaseTests
    {
        readonly List<Object> _created = new List<Object>();

        [TearDown]
        public void TearDown()
        {
            foreach (var obj in _created) Object.DestroyImmediate(obj);
            _created.Clear();
        }

        T Make<T>(string id) where T : ScriptableObject
        {
            var asset = ScriptableObject.CreateInstance<T>();
            var so = new SerializedObject(asset);
            so.FindProperty("id").stringValue = id;
            so.ApplyModifiedPropertiesWithoutUndo();
            _created.Add(asset);
            return asset;
        }

        GameDatabase MakeDb(params DataAsset[] assets)
        {
            var db = ScriptableObject.CreateInstance<GameDatabase>();
            _created.Add(db);
            foreach (var a in assets) db.Register(a);
            return db;
        }

        [Test]
        public void Find_ById_ReturnsMatch()
        {
            var clue = Make<ClueCardData>("a");
            var db = MakeDb(clue);

            Assert.AreSame(clue, db.Find<ClueCardData>("a"));
        }

        [Test]
        public void Find_UnknownOrEmptyId_ReturnsNull()
        {
            var db = MakeDb(Make<ClueCardData>("a"));

            Assert.IsNull(db.Find<ClueCardData>("b"));
            Assert.IsNull(db.Find<ClueCardData>(""));
            Assert.IsNull(db.Find<ClueCardData>(null));
        }

        [Test]
        public void Find_SameIdDifferentType_ReturnsRequestedType()
        {
            var clue = Make<ClueCardData>("a");
            var quiz = Make<QuizData>("a");
            var db = MakeDb(clue, quiz);

            Assert.AreSame(clue, db.Find<ClueCardData>("a"));
            Assert.AreSame(quiz, db.Find<QuizData>("a"));
        }

        [Test]
        public void All_ReturnsOnlyRequestedType()
        {
            var db = MakeDb(Make<ClueCardData>("a"), Make<ClueCardData>("b"), Make<EndingData>("c"));

            Assert.AreEqual(2, db.All<ClueCardData>().Count());
        }

        [Test]
        public void Register_SameAssetTwice_AddsOnce()
        {
            var clue = Make<ClueCardData>("a");
            var db = MakeDb(clue, clue);

            Assert.AreEqual(1, db.Assets.Count);
        }

        [Test]
        public void Validate_CleanData_HasNoProblems()
        {
            var db = MakeDb(Make<ClueCardData>("a"), Make<QuizData>("a"));

            Assert.IsEmpty(db.Validate());
        }

        [Test]
        public void Validate_DuplicateAndMissingIds_AreReported()
        {
            var db = MakeDb(Make<ClueCardData>("a"), Make<ClueCardData>("a"), Make<EndingData>(""));

            Assert.AreEqual(2, db.Validate().Count);
        }
    }
}
