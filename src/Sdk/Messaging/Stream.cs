/*
 * Copyright 2017 Intel Corporation
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 * -----------------------------------------------------------------------------
 */
using System;
using Google.Protobuf;
using NetMQ;
using NetMQ.Sockets;
using static Message.Types;
using System.Linq;

namespace Sawtooth.Sdk.Messaging
{
    /// <summary>
    /// Stream.
    /// </summary>
    public class Stream
    {
        readonly string Address;

        readonly NetMQSocket Socket;
        readonly NetMQPoller Poller;

        readonly IStreamListener Listener;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Sawtooth.Sdk.Messaging.Stream"/> class.
        /// </summary>
        /// <param name="address">Address.</param>
        /// <param name="listener">Listener.</param>
        public Stream(string address, IStreamListener listener = null)
        {
            Address = address;

            Socket = new DealerSocket();
            Socket.ReceiveReady += Receive;
            Socket.Options.ReconnectInterval = TimeSpan.FromSeconds(2);

            Poller = new NetMQPoller();
            Poller.Add(Socket);

            Listener = listener;
        }

        void Receive(object _, NetMQSocketEventArgs e)
        {
            var message = new Message();
            message.MergeFrom(Socket.ReceiveMultipartBytes().SelectMany(x => x).ToArray());

            if (message.MessageType == MessageType.PingRequest)
            {
                Socket.SendFrame(new PingResponse().Wrap(message, MessageType.PingResponse).ToByteArray());
                return;
            }

            Listener?.OnMessage(message);
        }

        /// <summary>
        /// Send the specified message.
        /// </summary>
        /// <returns>The send.</returns>
        /// <param name="message">Message.</param>
        public void Send(Message message) => Socket.SendFrame(message.ToByteString().ToByteArray());

        /// <summary>
        /// Connects to the validator
        /// </summary>
        public void Connect()
        {
            Socket.Connect(Address);
            Poller.RunAsync();
        }

        /// <summary>
        /// Disconnects from the validator
        /// </summary>
        public void Disconnect()
        {
            Socket.Disconnect(Address);
            Poller.StopAsync();
        }
    }
}
