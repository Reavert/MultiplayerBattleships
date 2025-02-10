using System;
using System.Linq;
using Data.Messages;

namespace Server.MessageProcessors
{
    public class FetchEventsProcessor : MessageProcessor
    {
        private readonly EventsHandler m_EventsHandler;
        
        public FetchEventsProcessor(EventsHandler eventsHandler)
        {
            m_EventsHandler = eventsHandler;
        }
        
        public override object Process(object parameter)
        {
            if (parameter is not FetchEventRequest fetchEventRequest)
                return null;

            try
            {
                var fetchedEvents = m_EventsHandler.FetchEvents(fetchEventRequest.PlayerId).ToArray();
                var response = new FetchEventsResponse
                {
                    Success = true,
                    Events = fetchedEvents
                };

                return response;
            }
            catch (Exception e)
            {
                var response = new FetchEventsResponse
                {
                    Success = false,
                    Message = e.Message
                };

                return response;
            }
        }
    }
}