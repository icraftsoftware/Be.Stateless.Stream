#region Copyright & License

// Copyright © 2012 - 2021 François Chabot
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#endregion

using System;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Xunit;

namespace Be.Stateless.Extensions
{
	public class OperatingSystemExtensionsFixture
	{
		[Fact]
		public void TransactionalFileSystemSupported()
		{
			Windows7.SupportTransactionalFileSystem().Should().BeTrue();
		}

		[Fact]
		public void TransactionalFileSystemUnsupported()
		{
			WindowsXP.SupportTransactionalFileSystem().Should().BeFalse();
		}

		[SuppressMessage("ReSharper", "InconsistentNaming")]
		internal static readonly OperatingSystem Windows7 = new(PlatformID.Win32NT, new Version("6.1.7601.65536"));

		[SuppressMessage("ReSharper", "InconsistentNaming")]
		internal static readonly OperatingSystem WindowsXP = new(PlatformID.Win32NT, new Version("5.2.7601.65536"));
	}
}
