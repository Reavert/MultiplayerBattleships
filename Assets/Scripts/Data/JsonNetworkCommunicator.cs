using System.IO;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;

namespace Data
{
    public class JsonNetworkCommunicator
    {
        private readonly JsonSerializerSettings m_JsonSerializerSettings = new()
        {
            TypeNameHandling = TypeNameHandling.Objects
        };
    
        public object? Read(NetworkStream networkStream)
        {
            byte[] buffer = new byte[512];
            int bytesRead;
        
            using MemoryStream ms = new MemoryStream();
            while ((bytesRead = networkStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                ms.Write(buffer, 0, bytesRead);

                if (!networkStream.DataAvailable)
                    break;
            }
        
            string jsonString = Encoding.UTF8.GetString(ms.ToArray());
            return JsonConvert.DeserializeObject(jsonString, m_JsonSerializerSettings);
        }

        public void Write(NetworkStream networkStream, object? value)
        {
            string json = JsonConvert.SerializeObject(value, m_JsonSerializerSettings);
            byte[] bytes = Encoding.UTF8.GetBytes(json);
        
            networkStream.Write(bytes);
        }
    }
}