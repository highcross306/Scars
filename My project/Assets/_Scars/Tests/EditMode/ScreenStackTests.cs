using System.Collections.Generic;
using NUnit.Framework;
using Scars.Controls;
using Scars.Investigation;

namespace Scars.Tests
{
    // 화면 대신 열림·닫힘만 기록하는 가짜 화면
    public class FakeScreen : IScreen
    {
        public readonly string Name;
        public int OpenCount;
        public int CloseCount;

        public FakeScreen(string name = "screen") { Name = name; }

        public bool CanClose { get; set; } = true;

        public string PauseReason { get; set; }

        public bool IsOpen => OpenCount > CloseCount;

        public void Open() { OpenCount++; }

        public void Close() { CloseCount++; }
    }

    public class ScreenStackTests
    {
        [Test]
        public void New_Empty()
        {
            var stack = new ScreenStack();
            Assert.IsTrue(stack.IsEmpty);
            Assert.IsNull(stack.Top);
            Assert.IsFalse(stack.Back());
        }

        [Test]
        public void Push_OpensAndBecomesTop()
        {
            var stack = new ScreenStack();
            var a = new FakeScreen("a");
            var b = new FakeScreen("b");

            Assert.IsTrue(stack.Push(a));
            Assert.IsTrue(stack.Push(b));

            Assert.AreSame(b, stack.Top);
            Assert.AreEqual(2, stack.Count);
            Assert.IsTrue(a.IsOpen);
            Assert.IsTrue(b.IsOpen);
        }

        [Test]
        public void Push_SameScreenTwice_Ignored()
        {
            var stack = new ScreenStack();
            var a = new FakeScreen();
            stack.Push(a);

            Assert.IsFalse(stack.Push(a));
            Assert.AreEqual(1, stack.Count);
            Assert.AreEqual(1, a.OpenCount);
        }

        [Test]
        public void Push_Null_False()
        {
            Assert.IsFalse(new ScreenStack().Push(null));
        }

        [Test]
        public void Back_ClosesTopFirst()
        {
            var stack = new ScreenStack();
            var a = new FakeScreen("a");
            var b = new FakeScreen("b");
            stack.Push(a);
            stack.Push(b);

            Assert.IsTrue(stack.Back());
            Assert.IsFalse(b.IsOpen);
            Assert.IsTrue(a.IsOpen);
            Assert.AreSame(a, stack.Top);

            Assert.IsTrue(stack.Back());
            Assert.IsFalse(a.IsOpen);
            Assert.IsFalse(stack.Back());
        }

        [Test]
        public void Close_MiddleScreen_KeepsOthers()
        {
            var stack = new ScreenStack();
            var a = new FakeScreen("a");
            var b = new FakeScreen("b");
            stack.Push(a);
            stack.Push(b);

            Assert.IsTrue(stack.Close(a));

            Assert.IsFalse(a.IsOpen);
            Assert.AreSame(b, stack.Top);
            Assert.AreEqual(1, stack.Count);
        }

        [Test]
        public void Close_NotOpen_False()
        {
            var stack = new ScreenStack();
            var a = new FakeScreen();
            Assert.IsFalse(stack.Close(a));
            Assert.AreEqual(0, a.CloseCount);
        }

        [Test]
        public void Toggle_OpensThenCloses()
        {
            var stack = new ScreenStack();
            var a = new FakeScreen();

            Assert.IsTrue(stack.Toggle(a));
            Assert.IsTrue(a.IsOpen);
            Assert.IsTrue(stack.Toggle(a));
            Assert.IsFalse(a.IsOpen);
            Assert.IsTrue(stack.IsEmpty);
        }

        [Test]
        public void Clear_ClosesAllTopFirst()
        {
            var stack = new ScreenStack();
            var a = new FakeScreen("a");
            var b = new FakeScreen("b");
            stack.Push(a);
            stack.Push(b);
            var order = new List<IScreen>();
            stack.Closed += s => order.Add(s);

            stack.Clear();

            Assert.IsTrue(stack.IsEmpty);
            CollectionAssert.AreEqual(new IScreen[] { b, a }, order);
        }

