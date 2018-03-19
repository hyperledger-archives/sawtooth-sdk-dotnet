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
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf;
using NetMQ;
using NetMQ.Sockets;
using Sawtooth.Sdk.Messaging;
using Sawtooth.Sdk.Processor;
using Xunit;
using static Message.Types;

namespace Sawtooth.Sdk.Test
{
    public class ContextTests
    {
        [Fact]
        public async Task CanGetState()
        {
            var serverAddress = "inproc://get-state-test";

            var serverSocket = new PairSocket();
            serverSocket.Bind(serverAddress);

            var processor = new TransactionProcessor(serverAddress);
            processor.Start();

            var context = new TransactionContext(processor, "context");
            var addresses = new[] { "address1", "address2" };

            var task = Task.Run(() =>
            {
                var message = new Message();
                message.MergeFrom(serverSocket.ReceiveFrameBytes());

                Assert.Equal(MessageType.TpStateGetRequest, message.MessageType);

                var response = new TpStateGetResponse();
                response.Entries.AddRange(addresses.Select(x => new TpStateEntry { Address = x, Data = ByteString.Empty }));

                serverSocket.SendFrame(response.Wrap(message, MessageType.TpStateGetResponse).ToByteArray());
            });

            var stateResponse = await context.GetStateAsync(addresses);

            Assert.Equal(addresses.Length, stateResponse.Count());
        }
    }
}
