﻿#region Copyright & License

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
using System.Text;
using Be.Stateless.Extensions;

namespace Be.Stateless.IO
{
	/// <summary>
	/// Stream-derived class meant to minimize the amount of overhead required to wrap a string as a stream.
	/// </summary>
	/// <remarks>
	/// <para>
	/// Notice that a Unicode BOM (see <see cref="Encoding.GetPreamble"/>), is always inserted at the beginning of the <see cref="StringStream"/>'s content.
	/// </para>
	/// </remarks>
	/// <seealso href="http://msdn.microsoft.com/en-us/magazine/cc163768.aspx" />
	[SuppressMessage("ReSharper", "UnusedType.Global", Justification = "Public API.")]
	public class StringStream : Stream
	{
		/// <summary>
		/// Instantiate a new <see cref="StringStream"/> that wraps a given <see cref="string"/>.
		/// </summary>
		/// <param name="string">
		/// The <see cref="string"/> to wrap.
		/// </param>
		/// <seealso href="http://msdn.microsoft.com/en-us/magazine/cc163768.aspx" />
		public StringStream(string @string)
		{
			if (@string.IsNullOrEmpty()) throw new ArgumentNullException(nameof(@string));
			_string = @string;
			// Unicode/UTF-16 code points are 2-byte wide, plus extra bytes for the Unicode BOM
			_byteLength = _string.Length * 2 + _unicodeBom.Length;
			_position = 0;
		}

		#region Base Class Member Overrides

		public override bool CanRead => true;

		public override bool CanSeek => true;

		public override bool CanWrite => false;

		public override void Flush()
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// When overridden in a derived class, gets the length in bytes of the stream.
		/// </summary>
		/// <returns>
		/// A long value representing the length of the stream in bytes.
		/// </returns>
		/// <exception cref="NotSupportedException">
		/// A class derived from Stream does not support seeking.
		/// </exception>
		/// <exception cref="ObjectDisposedException">
		/// Methods were called after the stream was closed.
		/// </exception>
		/// <remarks>
		/// The length of the string multiplied by two, since a <see cref="string"/> is a series of Unicode UTF-16 code points and each code point is 2 bytes. Notice the
		/// length will also account for the extra 2-byte BOM preamble.
		/// </remarks>
		/// <seealso href="http://msdn.microsoft.com/en-us/magazine/cc163768.aspx" />
		public override long Length => _byteLength;

		public override long Position
		{
			get => _position;
			set
			{
				if (value < 0 || value > _byteLength) throw new ArgumentOutOfRangeException(nameof(value));
				_position = (int) value;
			}
		}

		/// <summary>
		/// When overridden in a derived class, reads a sequence of bytes from the current stream and advances the position within
		/// the stream by the number of bytes read.
		/// </summary>
		/// <returns>
		/// The total number of bytes read into the buffer. This can be less than the number of bytes requested if that many bytes
		/// are not currently available, or zero (0) if the end of the stream has been reached.
		/// </returns>
		/// <param name="buffer">
		/// An array of bytes. When this method returns, the buffer contains the specified byte array with the values between
		/// <paramref name="offset"/> and (<paramref name="offset"/> + <paramref name="count"/> - 1) replaced by the bytes read
		/// from the current source.
		/// </param>
		/// <param name="offset">
		/// The zero-based byte offset in <paramref name="buffer"/> at which to begin storing the data read from the current
		/// stream.
		/// </param>
		/// <param name="count">
		/// The maximum number of bytes to be read from the current stream.
		/// </param>
		/// <exception cref="ArgumentException">
		/// The sum of <paramref name="offset"/> and <paramref name="count"/> is larger than the buffer length.
		/// </exception>
		/// <exception cref="ArgumentNullException">
		/// <paramref name="buffer"/> is null.
		/// </exception>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="offset"/> or <paramref name="count"/> is negative.
		/// </exception>
		/// <exception cref="IOException">
		/// An I/O error occurs.
		/// </exception>
		/// <exception cref="NotSupportedException">
		/// The stream does not support reading.
		/// </exception>
		/// <exception cref="ObjectDisposedException">
		/// Methods were called after the stream was closed.
		/// </exception>
		/// <remarks>
		/// The loop consists of picking up the next character from the string and extracting from it the appropriate byte by
		/// using some simple bit manipulation. <see cref="BitConverter.GetBytes(char)"/> could have been used but that would
		/// result in a 2-byte array being allocated each time the bytes for a character are retrieved, and since the whole point
		/// of this class is to avoid extraneous allocations for large strings, that would have been a bit counterproductive.
		/// </remarks>
		/// <seealso href="http://msdn.microsoft.com/en-us/magazine/cc163768.aspx" />
		public override int Read(byte[] buffer, int offset, int count)
		{
			if (buffer == null) throw new ArgumentNullException(nameof(buffer));
			var bytesRead = 0;
			while (bytesRead < count && _position < _unicodeBom.Length)
			{
				buffer[offset + bytesRead] = _unicodeBom[bytesRead];
				_position++;
				bytesRead++;
			}
			while (bytesRead < count && _position < _byteLength)
			{
				var c = _string[(_position - 2) / 2];
				buffer[offset + bytesRead] = (byte) ((_position - 2) % 2 == 0
					? c & 0xFF
					: (c >> 8) & 0xFF);
				_position++;
				bytesRead++;
			}
			return bytesRead;
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			Position = origin switch {
				SeekOrigin.Begin => offset,
				SeekOrigin.End => _byteLength + offset,
				SeekOrigin.Current => Position + offset,
				_ => Position
			};
			return Position;
		}

		public override void SetLength(long value)
		{
			throw new NotSupportedException();
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			throw new NotSupportedException();
		}

		#endregion

		private static readonly byte[] _unicodeBom = Encoding.Unicode.GetPreamble();
		private readonly long _byteLength;
		private readonly string _string;
		private int _position;
	}
}
