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
using System.Runtime.Serialization;

namespace Sawtooth.Sdk.Processor
{
    /// <summary>
    /// Invalid transaction exception.
    /// </summary>
    public class InvalidTransactionException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Sawtooth.Sdk.Processor.InvalidTransactionException"/> class.
        /// </summary>
        /// <param name="message">Message.</param>
        public InvalidTransactionException(string message) : base(message)
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Sawtooth.Sdk.Processor.InvalidTransactionException"/> class.
        /// </summary>
        public InvalidTransactionException() : base("Transaction was invalid")
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Sawtooth.Sdk.Processor.InvalidTransactionException"/> class.
        /// </summary>
        /// <param name="info">Info.</param>
        /// <param name="context">Context.</param>
        protected InvalidTransactionException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Sawtooth.Sdk.Processor.InvalidTransactionException"/> class.
        /// </summary>
        /// <param name="message">Message.</param>
        /// <param name="innerException">Inner exception.</param>
        public InvalidTransactionException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
