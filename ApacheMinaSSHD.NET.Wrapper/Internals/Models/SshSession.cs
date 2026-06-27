// Copyright (c) 2026 SERALYNX LLC and ApacheMinaSSHD.NET contributors.
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Runtime.CompilerServices;

namespace ApacheMinaSSHD.NET.Wrapper.Internals.Models
{
    internal class SshSession : Abstractions.Models.ISshSession
    {
        private static readonly ConditionalWeakTable<object, object> SessionIdMap = new();
        private static readonly ConditionalWeakTable<object, string> NegCipherMap = new();

        private readonly org.apache.sshd.server.session.ServerSession? _javaSession;
        private string? _keyAlgorithmId;
        private string? _keyAlgorithm;
        private int _keySize;
        private string? _clientHostKeyAlgorithm;
        private byte[]? _clientHostKeyData;
        private IReadOnlyList<byte[]>? _clientCertificates;
        private string? _clientUsername;

        public SshSession(org.apache.sshd.server.session.ServerSession javaSession)
        {
            _javaSession = javaSession;
            SessionId = (Guid)SessionIdMap.GetValue(javaSession, _ => (object)Guid.NewGuid());
        }

        public SshSession()
        {
            _javaSession = null;
            SessionId = Guid.NewGuid();
        }

        internal void SetKeyInfo(string? algorithmId, string? algorithm, int keySize)
        {
            _keyAlgorithmId = algorithmId;
            _keyAlgorithm = algorithm;
            _keySize = keySize;
        }

        internal void SetHostKeyInfo(string? algorithmId, byte[]? keyData, IReadOnlyList<byte[]>? certificates, string? clientUsername)
        {
            _clientHostKeyAlgorithm = algorithmId;
            _clientHostKeyData = keyData;
            _clientCertificates = certificates;
            _clientUsername = clientUsername;
        }

        public string RemoteAddress
        {
            get
            {
                try
                {
                    if (_javaSession?.getIoSession()?.getRemoteAddress() != null)
                        return _javaSession.getIoSession().getRemoteAddress().toString();
                }
                catch
                {
                }
                return "unknown";
            }
        }

        public Guid SessionId { get; }

        public string? SessionCipher
        {
            get
            {
                try
                {
                    if (_javaSession != null)
                    {
                        if (NegCipherMap.TryGetValue(_javaSession, out var cached))
                            return cached;

                        var props = _javaSession.getProperties();
                        if (props != null)
                        {
                            var entrySet = props.entrySet();
                            var iter = entrySet.iterator();
                            while (iter.hasNext())
                            {
                                var entry = (java.util.Map.Entry)iter.next();
                                var key = entry.getKey()?.ToString();
                                if (key != null && (key.Contains("ipher", StringComparison.OrdinalIgnoreCase) || key.Equals("C2SENC", StringComparison.OrdinalIgnoreCase) || key.Equals("S2CENC", StringComparison.OrdinalIgnoreCase)))
                                {
                                    var val = entry.getValue()?.ToString();
                                    if (!string.IsNullOrEmpty(val))
                                    {
                                        NegCipherMap.TryAdd(_javaSession, val);
                                        return val;
                                    }
                                }
                            }
                        }
                    }
                }
                catch
                {
                }
                return null;
            }
        }

        internal void SetNegotiatedCipher(string? cipher)
        {
            if (!string.IsNullOrEmpty(cipher) && _javaSession != null)
                NegCipherMap.TryAdd(_javaSession, cipher);
        }

        public string? ClientSoftwareIdentifier
        {
            get
            {
                try
                {
                    if (_javaSession != null)
                    {
                        var version = ((org.apache.sshd.common.session.Session)_javaSession).getClientVersion();
                        if (version != null)
                            return version;
                    }
                }
                catch
                {
                }
                return null;
            }
        }

        public string? KeyAlgorithmId => _keyAlgorithmId;
        public string? KeyAlgorithm => _keyAlgorithm;
        public int KeySize => _keySize;
        public string? ClientHostKeyAlgorithm => _clientHostKeyAlgorithm;
        public byte[]? ClientHostKeyData => _clientHostKeyData;
        public IReadOnlyList<byte[]>? ClientCertificates => _clientCertificates;
        public string? ClientUsername => _clientUsername;

        public void Disconnect()
        {
            try
            {
                var ioSession = _javaSession?.getIoSession();
                if (ioSession != null)
                    ioSession.close(true);
                else
                    _javaSession?.close();
            }
            catch
            {
            }
        }
    }
}
