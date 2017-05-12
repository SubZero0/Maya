using System.Threading.Tasks;

namespace Maya.Interfaces
{
    public interface IGuildHandler : IHandler
    {
        Task InitializeAsync();
    }
}
