using Nous_Tale_API.Model.Entities;
using System.Threading.Tasks;

namespace Nous_Tale_API.Controllers
{
    /// <summary>
    /// Defines the methods in the client available to be called.
    /// </summary>
    public interface IPlayerClient
    {
        // Room methods
        Task EnterRoom(Room room);
        Task ReturnToMenu();
        Task PlayerExited(int playerID);
        Task PlayerEntered(Player playerID);
    }
}
