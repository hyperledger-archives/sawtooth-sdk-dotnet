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

namespace Sawtooth.Sdk.Client
{
    /// <summary>
    /// Encoder settings.
    /// </summary>
    public class EncoderSettings
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:Sawtooth.Sdk.Client.EncoderSettings"/> class.
        /// </summary>
        public EncoderSettings()
        {
            Inputs = new List<string>();
            Outputs = new List<string>();
        }
        /// <summary>
        /// Gets or sets the name of the family.
        /// </summary>
        /// <value>The name of the family.</value>
        public string FamilyName { get; set; }
        /// <summary>
        /// Gets or sets the family version.
        /// </summary>
        /// <value>The family version.</value>
        public string FamilyVersion { get; set; }
        /// <summary>
        /// Gets or sets the inputs.
        /// </summary>
        /// <value>The inputs.</value>
        public List<string> Inputs { get; set; }
        /// <summary>
        /// Gets or sets the outputs.
        /// </summary>
        /// <value>The outputs.</value>
        public List<string> Outputs { get; set; }
        /// <summary>
        /// Gets or sets the signer publickey.
        /// </summary>
        /// <value>The signer publickey.</value>
        public string SignerPublickey { get; set; }
        /// <summary>
        /// Gets or sets the batcher public key.
        /// </summary>
        /// <value>The batcher public key.</value>
        public string BatcherPublicKey { get; set; }
    }
}