using System;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace ServerEventsTest1
{
    public interface ISender
    {
        event EventHandler<MsgEventArgs> MessageAvailable;
        void SendMessage(string message);
    }

    public class MsgEventArgs : EventArgs
    {
        public string Message;
    }

    public class MessageSender : ISender
    {
        public event EventHandler<MsgEventArgs> MessageAvailable;

        public void SendMessage(string message)
        {
            if (MessageAvailable != null)
                MessageAvailable(this, new MsgEventArgs { Message = message });
        }
    }
    public class SenderTests
    {
        [Fact]
        public void should_check_if_message_sent()
        {
            //arrange
            var expected = "text";
            var sender = new MessageSender();
            string raisedMessage = null;
            sender.MessageAvailable += (obj, args) => raisedMessage = args.Message;
            //act
            sender.SendMessage(expected);
            //assert
            raisedMessage.Should().Be(expected);
        }
    }

    public class Server : ISender, IReceiver
    {
        public event EventHandler<MsgEventArgs> MessageAvailable;

        public string ReceivedMessage { get; set; }

        public void SendMessage(string message)
        {
            if (MessageAvailable != null)
                MessageAvailable(this, new MsgEventArgs {Message = message});
        }

        public void Receive(ISender sender)
        {
            sender.MessageAvailable += (o, args) =>
                                           {
                                               ReceivedMessage = args.Message;
                                               Console.WriteLine(ReceivedMessage);
                                           };
        }
    }

    public class ServerTests
    {
        private const string expected = "some text";

        [Fact]
        public void should_check_if_message_sent_from_server()
        {
            //arrange
            string raisedMessage = null;
            var server = new Server();
            //act
            server.MessageAvailable += (sender, args) => raisedMessage = args.Message;
            server.SendMessage(expected);
            raisedMessage.Should().Be(expected);
        }

        [Fact]
        public void should_receive_message_from_sender()
        {
            //arrange
            var sender = Substitute.For<ISender>();
            var server = new Server();

            //act
            server.Receive(sender);
            sender.MessageAvailable += Raise.EventWith(new MsgEventArgs {Message = expected});

            //assert
            server.ReceivedMessage.Should().Be(expected);
        }
    }

    public class EventsIntegrationTests
    {
        [Fact]
        public void should_check_all_structure()
        {
            //arrange
            var sender = new MessageSender();
            var server = new Server();
            var rec1 = new MessageReceiver();
            var rec2 = new MessageReceiver();

            //act
            rec1.Receive(server);
            rec2.Receive(server);

            //assert
            sender.SendMessage("ku");
        }
    }

    public interface IReceiver
    {
        string ReceivedMessage { get; set; }
        void Receive(ISender sender);
    }

    public class MessageReceiver : IReceiver
    {
        public string ReceivedMessage { get; set; }

        public void Receive(ISender sender)
        {
            sender.MessageAvailable += (o, args) =>
                                           {
                                               ReceivedMessage = args.Message;
                                               Console.WriteLine(ReceivedMessage);
                                           };
        }
    }

    public class ReceiverTests
    {
        const string expected = "some text";

        [Fact]
        public void should_receive_message_from_server()
        {
            //arrange
            var server = Substitute.For<Server>();
            var receiver = new MessageReceiver();

            //act
            receiver.Receive(server);
            server.MessageAvailable += Raise.EventWith(new MsgEventArgs{Message = expected});

            //assert
            receiver.ReceivedMessage.Should().Be(expected);
        }
    }
}
