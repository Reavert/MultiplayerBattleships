using Data.Structures;

namespace Data.Messages
{
    public class PlayerCreationRequest : BaseRequest
    {
        public Position[] ShipsPositions;
    }
}