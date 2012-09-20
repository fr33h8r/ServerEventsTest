using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NSubstitute;
using StringEncryptor;
using Xunit;

namespace ServerEventsTest
{
    public class Server : ISender, IReceiver
    {
        private readonly IEncryptor encryptor;

        public Server(IEncryptor encryptor)
        {
            this.encryptor = encryptor;
        }

        public event EventHandler<MsgEventArgs> MessageAvailable;
        
        public void SendMessage(string message)
        {
            if (MessageAvailable != null)
                MessageAvailable(this, new MsgEventArgs { Message = message });
        }

        public void Subscribe(ISender sender)
        {
            sender.MessageAvailable += (o, args) => SendMessage(encryptor.Convert(args.Message));
        }
    }

    public class ServerTests
    {
        [Fact]
        public void server_should_redirect_message()
        {
            const string initialMessage = "Hello world";
            const string encryptedMessage = "1_2";
            
            var sender = Substitute.For<ISender>();

            var enc = Substitute.For<IEncryptor>();
            enc.Convert(initialMessage).Returns(encryptedMessage);
            Console.Out.WriteLine("enc = {0}", enc);
            
            var server = new Server(enc);
            string messageSentToReceiver = null;
            server.MessageAvailable += (o, args) => messageSentToReceiver = args.Message;

            server.Subscribe(sender);
            sender.MessageAvailable += Raise.EventWith(new MsgEventArgs { Message = initialMessage });

            messageSentToReceiver.Should().Be(encryptedMessage);
        }
    }

    public class EventsIntegrationTests
    {
        [Fact]
        public void should_check_all_structure()
        {
            //arrange
            var sender = new MessageSender();
            var server = new Server(new Encryptor());
            var recList = new List<MessageReceiver>();
            recList.AddRange(new[] {new MessageReceiver(), new MessageReceiver(), new MessageReceiver()});
            recList.ForEach(x=>x.Subscribe(server));
            //act
            server.Subscribe(sender);
            sender.SendMessage("ku");
            //assert

            recList.ForEach(x => Console.Out.WriteLine("Received: {0}", x.ReceivedMessage));
        }
    }
}
