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
using System.Security.Cryptography;
using System.Text;
using Google.Protobuf;
using static Message.Types;

namespace Sawtooth.Sdk
{
    /// <summary>
    /// Extensions.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Wrap the specified message, requestMessage and messageType.
        /// </summary>
        /// <returns>The wrap.</returns>
        /// <param name="message">Message.</param>
        /// <param name="requestMessage">Request message.</param>
        /// <param name="messageType">Message type.</param>
        public static Message Wrap(this IMessage message, Message requestMessage, MessageType messageType)
        {
            return new Message
            {
                MessageType = messageType,
                CorrelationId = requestMessage.CorrelationId,
                Content = message.ToByteString()
            };
        }

        /// <summary>
        /// Wrap the specified message and messageType.
        /// </summary>
        /// <returns>The wrap.</returns>
        /// <param name="message">Message.</param>
        /// <param name="messageType">Message type.</param>
        public static Message Wrap(this IMessage message, MessageType messageType)
        {
            return new Message
            {
                MessageType = messageType,
                CorrelationId = Guid.NewGuid().ToByteArray().ToSha256().ToHexString(),
                Content = message.ToByteString()
            };
        }

        /// <summary>
        /// Unwrap the specified message.
        /// </summary>
        /// <returns>The unwrap.</returns>
        /// <param name="message">Message.</param>
        /// <typeparam name="T">The 1st type parameter.</typeparam>
        public static T Unwrap<T>(this Message message)
            where T : IMessage, new()
        {
            var request = new T();
            request.MergeFrom(message.Content);
            return request;
        }

        /// <summary>
        /// Converts a byte arrat to hex encoded string
        /// </summary>
        /// <returns>The hex string.</returns>
        /// <param name="data">Data.</param>
        public static string ToHexString(this byte[] data) => String.Concat(data.Select(x => x.ToString("x2")));

        /// <summary>
        /// Hashes the specified byte array using Sha256
        /// </summary>
        /// <returns>The sha256.</returns>
        /// <param name="data">Data.</param>
        public static byte[] ToSha256(this byte[] data) => SHA256.Create().ComputeHash(data);

        /// <summary>
        /// Hashes the specified byte array using Sha512
        /// </summary>
        /// <returns>The sha512.</returns>
        /// <param name="data">Data.</param>
        public static byte[] ToSha512(this byte[] data) => SHA512.Create().ComputeHash(data);

        /// <summary>
        /// Converts a string to byte array using UTF8 encoding
        /// </summary>
        /// <returns>The byte array.</returns>
        /// <param name="data">Data.</param>
        public static byte[] ToByteArray(this string data) => Encoding.UTF8.GetBytes(data);
    }
}
