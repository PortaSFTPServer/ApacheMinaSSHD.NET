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

using System.Collections.Concurrent;
using ApacheMinaSSHD.NET.Wrapper.Abstractions;
using ApacheMinaSSHD.NET.Wrapper.Abstractions.Models;

namespace DareSftpServer;

public class DareSftpAccessor : AMNetSftpFileSystemAccessor
{
    private readonly string _encryptedRoot;
    private readonly string _stagingRoot;
    private readonly ConcurrentDictionary<string, FileOperation> _openFiles = new(StringComparer.OrdinalIgnoreCase);
    private long _totalBytesRead;
    private long _totalBytesWritten;

    public DareSftpAccessor(string encryptedRoot, string stagingRoot)
    {
        _encryptedRoot = encryptedRoot;
        _stagingRoot = stagingRoot;
    }

    public long TotalBytesRead => Interlocked.Read(ref _totalBytesRead);
    public long TotalBytesWritten => Interlocked.Read(ref _totalBytesWritten);
    public IReadOnlyDictionary<string, FileOperation> OpenFiles => _openFiles;

    public override string ResolveLocalFilePath(ISshFileSystemAccess context, string resolvedLocalPath)
    {
        if (string.IsNullOrWhiteSpace(context.RemotePath))
            return base.ResolveLocalFilePath(context, resolvedLocalPath);

        var filename = Path.GetFileName(context.RemotePath);
        if (string.IsNullOrWhiteSpace(filename))
            return base.ResolveLocalFilePath(context, resolvedLocalPath);

        var encryptedPath = Path.Combine(_encryptedRoot, filename + ".dare");
        var stagingPath = Path.Combine(_stagingRoot, filename);

        Log($"[DARE] Path map: {context.RemotePath} -> encrypted={encryptedPath}, staging={stagingPath}");
        return base.ResolveLocalFilePath(context, resolvedLocalPath);
    }

    public override void OpenFile(ISshFileSystemAccess context)
    {
        var filename = Path.GetFileName(context.RemotePath) ?? "unknown";
        _openFiles[context.RemoteHandle ?? filename] = new FileOperation
        {
            Filename = filename,
            RemotePath = context.RemotePath,
            LocalPath = context.LocalPath,
            OpenedAt = DateTime.UtcNow,
            FileSize = context.Attributes?.TryGetValue("size", out var size) == true ? Convert.ToInt64(size) : 0
        };
        Log($"[DARE] Open: {filename}");
    }

    public override void CloseFile(ISshFileSystemAccess context)
    {
        var filename = Path.GetFileName(context.RemotePath) ?? "unknown";

        if (context.RemoteHandle != null && _openFiles.TryRemove(context.RemoteHandle, out var op))
        {
            var duration = DateTime.UtcNow - op.OpenedAt;
            Log($"[DARE] Close: {filename} (open for {duration.TotalSeconds:F1}s)");
        }
    }

    public override void RenameFile(ISshFileSystemAccess context)
    {
        Log($"[DARE] Rename: {context.RemotePath} -> {context.DestinationPath}");
    }

    public override void RemoveFile(ISshFileSystemAccess context)
    {
        Log($"[DARE] Remove: {context.RemotePath}");
    }

    public override void CopyFile(ISshFileSystemAccess context)
    {
        Log($"[DARE] Copy: {context.RemotePath} -> {context.DestinationPath}");
    }

    protected override bool IsVisibleByDefault(string? localPath)
    {
        if (string.IsNullOrWhiteSpace(localPath))
            return true;

        var name = Path.GetFileName(localPath);
        if (string.IsNullOrWhiteSpace(name))
            return true;

        if (name.EndsWith(".dare", StringComparison.OrdinalIgnoreCase))
            return false;

        return base.IsVisibleByDefault(localPath);
    }

    internal void TrackRead(long bytes) => Interlocked.Add(ref _totalBytesRead, bytes);
    internal void TrackWrite(long bytes) => Interlocked.Add(ref _totalBytesWritten, bytes);

    internal static void Log(string message) =>
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {message}");

    public record FileOperation
    {
        public required string Filename { get; init; }
        public required string? RemotePath { get; init; }
        public required string? LocalPath { get; init; }
        public required DateTime OpenedAt { get; init; }
        public long FileSize { get; init; }
        public long BytesTransferred { get; set; }
    }
}
