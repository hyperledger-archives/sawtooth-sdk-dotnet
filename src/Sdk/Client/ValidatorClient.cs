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
using System.Threading;
using System.Threading.Tasks;
using Sawtooth.Sdk.Messaging;

namespace Sawtooth.Sdk.Client
{
    /// <summary>
    /// Validator client.
    /// </summary>
    public class ValidatorClient : StreamListenerBase, IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Sawtooth.Sdk.Client.ValidatorClient"/> class.
        /// </summary>
        /// <param name="address">Address.</param>
        ValidatorClient(string address) : base(address)
        {
            Connect();
        }

        /// <summary>
        /// Creates a <see cref="ValidatorClient"/> instance and connects to the specified address.
        /// Use this inside a <c>using</c> statement.
        /// </summary>
        /// <returns>The create.</returns>
        /// <param name="address">Address.</param>
        public static ValidatorClient Create(string address)
        {
            return new ValidatorClient(address);
        }

        /// <summary>
        /// Sends a batch list to the validator
        /// </summary>
        /// <returns>The batch list async.</returns>
        /// <param name="batchList">Batch list.</param>
        public async Task<ClientBatchSubmitResponse> SubmitBatchAsync(BatchList batchList)
        {
            var request = new ClientBatchSubmitRequest();
            request.Batches.AddRange(batchList.Batches);

            var response = await SendAsync(request.Wrap(Message.Types.MessageType.ClientBatchSubmitRequest), CancellationToken.None);
            return response.Unwrap<ClientBatchSubmitResponse>();
        }

        /// <summary>
        /// Gets the batch data for the given <c>batchId</c>. />
        /// </summary>
        /// <returns>The batch async.</returns>
        /// <param name="batchId">Batch identifier.</param>
        public async Task<ClientBatchGetResponse> GetBatchAsync(string batchId)
        {
            var response = await SendAsync(new ClientBatchGetRequest() { BatchId = batchId }
                                           .Wrap(Message.Types.MessageType.ClientBatchGetRequest), CancellationToken.None);
            return response.Unwrap<ClientBatchGetResponse>();
        }

        /// <summary>
        /// Gets the state for aa spcific address
        /// </summary>
        /// <returns>The state async.</returns>
        /// <param name="address">Address.</param>
        /// <param name="stateRoot">State root.</param>
        public async Task<ClientStateGetResponse> GetStateAsync(string address, string stateRoot)
        {
            var response = await SendAsync(new ClientStateGetRequest
            {
                Address = address,
                StateRoot = stateRoot
            }.Wrap(Message.Types.MessageType.ClientStateGetRequest), CancellationToken.None);
            return response.Unwrap<ClientStateGetResponse>();
        }

        /// <summary>
        /// Gets a paged list of state data.
        /// </summary>
        /// <returns>The state list async.</returns>
        /// <param name="request">Request.</param>
        public async Task<ClientStateListResponse> GetStateListAsync(ClientStateListRequest request)
        {
            var respoonse = await SendAsync(request.Wrap(Message.Types.MessageType.ClientStateListRequest), CancellationToken.None);
            return respoonse.Unwrap<ClientStateListResponse>();
        }

        /// <summary>
        /// Gets a transaction information from the validator.
        /// </summary>
        /// <returns>The transaction async.</returns>
        /// <param name="trasnactionId">Trasnaction identifier.</param>
        public async Task<ClientTransactionGetResponse> GetTransactionAsync(string trasnactionId)
        {
            var response = await SendAsync(new ClientTransactionGetRequest { TransactionId = trasnactionId }
                                           .Wrap(Message.Types.MessageType.ClientTransactionGetRequest), CancellationToken.None);
            return response.Unwrap<ClientTransactionGetResponse>();
        }

        /// <summary>
        /// Sends a message to the validator.
        /// This method allows sending a message directly to the validator. The message content must be of a type the validator can process.
        /// </summary>
        /// <returns>The message async.</returns>
        /// <param name="message">Message.</param>
        public Task<Message> SendMessageAsync(Message message)
        {
            return SendAsync(message, CancellationToken.None);
        }

        /// <summary>
        /// Releases all resource used by the <see cref="T:Sawtooth.Sdk.Client.ValidatorClient"/> object.
        /// </summary>
        /// <remarks>Call <see cref="Dispose"/> when you are finished using the
        /// <see cref="T:Sawtooth.Sdk.Client.ValidatorClient"/>. The <see cref="Dispose"/> method leaves the
        /// <see cref="T:Sawtooth.Sdk.Client.ValidatorClient"/> in an unusable state. After calling
        /// <see cref="Dispose"/>, you must release all references to the
        /// <see cref="T:Sawtooth.Sdk.Client.ValidatorClient"/> so the garbage collector can reclaim the memory that the
        /// <see cref="T:Sawtooth.Sdk.Client.ValidatorClient"/> was occupying.</remarks>
        public void Dispose()
        {
            Disconnect();
        }
    }
}