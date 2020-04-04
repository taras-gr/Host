using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Host
{
    [ServiceContract]
    public interface IChatClient
    {
        //int Length { get; set; }
        [OperationContract(IsOneWay = false)]
        void RecieveMessage(string user, string message);
    }

    [ServiceContract(CallbackContract = typeof(IChatClient))]
    public interface IChatService
    {
        [OperationContract(IsOneWay = false)]
        bool Join(string userName);
        [OperationContract(IsOneWay = true)]
        void SendMessage(string message);
    }

    [ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Single,
        InstanceContextMode = InstanceContextMode.Single)]
    public class ChatService : IChatService
    {
        Dictionary<IChatClient, string> _users = new Dictionary<IChatClient, string>();

        public bool Join(string userName)
        {
            try
            {
                var connection = OperationContext.Current
                .GetCallbackChannel<IChatClient>();
                _users[connection] = userName;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
            
        }

        public void SendMessage(string message)
        {
            var connection = OperationContext.Current
                .GetCallbackChannel<IChatClient>();
            string user;
            if (!_users.TryGetValue(connection, out user))
                return;
            foreach (var other in _users.Keys)
            {
                if (other == connection)
                    continue;
                other.RecieveMessage(user, message);
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            ServiceHost host = new ServiceHost(typeof(ChatService));
            host.Open();
            Console.WriteLine("Service is ready");
            Console.ReadLine();
            host.Close();
        }
    }
}
