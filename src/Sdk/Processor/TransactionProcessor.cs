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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Sawtooth.Sdk.Messaging;
using static Message.Types;

namespace Sawtooth.Sdk.Processor
{
    /// <summary>
    /// Transaction processor.
    /// </summary>
    public class TransactionProcessor : StreamListenerBase
    {
        readonly List<ITransactionHandler> Handlers;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Sawtooth.Sdk.Processor.TransactionProcessor"/> class.
        /// </summary>
        /// <param name="address">Address.</param>
        public TransactionProcessor(string address) : base(address)
        {
            Handlers = new List<ITransactionHandler>();
        }

        /// <summary>
        /// Adds a new transaction handler to this process. This method must be called before calling <see cref="Start()"/>
        /// </summary>
        /// <param name="handler">Handler.</param>
        public void AddHandler(ITransactionHandler handler) => Handlers.Add(handler);

        /// <summary>
        /// Starts the processor and connects to the message stream
        /// </summary>
        public async void Start()
        {
            Connect();

            foreach (var handler in Handlers)
            {
                var request = new TpRegisterRequest { Version = handler.Version, Family = handler.FamilyName };
                request.Namespaces.AddRange(handler.Namespaces);

                var response = await SendAsync(request.Wrap(MessageType.TpRegisterRequest), CancellationToken.None);
                Console.WriteLine($"Transaction processor registration: {response.Unwrap<TpRegisterResponse>().Status}");
            }
        }

        /// <summary>
        /// Stops the processor and sends <see cref="TpUnregisterRequest"/> message to the validator
        /// </summary>
        public void Stop()
        {
            Task.WaitAll(new[] { SendAsync(new TpUnregisterRequest().Wrap(MessageType.TpUnregisterRequest), CancellationToken.None) });
            Disconnect();
        }

        /// <summary>
        /// Processes the received message from the validator
        /// </summary>
        /// <remarks>
        /// Do not call this method directly
        /// </remarks>
        /// <param name="message">Message.</param>
        public override async void OnMessage(Message message)
        {
            base.OnMessage(message);

            if (message.MessageType == MessageType.TpProcessRequest)
            {
                var request = message.Unwrap<TpProcessRequest>();
                var handler = Handlers.FirstOrDefault(x => x.FamilyName == request.Header.FamilyName && x.Version == request.Header.FamilyVersion);

                if (handler == null)
                {
                    // This shouldn't ever happen, but if it does, fail gracefully with internal error
                    await ApplyCompletion(Task.FromException(new Exception("Cannot locate handler.")), message);
                    return;
                }

                await handler
                    .ApplyAsync(request, new TransactionContext(this, request.ContextId))
                    .ContinueWith(ApplyCompletion, message);
            }
        }

        /// <summary>
        /// Completes the <see cref="TpProcessRequest"/> message by sending the resulting status code.
        /// </summary>
        /// <returns>The completion.</returns>
        /// <param name="task">Task.</param>
        /// <param name="msg">Message.</param>
        async Task ApplyCompletion(Task task, object msg)
        {
            var message = (Message)msg;

            switch (task.Status)
            {
                case TaskStatus.RanToCompletion:
                    await SendAsync(new TpProcessResponse { Status = TpProcessResponse.Types.Status.Ok }
                         .Wrap(message, MessageType.TpProcessResponse), CancellationToken.None);
                    break;

                case TaskStatus.Faulted:
                    var errorData = ByteString.CopyFrom(task.Exception?.ToString() ?? string.Empty, Encoding.UTF8);
                    if (task.Exception != null && task.Exception.InnerException is InvalidTransactionException)
                    {
                        await SendAsync(new TpProcessResponse { Status = TpProcessResponse.Types.Status.InvalidTransaction }
                             .Wrap(message, MessageType.TpProcessResponse), CancellationToken.None);
                    }
                    else
                    {
                        await SendAsync(new TpProcessResponse { Status = TpProcessResponse.Types.Status.InternalError }
                             .Wrap(message, MessageType.TpProcessResponse), CancellationToken.None);
                    }
                    break;
            }
        }
    }
}