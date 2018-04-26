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
using System.Diagnostics;
using System.Threading.Tasks;
using Google.Protobuf;
using NetMQ;
using NetMQ.Sockets;
using Sawtooth.Sdk.Messaging;
using Xunit;
using static Message.Types;

namespace Sawtooth.Sdk.Test
{
    public class StreamTests
    {
        [Fact]
        public void RespondToPing()
        {
            // Setup
            var serverSocket = new PairSocket();
            serverSocket.Bind("inproc://stream-test");

            var pingMessage = new PingRequest().Wrap(MessageType.PingRequest);

            var stream = new Stream("inproc://stream-test");
            stream.Connect();

            // Run test case
            var task1 = Task.Run(() => serverSocket.SendFrame(pingMessage.ToByteString().ToByteArray()));
            var task2 = Task.Run(() =>
            {
                var message = new Message();
                message.MergeFrom(serverSocket.ReceiveFrameBytes());

                return message;
            });

            Task.WhenAll(new[] { task1, task2 });

            var actualMessage = task2.Result;

            // Verify
            Assert.Equal(MessageType.PingResponse, actualMessage.MessageType);
            Assert.Equal(pingMessage.CorrelationId, actualMessage.CorrelationId);

            serverSocket.Unbind("inproc://stream-test");
            stream.Disconnect();

            try {
                var s = new Stream("tcp://localhost:4523");
                s.Connect();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }
    }
}
