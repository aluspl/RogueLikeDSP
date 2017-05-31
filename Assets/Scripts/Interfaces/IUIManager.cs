namespace LifeLike.Inferfaces
{
    public interface IUIManager :IManager
    {
        bool Enabled { get; set; } 

        void ClearLog();
        void AddLog(string result);
    }
}