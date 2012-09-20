using System;
using FluentAssertions;
using Xunit;

namespace ServerEventsTest
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

    public class MessageSender:ISender
    {
        public event EventHandler<MsgEventArgs> MessageAvailable;

        public void SendMessage(string message)
        {
            if (MessageAvailable != null)
                MessageAvailable(this, new MsgEventArgs {Message = message});
            Console.WriteLine("Sent: {0}", message);
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
}
