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
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using Sawtooth.Sdk.Messaging;
using static Message.Types;

namespace Sawtooth.Sdk.Processor
{
    /// <summary>
    /// Transaction context.
    /// </summary>
    public class TransactionContext
    {
        readonly IStreamListener Stream;
        readonly string ContextId;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:Sawtooth.Sdk.Processor.TransactionContext"/> class.
        /// </summary>
        /// <param name="stream">Stream.</param>
        /// <param name="contextId">Context identifier.</param>
        public TransactionContext(IStreamListener stream, string contextId)
        {
            Stream = stream;
            ContextId = contextId;
        }

        /// <summary>
        /// Gets the state for the given addresses.
        /// </summary>
        /// <returns>The state async.</returns>
        /// <param name="addresses">Addresses.</param>
        public async Task<Dictionary<string, ByteString>> GetStateAsync(string[] addresses)
        {
            var request = new TpStateGetRequest { ContextId = ContextId };
            request.Addresses.AddRange(addresses);

            var response = await Stream.SendAsync(request.Wrap(MessageType.TpStateGetRequest), CancellationToken.None);
            return response.Unwrap<TpStateGetResponse>()
                           .Entries.ToDictionary(x => x.Address, x => x.Data);
        }

        /// <summary>
        /// Sets the state at the given addresses
        /// </summary>
        /// <returns>The state async.</returns>
        /// <param name="addressValuePairs">Address value pairs.</param>
        public async Task<string[]> SetStateAsync(Dictionary<string, ByteString> addressValuePairs)
        {
            var request = new TpStateSetRequest { ContextId = ContextId };
            request.Entries.AddRange(addressValuePairs.Select(x => new TpStateEntry { Address = x.Key, Data = x.Value }));

            var response = await Stream.SendAsync(request.Wrap(MessageType.TpStateSetRequest), CancellationToken.None);
            return response.Unwrap<TpStateSetResponse>()
                             .Addresses.ToArray();
        }

        /// <summary>
        /// Deletes the state for the given addresses.
        /// </summary>
        /// <returns>The state async.</returns>
        /// <param name="addresses">Addresses.</param>
        public async Task<string[]> DeleteStateAsync(string[] addresses)
        {
            var request = new TpStateDeleteRequest { ContextId = ContextId };
            request.Addresses.AddRange(addresses);

            var response = await Stream.SendAsync(request.Wrap(MessageType.TpStateDeleteRequest), CancellationToken.None);
            return response.Unwrap<TpStateDeleteResponse>()
                             .Addresses.ToArray();
        }

        /// <summary>
        /// Adds custom receipt data for the trasnaction.
        /// </summary>
        /// <returns>The receipt data async.</returns>
        /// <param name="data">Data.</param>
        public async Task<bool> AddReceiptDataAsync(ByteString data)
        {
            var request = new TpReceiptAddDataRequest() { ContextId = ContextId };
            request.Data = data;

            var response = await Stream.SendAsync(request.Wrap(MessageType.TpReceiptAddDataRequest), CancellationToken.None);
            return response.Unwrap<TpReceiptAddDataResponse>()
                             .Status == TpReceiptAddDataResponse.Types.Status.Ok;
        }

        /// <summary>
        /// Adds an event with custom data
        /// </summary>
        /// <returns><code>true</code> if the event request succeeded.</returns>
        /// <param name="name">Name.</param>
        /// <param name="attributes">Attributes.</param>
        /// <param name="data">Data.</param>
        public async Task<bool> AddEventAsync(string name, Dictionary<string, string> attributes, ByteString data)
        {
            var addEvent = new Event { EventType = name, Data = data };
            addEvent.Attributes.AddRange(attributes.Select(x => new Event.Types.Attribute { Key = x.Key, Value = x.Value }));

            var request = new TpEventAddRequest { ContextId = ContextId, Event = addEvent };

            var response = await Stream.SendAsync(request.Wrap(MessageType.TpEventAddRequest), CancellationToken.None);
            return response.Unwrap<TpEventAddResponse>()
                             .Status == TpEventAddResponse.Types.Status.Ok;
        }
    }
}
