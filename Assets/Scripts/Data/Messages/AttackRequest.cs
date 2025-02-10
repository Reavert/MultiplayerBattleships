using Data.Structures;

namespace Data.Messages
{
    public class AttackRequest : PlayerRequest
    {
        public int TargetId;
        public Position Position;
    }
}