namespace Data.Messages
{
    public class GetGameStateResponse : BaseResponse
    {
        public bool IsGameStarted;
        public bool IsGameFinished;
        public int CurrentPlayerId;
        public int WinnerId;
    }
}