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

using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Be.Stateless.IO.Extensions;
using Be.Stateless.Resources;
using FluentAssertions;
using Xunit;

namespace Be.Stateless.IO
{
	public class StringStreamFixture
	{
		[Fact]
		[SuppressMessage("ReSharper", "StringLiteralTypo")]
		public void PlainTextLengthIsByteCountPlusBom()
		{
			const string content = "Hello world! And some @#$%^&*éèöäñ";
			using (var stream = new StringStream(content))
			{
				stream.Length.Should().Be(Encoding.Unicode.GetByteCount(content) + Encoding.Unicode.GetPreamble().Length);
			}
		}

		[Fact]
		[SuppressMessage("ReSharper", "StringLiteralTypo")]
		public void PlainTextRoundTripping()
		{
			const string content = "Hello world! And some @#$%^&*éèöäñ";
			using (var stream = new StringStream(content))
			{
				var reader = new StreamReader(stream);
				reader.ReadToEnd().Should().Be(content);
			}
		}

		[Fact]
		[SuppressMessage("ReSharper", "PossibleNullReferenceException")]
		public void Utf16EmbeddedResourceRoundTripping()
		{
			var xmlContent = ResourceManager.Load(Assembly.GetExecutingAssembly(), "Be.Stateless.Resources.utf-16.xml", s => new StreamReader(s).ReadToEnd());
			using (var stream = new StringStream(CreateXmlDocument(xmlContent).DocumentElement.OuterXml))
			{
				var actual = XDocument.Parse(stream.ReadToEnd());
				XNode.DeepEquals(actual, XDocument.Parse(xmlContent)).Should().BeTrue();
			}
		}

		[Fact]
		[SuppressMessage("ReSharper", "PossibleNullReferenceException")]
		public void Utf16XmlReaderRoundTripping()
		{
			var xmlContent = ResourceManager.Load(Assembly.GetExecutingAssembly(), "Be.Stateless.Resources.utf-16.xml", s => new StreamReader(s).ReadToEnd());
			using (var stream = new StringStream(CreateXmlDocument(xmlContent).DocumentElement.OuterXml))
			using (var xmlReader = XmlReader.Create(stream, new() { CloseInput = true }))
			{
				xmlReader.MoveToContent();
				var actual = XDocument.Parse(xmlReader.ReadOuterXml());
				XNode.DeepEquals(actual, XDocument.Parse(xmlContent)).Should().BeTrue();
			}
		}

		[Fact]
		[SuppressMessage("ReSharper", "PossibleNullReferenceException")]
		public void Utf8EmbeddedResourceRoundTripping()
		{
			var xmlContent = ResourceManager.Load(Assembly.GetExecutingAssembly(), "Be.Stateless.Resources.utf-8.xml", s => new StreamReader(s).ReadToEnd());
			using (var stream = new StringStream(CreateXmlDocument(xmlContent).DocumentElement.OuterXml))
			{
				var actual = XDocument.Parse(stream.ReadToEnd());
				XNode.DeepEquals(actual, XDocument.Parse(xmlContent)).Should().BeTrue();
			}
		}

		[Fact]
		[SuppressMessage("ReSharper", "PossibleNullReferenceException")]
		public void Utf8XmlReaderRoundTripping()
		{
			var xmlContent = ResourceManager.Load(Assembly.GetExecutingAssembly(), "Be.Stateless.Resources.utf-8.xml", s => new StreamReader(s).ReadToEnd());
			using (var stream = new StringStream(CreateXmlDocument(xmlContent).DocumentElement.OuterXml))
			using (var xmlReader = XmlReader.Create(stream, new() { CloseInput = true }))
			{
				xmlReader.MoveToContent();
				var actual = XDocument.Parse(xmlReader.ReadOuterXml());
				XNode.DeepEquals(actual, XDocument.Parse(xmlContent)).Should().BeTrue();
			}
		}

		[Fact]
		public void XmlTextLengthIsByteCountPlusBom()
		{
			const string content = "<root><node>content</node></root>";
			using (var stream = new StringStream(content))
			{
				stream.Length.Should().Be(Encoding.Unicode.GetByteCount(content) + Encoding.Unicode.GetPreamble().Length);
			}
		}

		[Fact]
		public void XmlTextRoundTripping()
		{
			const string content = "<root><node>content</node></root>";
			using (var reader = XmlReader.Create(new StringStream(content), new() { CloseInput = true }))
			{
				reader.MoveToContent();
				reader.ReadOuterXml().Should().Be(content);
			}
		}

		private XmlDocument CreateXmlDocument(string xmlContent)
		{
			var xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(xmlContent);
			return xmlDocument;
		}
	}
}
