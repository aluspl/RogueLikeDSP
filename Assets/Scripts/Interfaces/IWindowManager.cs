using LifeLike.Enums;
using LifeLike.Inferfaces;
namespace LifeLike.Inferfaces{
    public interface IWindowManager : IManager
    {
        WindowState Status { get; set; }

        void Open(WindowType create);
    }
}