        // ── 닫을 수 없는 화면 (시간 초과 뒤 가설 창) ──

        [Test]
        public void Locked_BackCloseToggle_Refused()
        {
            var stack = new ScreenStack();
            var board = new FakeScreen { CanClose = false };
            stack.Push(board);

            Assert.IsFalse(stack.Back());
            Assert.IsFalse(stack.Close(board));
            Assert.IsFalse(stack.Toggle(board));

            Assert.IsTrue(board.IsOpen);
            Assert.AreSame(board, stack.Top);
        }

        [Test]
        public void Locked_ScreenAboveStillCloses()
        {
            var stack = new ScreenStack();
            var board = new FakeScreen("board") { CanClose = false };
            var menu = new FakeScreen("menu");
            stack.Push(board);
            stack.Push(menu);

            Assert.IsTrue(stack.Back());
            Assert.IsFalse(menu.IsOpen);
            Assert.IsFalse(stack.Back());
            Assert.IsTrue(board.IsOpen);
        }

        [Test]
        public void Locked_ClosesWhenAllowedAgain()
        {
            var stack = new ScreenStack();
            var board = new FakeScreen { CanClose = false };
            stack.Push(board);

            board.CanClose = true;
            Assert.IsTrue(stack.Back());
            Assert.IsFalse(board.IsOpen);
        }

        [Test]
        public void Clear_ClosesLockedToo()
        {
            var stack = new ScreenStack();
            var board = new FakeScreen { CanClose = false };
            stack.Push(board);

            stack.Clear();

            Assert.IsTrue(stack.IsEmpty);
            Assert.IsFalse(board.IsOpen);
        }

        // ── 조사 타이머 멈춤 ──

        static InvestigationTimer Running()
        {
            var timer = new InvestigationTimer(420f);
            timer.Start();
            return timer;
        }

        [Test]
        public void ScreenWithReason_PausesTimer_UntilClosed()
        {
            var timer = Running();
            var stack = new ScreenStack { Timer = timer };
            var journal = new FakeScreen { PauseReason = "journal" };

            stack.Push(journal);
            timer.Tick(10f);
            Assert.AreEqual(420f, timer.Remaining);

            stack.Back();
            timer.Tick(10f);
            Assert.AreEqual(410f, timer.Remaining);
        }

        [Test]
        public void ScreenWithoutReason_TimerKeepsRunning()
        {
            var timer = Running();
            var stack = new ScreenStack { Timer = timer };
            stack.Push(new FakeScreen("zoom"));   // 확대 조사는 시간이 흐른다

            timer.Tick(10f);
            Assert.AreEqual(410f, timer.Remaining);
        }

        [Test]
        public void TwoScreens_ResumesOnlyWhenAllClosed()
        {
            var timer = Running();
            var stack = new ScreenStack { Timer = timer };
            var journal = new FakeScreen("journal") { PauseReason = "journal" };
            var board = new FakeScreen("board") { PauseReason = "hypothesis" };
            stack.Push(journal);
            stack.Push(board);

            stack.Back();
            Assert.IsTrue(timer.IsPaused);
            stack.Back();
            Assert.IsFalse(timer.IsPaused);
        }

        [Test]
        public void SameReasonTwice_ResumesOnlyWhenBothClosed()
        {
            var timer = Running();
            var stack = new ScreenStack { Timer = timer };
            var a = new FakeScreen("clue a") { PauseReason = "clueDetail" };
            var b = new FakeScreen("clue b") { PauseReason = "clueDetail" };
            stack.Push(a);
            stack.Push(b);

            stack.Close(a);
            Assert.IsTrue(timer.IsPaused);
            stack.Close(b);
            Assert.IsFalse(timer.IsPaused);
        }

        [Test]
        public void ScreenReason_DoesNotClashWithDialoguePause()
        {
            var timer = Running();
            var stack = new ScreenStack { Timer = timer };
            timer.Pause("dialogue");
            var screen = new FakeScreen { PauseReason = "dialogue" };

            stack.Push(screen);
            stack.Back();

            Assert.IsTrue(timer.IsPaused);   // 대화 멈춤은 그대로
        }

