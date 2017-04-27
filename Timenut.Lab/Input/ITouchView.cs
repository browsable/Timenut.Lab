namespace Timenut.Lab.Input
{
    public interface ITouchView
    {
        void OnDown(float x, float y);
        void OnUp(float x, float y);
        void OnMove(float x, float y);
    }
}
