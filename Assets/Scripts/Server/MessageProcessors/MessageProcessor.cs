using System;

namespace Server.MessageProcessors
{
    public abstract class MessageProcessor
    {
        public event Action<object> EventBroadcast;
        
        public abstract object? Process(object parameter);

        protected void BroadcastEvent(object e)
        {
            EventBroadcast?.Invoke(e);
        }
    }
}