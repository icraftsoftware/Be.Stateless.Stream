﻿#region Copyright & License

// Copyright © 2012 - 2022 François Chabot
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
using System.Xml;
using System.Xml.Schema;

namespace Be.Stateless.IO.Extensions
{
	// TODO move to Be.Stateless.Xml
	[SuppressMessage("ReSharper", "UnusedType.Global", Justification = "Public API.")]
	public static class TextReaderExtensions
	{
		[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Public API.")]
		public static bool IsXml(this TextReader reader)
		{
			// https://stackoverflow.com/questions/18704586/testing-whether-or-not-something-is-parseable-xml-in-c-sharp
			var settings = new XmlReaderSettings {
				CheckCharacters = true,
				ConformanceLevel = ConformanceLevel.Document,
				DtdProcessing = DtdProcessing.Ignore,
				IgnoreComments = true,
				IgnoreProcessingInstructions = true,
				IgnoreWhitespace = true,
				ValidationFlags = XmlSchemaValidationFlags.None,
				ValidationType = ValidationType.None,
				XmlResolver = null
			};
			var isXml = true;
			using (var xmlReader = XmlReader.Create(reader, settings))
			{
				try
				{
					while (xmlReader.Read()) { }
				}
				catch (XmlException)
				{
					isXml = false;
				}
			}
			return isXml;
		}
	}
}
