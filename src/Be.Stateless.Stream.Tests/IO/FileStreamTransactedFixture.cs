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
using System.Text;
using FluentAssertions;
using Xunit;

namespace Be.Stateless.IO
{
	// ensure tests run sequentially to avoid side-effects between them, see https://stackoverflow.com/questions/1408175/execute-unit-tests-serially-rather-than-in-parallel
	[Collection("FileTransacted")]
	public class FileStreamTransactedFixture
	{
		public FileStreamTransactedFixture()
		{
			_buffer = Encoding.Unicode.GetBytes("foobar");
			_filename = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".txt");
		}

		~FileStreamTransactedFixture()
		{
			File.Delete(_filename);
		}

		[Fact]
		public void TransactionCommitHasToBeExplicit()
		{
			using (var file = FileTransacted.Create(_filename))
			{
				file.Should().BeOfType<FileStreamTransacted>();
				file.Write(_buffer, 0, _buffer.Length);
				((FileStreamTransacted) file).Commit();
			}

			File.Exists(_filename).Should().BeTrue("Transaction should have been committed: file is not found.");
		}

		[Fact]
		public void TransactionRollbackCanBeExplicit()
		{
			using (var file = FileTransacted.Create(_filename))
			{
				file.Should().BeOfType<FileStreamTransacted>();
				file.Write(_buffer, 0, _buffer.Length);
				((FileStreamTransacted) file).Rollback();
			}

			File.Exists(_filename).Should().BeFalse("Transaction should have been rolled back: file is found.");
		}

		[Fact]
		public void TransactionRollbackOnClose()
		{
			using (var file = FileTransacted.Create(_filename))
			{
				file.Should().BeOfType<FileStreamTransacted>();
				file.Write(_buffer, 0, _buffer.Length);
				file.Close();
			}

			File.Exists(_filename).Should().BeFalse("Transaction should have been rolled back: file is found.");
		}

		[Fact]
		public void TransactionRollbackOnDispose()
		{
			using (var file = FileTransacted.Create(_filename))
			{
				file.Should().BeOfType<FileStreamTransacted>();
				file.Write(_buffer, 0, _buffer.Length);
			}

			File.Exists(_filename).Should().BeFalse("Transaction should have been rolled back: file is found.");
		}

		[Fact]
		[SuppressMessage("ReSharper", "RedundantAssignment")]
		public void TransactionRollbackOnFinalize()
		{
			var file = FileTransacted.Create(_filename);
			file.Should().BeOfType<FileStreamTransacted>();
			file.Write(_buffer, 0, _buffer.Length);
			file = null;
			GC.Collect();
			GC.WaitForPendingFinalizers();
			File.Exists(_filename).Should().BeFalse("Transaction should have been rolled back: file is found.");
		}

		private readonly byte[] _buffer;
		private readonly string _filename;
	}
}
