#region Copyright & License

// Copyright © 2012 - 2020 François Chabot
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

using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Xunit;

namespace Be.Stateless.Extensions
{
	public class ArrayExtensionsFixture
	{
		[Fact]
		public void CommonPath()
		{
			var paths = new[] { "a.b.c.d.e.f", "a.b.c.d.k", "a.b.c" };
			paths.CommonPath(".").Should().Be("a.b.c");
		}

		[Fact]
		public void CommonPathInexistent()
		{
			var paths = new[] { "a.b.c.d.e.f", "a.b.c.d.k", "a.b.c", "x.y.z" };
			paths.CommonPath(".").Should().BeEmpty();
		}

		[Fact]
		public void CommonPathInexistentToo()
		{
			var paths = new[] { "a.b.c.d.e.f", "x.y.z", "m.n.o.p" };
			paths.CommonPath(".").Should().BeEmpty();
		}

		[Fact]
		public void CommonPathOfEmptyArray()
		{
			var paths = new string[0];
			paths.CommonPath(".").Should().BeEmpty();
		}

		[Fact]
		[SuppressMessage("ReSharper", "ExpressionIsAlwaysNull")]
		public void CommonPathOfNullArray()
		{
			string[] paths = null;
			paths.CommonPath(".").Should().BeEmpty();
		}

		[Fact]
		public void CommonPathOfSingletonArray()
		{
			var paths = new[] { "a.b.c.d.e.f" };
			paths.CommonPath(".").Should().Be("a.b.c.d.e.f");
		}

		[Fact]
		[SuppressMessage("ReSharper", "ExpressionIsAlwaysNull")]
		public void RangeOfNullArray()
		{
			byte[] array = null;
			array.Subarray(2).Should().BeNull();
		}

		[Fact]
		public void RangeReturnsTailFromStartIndex()
		{
			var array = new byte[] { 1, 2, 3, 4, 5 };
			array.Subarray(2).Should().BeEquivalentTo(new byte[] { 3, 4, 5 });
		}

		[Fact]
		public void RangeStartIndexIsBelowLowerBound()
		{
			var array = new byte[] { 1, 2, 3 };
			array.Subarray(-7).Should().BeEquivalentTo(array);
		}

		[Fact]
		public void RangeStartIndexIsBeyondUpperBound()
		{
			var array = new byte[] { 1, 2, 3 };
			array.Subarray(9).Should().BeNull();
		}
	}
}
