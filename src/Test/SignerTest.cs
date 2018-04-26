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
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Org.BouncyCastle.OpenSsl;
using Sawtooth.Sdk.Client;
using Xunit;

namespace Sawtooth.Sdk.Test
{
    public class SignerTest
    {
        [Fact]
        public void SignAndVerifyData()
        {
            var signer = new Signer();
            var message = "Sample message".ToByteArray();

            var verify = Signer.Verify(message, signer.Sign(message), signer.GetPublicKey());
            Assert.True(verify);
        }

        [Fact]
        public void SignAndFailVerify()
        {
            var signer = new Signer();
            var message = "Sample message".ToByteArray();

            var anotherSigner = new Signer();

            var verify = Signer.Verify(message, signer.Sign(message), anotherSigner.GetPublicKey());
            Assert.False(verify);
        }

        [Fact]
        public void CreateSignerFromPem()
        {
            var fileStream = File.OpenRead("Resources/mykey.pem");
            var signer = new Signer(fileStream, null);

            var pubKey = signer.GetPublicKey();
            var privKey = signer.GetPrivateKey();

            Assert.Equal(65, pubKey.Length);
        }

        [Fact]
        public void CreateSigner_FromPem_PasswordProtected()
        {
            var fileStream = System.IO.File.OpenRead("Resources/mykey_protected.pem");
            var signer = new Signer(fileStream, new PasswordFinder("supersecret"));

            var pubKey = signer.GetPublicKey();
            var privKey = signer.GetPrivateKey();

            Assert.Equal(65, pubKey.Length);
        }

        // Helper class to pass password for the signer
        class PasswordFinder : IPasswordFinder
        {
            readonly string password;

            internal PasswordFinder(string password)
            {
                this.password = password;
            }

            public char[] GetPassword() => password.ToCharArray();
        }
    }
}