        [Test]
        public void TimerSetLater_PausesForScreensAlreadyOpen()
        {
            var stack = new ScreenStack();
            stack.Push(new FakeScreen { PauseReason = "journal" });
            var timer = Running();

            stack.Timer = timer;

            Assert.IsTrue(timer.IsPaused);
        }

        [Test]
        public void TimerReplaced_OldResumed_NewPaused()
        {
            var old = Running();
            var stack = new ScreenStack { Timer = old };
            stack.Push(new FakeScreen { PauseReason = "journal" });
            var next = Running();

            stack.Timer = next;

            Assert.IsFalse(old.IsPaused);
            Assert.IsTrue(next.IsPaused);
        }

        [Test]
        public void Clear_ResumesTimer()
        {
            var timer = Running();
            var stack = new ScreenStack { Timer = timer };
            stack.Push(new FakeScreen { PauseReason = "hypothesis", CanClose = false });

            stack.Clear();

            Assert.IsFalse(timer.IsPaused);
        }

        [Test]
        public void Events_OpenedAndClosed()
        {
            var stack = new ScreenStack();
            var a = new FakeScreen();
            IScreen opened = null, closed = null;
            stack.Opened += s => opened = s;
            stack.Closed += s => closed = s;

            stack.Push(a);
            Assert.AreSame(a, opened);
            stack.Back();
            Assert.AreSame(a, closed);
        }
    }

    public class InputCommandsTests
    {
        ScreenStack _stack;
        FakeScreen _case, _character, _menu;
        InputCommands _commands;

        [SetUp]
        public void SetUp()
        {
            _stack = new ScreenStack();
            _case = new FakeScreen("case");
            _character = new FakeScreen("character");
            _menu = new FakeScreen("menu");
            _commands = new InputCommands(_stack) { CaseJournal = _case, CharacterJournal = _character, PauseMenu = _menu };
        }

        [Test]
        public void Tab_TogglesCaseJournal()
        {
            Assert.IsTrue(_commands.ToggleCaseJournal());
            Assert.IsTrue(_case.IsOpen);
            Assert.IsTrue(_commands.ToggleCaseJournal());
            Assert.IsFalse(_case.IsOpen);
        }

        [Test]
        public void C_TogglesCharacterJournal_OverCaseJournal()
        {
            _commands.ToggleCaseJournal();
            Assert.IsTrue(_commands.ToggleCharacterJournal());
            Assert.AreSame(_character, _stack.Top);

            // 아래에 깔린 사건 일지도 Tab으로 바로 닫힌다
            Assert.IsTrue(_commands.ToggleCaseJournal());
            Assert.IsFalse(_case.IsOpen);
            Assert.IsTrue(_character.IsOpen);
        }

        [Test]
        public void RightClick_ClosesTop()
        {
            _commands.ToggleCaseJournal();
            _commands.ToggleCharacterJournal();

            Assert.IsTrue(_commands.Back());
            Assert.IsFalse(_character.IsOpen);
            Assert.IsTrue(_case.IsOpen);
        }

        [Test]
        public void PauseMenuOpen_TabAndCIgnored()
        {
            _commands.TogglePauseMenu();

            Assert.IsTrue(_commands.IsPauseMenuOpen);
            Assert.IsFalse(_commands.ToggleCaseJournal());
            Assert.IsFalse(_commands.ToggleCharacterJournal());
            Assert.IsFalse(_case.IsOpen);
        }

        [Test]
        public void Esc_ClosesMenu_ThenTabWorks()
        {
            _commands.TogglePauseMenu();
            Assert.IsTrue(_commands.TogglePauseMenu());
            Assert.IsFalse(_menu.IsOpen);
            Assert.IsTrue(_commands.ToggleCaseJournal());
        }

        [Test]
        public void MissingScreen_NothingHappens()
        {
            var commands = new InputCommands(_stack);
            Assert.IsFalse(commands.ToggleCaseJournal());
            Assert.IsFalse(commands.TogglePauseMenu());
            Assert.IsTrue(_stack.IsEmpty);
        }
    }
}
