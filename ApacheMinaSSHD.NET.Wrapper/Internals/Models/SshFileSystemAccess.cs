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

using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;

namespace ApacheMinaSSHD.NET.Wrapper.Internals.Models
{
    internal class SshFileSystemAccess : ISshFileSystemAccess
    {
        public SshFileSystemOperation Operation { get; init; }
        public ISshSession? Session { get; init; }
        public string? RootPath { get; init; }
        public string? RemotePath { get; init; }
        public string? LocalPath { get; init; }
        public string? SourcePath { get; init; }
        public string? DestinationPath { get; init; }
        public string? RemoteHandle { get; init; }
        public string? RemoteName { get; init; }
        public string? Extension { get; init; }
        public string? FileAttributeView { get; init; }
        public string? FileAttributeName { get; init; }
        public string? Owner { get; init; }
        public string? Group { get; init; }
        public object? Value { get; init; }
        public bool IsDirectory { get; init; }
        public bool IsSymbolicLink { get; init; }
        public bool ShortName { get; init; }
        public bool FollowLinks { get; init; }
        public bool SharedLock { get; init; }
        public int Command { get; init; }
        public long Offset { get; init; }
        public long Length { get; init; }
        public IReadOnlyList<string> Options { get; init; } = Array.Empty<string>();
        public IReadOnlyDictionary<string, object> Attributes { get; init; } =
            new Dictionary<string, object>();
    }
}
