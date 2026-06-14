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

using ApacheMinaSSHD.NET.Wrapper.Logging;

namespace ApacheMinaSSHD.NET.Wrapper.Tests;

[Trait("Category", "Unit")]
public class LogEntryTests
{
    [Fact]
    public void Default_constructor_creates_empty_entry()
    {
        var entry = new LogEntry();
        Assert.Equal(string.Empty, entry.Message);
        Assert.Null(entry.ColorName);
    }

    [Fact]
    public void Can_set_and_get_message()
    {
        var entry = new LogEntry { Message = "Test message" };
        Assert.Equal("Test message", entry.Message);
    }

    [Fact]
    public void Can_set_and_get_color_name()
    {
        var entry = new LogEntry { ColorName = "Red" };
        Assert.Equal("Red", entry.ColorName);
    }

    [Fact]
    public void ColorName_can_be_null()
    {
        var entry = new LogEntry { ColorName = null };
        Assert.Null(entry.ColorName);
    }

    [Fact]
    public void Multiple_entries_independent()
    {
        var entry1 = new LogEntry { Message = "First", ColorName = "Green" };
        var entry2 = new LogEntry { Message = "Second", ColorName = "Red" };

        Assert.Equal("First", entry1.Message);
        Assert.Equal("Green", entry1.ColorName);
        Assert.Equal("Second", entry2.Message);
        Assert.Equal("Red", entry2.ColorName);
    }
}
