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
using System.IO;
using Be.Stateless.Extensions;
using Be.Stateless.IO.Extensions;

namespace Be.Stateless.Unit.IO.Extensions
{
	[SuppressMessage("ReSharper", "UnusedMember.Global", Justification = "Public API.")]
	[SuppressMessage("ReSharper", "UnusedType.Global", Justification = "Public API.")]
	public static class StreamExtensions
	{
		/// <summary>
		/// Save the content of a stream to disk by going through a temporary file.
		/// </summary>
		/// <param name="stream">The stream whose content needs to be saved.</param>
		/// <param name="folder">The folder where to save the stream.</param>
		/// <param name="name">The file name to use to save the stream.</param>
		/// <remarks>
		/// <para>
		/// The <paramref name="stream"/> is first saved to a temporary file before being moved to file with the given <paramref name="name"/>, but only after its content
		/// has been completely flushed to disk.
		/// </para>
		/// <para>
		/// The target <paramref name="folder"/> is created if it does not exist.
		/// </para>
		/// </remarks>
		public static void DropToFolder(this Stream stream, string folder, string name)
		{
			if (stream == null) throw new ArgumentNullException(nameof(stream));
			if (folder.IsNullOrEmpty()) throw new ArgumentNullException(nameof(folder));
			if (name.IsNullOrEmpty()) throw new ArgumentNullException(nameof(name));
			Directory.CreateDirectory(folder);
			var path = Path.Combine(folder, name);
			File.Delete(path);
			// save to a temporary file with a GUID and no extension as name
			var tempFileName = Guid.NewGuid().ToString("N");
			var tempPath = Path.Combine(folder, tempFileName);
			stream.Save(tempPath);
			stream.Close();
			File.Move(tempPath, path);
		}
	}
}
