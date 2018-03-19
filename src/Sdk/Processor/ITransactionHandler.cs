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
using System.Threading.Tasks;
using Sawtooth.Sdk.Processor;

namespace Sawtooth.Sdk.Processor
{
    /// <summary>
    /// Transaction handler.
    /// </summary>
    public interface ITransactionHandler
    {
        /// <summary>
        /// Gets the name of the family.
        /// </summary>
        /// <value>The name of the family.</value>
        string FamilyName { get; }

        /// <summary>
        /// Gets the version.
        /// </summary>
        /// <value>The version.</value>
        string Version { get; }

        /// <summary>
        /// Gets the namespaces.
        /// </summary>
        /// <value>The namespaces.</value>
        string[] Namespaces { get; }

        /// <summary>
        /// Called when the processor recieves <see cref="TpProcessRequest" /> message/>
        /// </summary>
        /// <returns>The async.</returns>
        /// <param name="request">Request.</param>
        /// <param name="context">Context.</param>
        Task ApplyAsync(TpProcessRequest request, TransactionContext context);
    }
}
