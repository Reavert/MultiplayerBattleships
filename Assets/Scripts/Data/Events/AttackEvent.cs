using Data.Structures;

namespace Data.Events
{
    public class AttackEvent
    {
        public int AttackerId;
        public int VictimId;
        public Position AttackPosition;
        public bool Hit;
    }
}