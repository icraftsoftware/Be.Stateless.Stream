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

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace Be.Stateless.IO
{
	public class BufferControllerFixture
	{
		[Fact]
		public void AppendArrayThatDoesNotExceedAvailability()
		{
			var buffer = new byte[3];
			var controller = new BufferController(buffer, 0, buffer.Length);

			controller.Append(new byte[] { 1, 2, 3 }, 1, 2).Should().BeEmpty();
			controller.Availability.Should().Be(1);
			controller.Count.Should().Be(2);

			buffer.Should().BeEquivalentTo(new byte[] { 2, 3, 0 });
		}

		[Fact]
		public void AppendArrayThatExceedsAvailability()
		{
			var buffer = new byte[3];
			var controller = new BufferController(buffer, 0, buffer.Length);

			controller.Append(new byte[] { 1, 2, 3, 4, 5, 6, 7 }, 2, 5).Should().BeEquivalentTo(new byte[] { 6, 7 });
			controller.Availability.Should().Be(0);
			controller.Count.Should().Be(3);

			buffer.Should().BeEquivalentTo(new byte[] { 3, 4, 5 });
		}

		[Fact]
		public void AppendArrayThrows()
		{
			var controller = new BufferController(new byte[3], 0, 3);

			Action act = () => controller.Append(null, 0, 0);
			act.Should().Throw<ArgumentNullException>().WithMessage("Value cannot be null.\r\nParameter name: bytes");

			act = () => controller.Append(new byte[0], -1, 0);
			act.Should().Throw<ArgumentException>().WithMessage("Cannot be negative.\r\nParameter name: offset");

			act = () => controller.Append(new byte[0], 1, -1);
			act.Should().Throw<ArgumentException>().WithMessage("Cannot be negative.\r\nParameter name: count");

			act = () => controller.Append(new byte[0], 1, 0);
			act.Should().Throw<ArgumentException>().WithMessage("The sum of offset and count is greater than the byte array length.");

			act = () => controller.Append(new byte[2], 0, 3);
			act.Should().Throw<ArgumentException>().WithMessage("The sum of offset and count is greater than the byte array length.");

			act = () => controller.Append(new byte[0], 0, 0);
			act.Should().NotThrow();
		}

		[Fact]
		public void AppendArrayWhenCountIsZero()
		{
			var controller = new BufferController(new byte[3], 0, 3);
			controller.Append(new byte[0], 0, 0).Should().BeNull();
			controller.Availability.Should().Be(3);
			controller.Count.Should().Be(0);
		}

		[Fact]
		public void AppendArrayWhenNoAvailability()
		{
			var controller = new BufferController(new byte[3], 3, 0);
			controller.Append(new byte[] { 1, 2, 3, 4, 5, 6, 7 }, 2, 5).Should().BeEquivalentTo(new byte[] { 3, 4, 5, 6, 7 });
			controller.Availability.Should().Be(0);
			controller.Count.Should().Be(0);
		}

		[Fact]
		public void AppendBufferListThatDoNoExceedAvailability()
		{
			var buffer = new byte[10];
			var controller = new BufferController(buffer, 0, buffer.Length);
			var buffers = new[] {
				new byte[] { 1, 2, 3 },
				new byte[] { 4, 5, 6, 7 },
				new byte[] { 8, 9 }
			};

			controller.Append(buffers).Should().BeEmpty();
			buffer.Should().BeEquivalentTo(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 });
			controller.Availability.Should().Be(1);
			controller.Count.Should().Be(9);
		}

		[Fact]
		public void AppendBufferListThatExceedAvailability()
		{
			var buffer = new byte[9];
			var controller = new BufferController(buffer, 0, buffer.Length);
			var buffers = new[] {
				new byte[] { 1, 2, 3 },
				new byte[] { 4, 5, 6, 7 },
				new byte[] { 8, 9, 8 },
				new byte[] { 7, 6, 5 }
			};

			controller.Append(buffers).Should().BeEquivalentTo(new byte[] { 8 }, new byte[] { 7, 6, 5 });
			buffer.Should().BeEquivalentTo(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 });
			controller.Availability.Should().Be(0);
			controller.Count.Should().Be(9);
		}

		[Fact]
		public void AppendBufferListThatIsEmpty()
		{
			var controller = new BufferController(new byte[3], 0, 3);
			controller.Append(Enumerable.Empty<byte[]>()).Should().BeEmpty();
			controller.Availability.Should().Be(3);
			controller.Count.Should().Be(0);
		}

		[Fact]
		public void AppendBufferListThrows()
		{
			var controller = new BufferController(new byte[3], 0, 3);
			Action act = () => controller.Append((IEnumerable<byte[]>) null);
			act.Should().Throw<ArgumentNullException>().WithMessage("Value cannot be null.\r\nParameter name: buffers");
		}

		[Fact]
		[SuppressMessage("ReSharper", "CoVariantArrayConversion")]
		public void AppendBufferListWhenNoAvailability()
		{
			var controller = new BufferController(new byte[3], 3, 0);
			var buffers = new[] {
				new byte[] { 1, 2, 3 },
				new byte[] { 4, 5, 6, 7 },
				new byte[] { 8, 9, 8 }
			};
			controller.Append(buffers).Should().BeEquivalentTo(buffers);
			controller.Availability.Should().Be(0);
			controller.Count.Should().Be(0);
		}

		[Fact]
		public void AppendEmptyArray()
		{
			var controller = new BufferController(new byte[3], 0, 3);

			controller.Append(new byte[] { }).Should().BeEquivalentTo(new byte[] { });
			controller.Availability.Should().Be(3);
			controller.Count.Should().Be(0);
		}

		[Fact]
		public void AppendLessThanAvailable()
		{
			var controller = new BufferController(new byte[3], 0, 3);
			controller.Append(new byte[] { 1, 2, 3 }).Should().BeNull();
			controller.Availability.Should().Be(0);
			controller.Count.Should().Be(3);
		}

		[Fact]
		public void AppendMoreThanAvailable()
		{
			var controller = new BufferController(new byte[3], 0, 3);
			controller.Append(new byte[] { 1, 2, 3, 4, 5 }).Should().BeEquivalentTo(new byte[] { 4, 5 });
			controller.Availability.Should().Be(0);
			controller.Count.Should().Be(3);
		}

		[Fact]
		public void AppendNullArray()
		{
			var controller = new BufferController(new byte[3], 0, 3);
			controller.Append((byte[]) null).Should().BeNull();
			controller.Availability.Should().Be(3);
			controller.Count.Should().Be(0);
		}

		[Fact]
		public void AppendWhenNoAvailability()
		{
			var controller = new BufferController(new byte[] { 1, 2, 3 }, 3, 0);
			controller.Append(new byte[] { 4, 5 }).Should().BeEquivalentTo(new byte[] { 4, 5 });
			controller.Availability.Should().Be(0);
			controller.Count.Should().Be(0);
		}

		[Fact]
		public void AppendWithReadDelegate()
		{
			var buffer = new byte[10];
			var controller = new BufferController(buffer, 0, buffer.Length);
			controller.Append(new byte[] { 0, 1 });

			controller.Append(Read);

			controller.Availability.Should().Be(2);
			controller.Count.Should().Be(8);

			buffer.Should().BeEquivalentTo(new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 0, 0 });
		}

		[Fact]
		public void AppendWithReadDelegateThrowsIfNoAvailability()
		{
			var buffer = new byte[0];
			var controller = new BufferController(buffer, 0, buffer.Length);
			Action act = () => controller.Append(Read);
			act.Should().Throw<InvalidOperationException>().WithMessage($"{typeof(BufferController).Name} has no more availability to append further bytes to buffer.");
		}

		private int Read(byte[] buffer, int offset, int count)
		{
			var bytes = new byte[] { 2, 3, 4, 5, 6, 7 };
			count = Math.Min(bytes.Length, count);
			Buffer.BlockCopy(bytes, 0, buffer, offset, count);
			return count;
		}
	}
}
