﻿#region Copyright & License

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
using System.IO;
using System.Linq;
using System.Text;

namespace Be.Stateless.IO
{
	/// <summary>
	/// Text <see cref="Stream"/> whose content is dynamically generated up to a rounded-up size limit.
	/// </summary>
	/// <remarks>
	/// The text content that is generated is made of <see cref="Guid"/>s, which, because of their pseudo-random nature, may
	/// come handy in claim-check scenarios as such a <see cref="Stream"/> will exhibit a poor compression rate.
	/// </remarks>
	public class FakeTextStream : EnumerableStream
	{
		static FakeTextStream()
		{
			_guidByteCount = Encoding.Unicode.GetByteCount(Guid.NewGuid().ToString("N"));
		}

		public static Stream Create(int size)
		{
			return new FakeTextStream(size / _guidByteCount + 1);
		}

		private FakeTextStream(int count) : base(Enumerable.Range(0, count).Select(i => Guid.NewGuid().ToString("N")))
		{
			_length = count * _guidByteCount;
		}

		#region Base Class Member Overrides

		public override long Length => _length;

		#endregion

		private static readonly int _guidByteCount;
		private readonly int _length;
	}
}
