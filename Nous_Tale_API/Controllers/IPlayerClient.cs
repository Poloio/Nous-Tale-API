using System.Threading.Tasks;

namespace Nous_Tale_API.Controllers
{
    /// <summary>
    /// Defines the methods in the client available to be called.
    /// </summary>
    public interface IPlayerClient
    {
        Task EnterRoom(int RoomID);
    }
}
