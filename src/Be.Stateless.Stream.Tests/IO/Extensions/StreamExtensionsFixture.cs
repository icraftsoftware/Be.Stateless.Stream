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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Text;
using Be.Stateless.Dummies.IO;
using Be.Stateless.Resources;
using FluentAssertions;
using Moq;
using Xunit;

namespace Be.Stateless.IO.Extensions
{
	public class StreamExtensionsFixture
	{
		[Fact]
		public void CanCompressEmptyStream()
		{
			new MemoryStream().CompressToBase64String().Should().NotBeNull();
		}

		[Fact]
		public void CanDecompressEmptyString()
		{
			string.Empty.DecompressFromBase64String().Should().NotBeNull();
		}

		[Fact]
		public void CanDecompressNullString()
		{
			((string) null).DecompressFromBase64String().Should().NotBeNull();
		}

		[Fact]
		public void CompressAboveThreshold()
		{
			using (var stream = TextStreamDummy.Create(1024 * 120))
			{
				stream.TryCompressToBase64String(16, out _).Should().BeFalse();
			}
		}

		[Fact]
		public void CompressBelowThreshold()
		{
			using (var stream = new MemoryStream(Encoding.Unicode.GetBytes(new string('A', 4096))))
			{
				stream.TryCompressToBase64String(1024, out _).Should().BeTrue();
			}
		}

		[Fact]
		public void CompressDecompressRoundtrip()
		{
			using (var inputStream = new MemoryStream())
			using (var writer = new StreamWriter(inputStream))
			{
				const string initial = "This is a string that I would like to find back after roundtrip.";
				writer.Write(initial);
				writer.Flush();

				inputStream.Position = 0;
				var encoded = inputStream.CompressToBase64String();

				string roundTripped;
				using (var decoded = encoded.DecompressFromBase64String())
				using (var reader = new StreamReader(decoded))
				{
					roundTripped = reader.ReadToEnd();
				}

				roundTripped.Should().Be(initial);
			}
		}

		[Fact]
		[SuppressMessage("ReSharper", "ReturnValueOfPureMethodIsNotUsed")]
		public void CompressedOutputIsBase64()
		{
			var input = new MemoryStream(128);
			var buffer = Encoding.UTF8.GetBytes("This is a test string.");
			input.Write(buffer, 0, buffer.Length);

			input.Position = 0;
			var output = input.CompressToBase64String();

			Action act = () => Convert.FromBase64String(output);
			act.Should().NotThrow();
		}

		[Fact]
		public void CompressionStartsAtCurrentPosition()
		{
			using (var inputStream = new MemoryStream())
			using (var writer = new StreamWriter(inputStream))
			{
				const string initial = "This is a string that I would like to find back after roundtrip.";
				writer.Write(initial);
				writer.Flush();

				var position = "This is ".Length;
				inputStream.Position = position;
				var encoded = inputStream.CompressToBase64String();

				string roundTripped;
				using (var decoded = encoded.DecompressFromBase64String())
				using (var reader = new StreamReader(decoded))
				{
					roundTripped = reader.ReadToEnd();
				}

				roundTripped.Should().Be(initial.Substring(position));
			}
		}

		[Fact]
		public void DrainReadToEnd()
		{
			var source = new MemoryStream();
			source.Write(new byte[100], 0, 100);
			source.Position = 0;

			source.Drain();

			source.Position.Should().Be(source.Length);
		}

		[Fact]
		[SuppressMessage("ReSharper", "StringLiteralTypo")]
		public void GetApplicationMimeType()
		{
			var stream = File.OpenRead(Assembly.GetExecutingAssembly().Location);
			stream.GetMimeType().Should().Be("application/x-msdownload");
		}

		[Fact]
		public void GetMimeTypeOfDeflatedStream()
		{
			var res = ResourceManager.Load(Assembly.GetExecutingAssembly(), "Be.Stateless.Resources.Schema.xsd");
			var stream = res
				.CompressToBase64String()
				.DecompressFromBase64String();
			stream.GetMimeType().Should().Be("text/xml");
		}

		[Fact]
		public void GetXmlMimeType()
		{
			var stream = ResourceManager.Load(Assembly.GetExecutingAssembly(), "Be.Stateless.Resources.Schema.xsd");
			stream.GetMimeType().Should().Be("text/xml");
		}

		[Fact]
		public void ThrowsWhenCompressingFromNonReadableInput()
		{
			var input = new Mock<Stream>();
			input.Setup(s => s.CanRead).Returns(false);

			Action act = () => input.Object.CompressToBase64String();

			act.Should().Throw<InvalidOperationException>();
		}

		[Fact]
		public void ThrowsWhenCompressingFromNullInput()
		{
			Action act = () => ((Stream) null).CompressToBase64String();
			act.Should().Throw<ArgumentNullException>();
		}
	}
}
