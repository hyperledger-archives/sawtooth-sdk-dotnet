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

namespace Sawtooth.Sdk.Messaging
{
    /// <summary>
    /// Stream listener.
    /// </summary>
    public interface IStreamListener
    {
        /// <summary>
        /// Called when a new message is received from the validator
        /// </summary>
        /// <param name="message">Message.</param>
        void OnMessage(Message message);

        /// <summary>
        /// Send a message to the validator
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="message">Message.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        Task<Message> SendAsync(Message message, CancellationToken cancellationToken);
    }
}
