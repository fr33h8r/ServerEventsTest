using System;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace ServerEventsTest
{
    public interface IReceiver
    {
        void Subscribe(ISender sender);
    }

    public class MessageReceiver : IReceiver
    {
        public string ReceivedMessage { get; private set; }

        public void Subscribe(ISender sender)
        {
            sender.MessageAvailable += (o, args) =>
                                           {
                                               ReceivedMessage = args.Message;
                                           };
        }
    }

    public class ReceiverTests
    {
        const string expected = "1_2_3";

        [Fact]
        public void should_receive_message_from_server()
        {
            //arrange
            var server = Substitute.For<ISender>();
            var receiver = new MessageReceiver();

            //act
            receiver.Subscribe(server);
            server.MessageAvailable += Raise.EventWith(new MsgEventArgs{Message = expected});

            //assert
            receiver.ReceivedMessage.Should().Be(expected);
        }
    }
}
