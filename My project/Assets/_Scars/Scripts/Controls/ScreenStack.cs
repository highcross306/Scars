namespace Scars.Controls
{
    public interface IScreen
    {
        void Open();
        void Close();
    }

    // 열린 화면(확대 조사, 일지, 팝업)을 쌓아 두고 우클릭 때 맨 위부터 닫는다
    public class ScreenStack
    {
        public IScreen Top => null;

        public void Push(IScreen screen) { }

        // 닫았으면 true
        public bool Back() { return false; }
    }
}
