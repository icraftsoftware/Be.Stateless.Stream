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

#if NETFRAMEWORK
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Transactions;
using FluentAssertions;
using Xunit;

namespace Be.Stateless.IO
{
	public class FileStreamFixture
	{
		[Fact]
		[SuppressMessage("ReSharper", "SuspiciousTypeConversion.Global")]
		public void TransactionCommitWithAmbientTransaction()
		{
			using (var scope = new TransactionScope())
			{
				var kernelTransaction = (IKernelTransaction) TransactionInterop.GetDtcTransaction(Transaction.Current);
				using (var file = TransactionalFile.Create(_filename, 1024, kernelTransaction))
				{
					file.Should().BeOfType<FileStream>();
					file.Write(_buffer, 0, _buffer.Length);
				}
				// this is the root scope and it has to cast a vote
				scope.Complete();
			}

			File.Exists(_filename).Should().BeTrue();
		}

		[Fact]
		[SuppressMessage("ReSharper", "SuspiciousTypeConversion.Global")]
		public void TransactionRollbackWithAmbientTransaction()
		{
			using (new TransactionScope())
			{
				var kernelTransaction = (IKernelTransaction) TransactionInterop.GetDtcTransaction(Transaction.Current);
				using (var file = TransactionalFile.Create(_filename, 1024, kernelTransaction))
				{
					file.Should().BeOfType<FileStream>();
					file.Write(_buffer, 0, _buffer.Length);
				}
				// this is the root scope and failing to cast a vote will abort the ambient transaction
			}

			File.Exists(_filename).Should().BeFalse();
		}

		private static string GetTempFileName()
		{
			return System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".txt");
		}

		~FileStreamFixture()
		{
			File.Delete(_filename);
		}

		private readonly byte[] _buffer = Encoding.Unicode.GetBytes("foobar");

		private static readonly string _filename = GetTempFileName();
	}
}
#endif
