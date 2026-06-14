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

using ApacheMinaSSHD.NET.Wrapper.Abstractions;
using ApacheMinaSSHD.NET.Wrapper.Factories;

namespace ApacheMinaSSHD.NET.Wrapper.Tests;

[Trait("Category", "Unit")]
public class SftpSubsystemFactoryTests
{
    [Fact]
    public void Constructor()
    {
        var factory = new AMNetSftpSubsystemFactory();
        Assert.NotNull(factory);
    }

    [Fact]
    public void addSftpEventListener_accepts()
    {
        var factory = new AMNetSftpSubsystemFactory();
        factory.addSftpEventListener(new AMNetSftpEventListener());
    }

    [Fact]
    public void addSftpEventListener_null_throws()
    {
        var factory = new AMNetSftpSubsystemFactory();
        Assert.Throws<ArgumentNullException>(() => factory.addSftpEventListener(null!));
    }

    [Fact]
    public void setFileSystemAccessor_accepts()
    {
        var factory = new AMNetSftpSubsystemFactory();
        factory.setFileSystemAccessor(new AMNetSftpFileSystemAccessor());
    }

    [Fact]
    public void setFileSystemAccessor_null_throws()
    {
        var factory = new AMNetSftpSubsystemFactory();
        Assert.Throws<ArgumentNullException>(() => factory.setFileSystemAccessor(null!));
    }
}
