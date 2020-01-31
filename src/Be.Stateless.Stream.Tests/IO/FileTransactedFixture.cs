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
using System.IO;
using Be.Stateless.Extensions;
using FluentAssertions;
using Xunit;

#if NETFRAMEWORK
using System.Diagnostics.CodeAnalysis;
using System.Transactions;
#endif

namespace Be.Stateless.IO
{
	// ensure tests run sequentially to avoid side-effects between them, see https://stackoverflow.com/questions/1408175/execute-unit-tests-serially-rather-than-in-parallel
	[Collection("FileTransacted")]
	public class FileTransactedFixture
	{
		public FileTransactedFixture()
		{
			_filename = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".txt");
		}

		~FileTransactedFixture()
		{
			File.Delete(_filename);
			File.Delete(_filename + ".moved");
			FileTransacted._operatingSystem = Environment.OSVersion;
		}

		[Fact]
		public void CreateFileStreamTransactedWhenTransactionalFileSystemSupported()
		{
			FileTransacted._operatingSystem = OperatingSystemExtensionsFixture.Windows7;
			using (var file = FileTransacted.Create(_filename))
			{
				file.Should().BeOfType<FileStreamTransacted>();
			}
		}

#if NETFRAMEWORK
		[Fact]
		[SuppressMessage("ReSharper", "SuspiciousTypeConversion.Global")]
		public void CreateFileStreamWhenGivenAmbientTransactionAndTransactionalFileSystemSupported()
		{
			FileTransacted._operatingSystem = OperatingSystemExtensionsFixture.Windows7;
			using (new TransactionScope())
			{
				// grab kernel level transaction handle
				var dtcTransaction = TransactionInterop.GetDtcTransaction(Transaction.Current);
				var kernelTransaction = (IKernelTransaction) dtcTransaction;
				var file = FileTransacted.Create(_filename, 1024, kernelTransaction);
				file.Should().BeOfType<FileStream>();
			}
		}
#endif

		[Fact]
		public void CreateFileStreamWhenNetworkPath()
		{
			var uncFilename = @"\\localhost\" + _filename.Replace(':', '$');
			FileTransacted._operatingSystem = OperatingSystemExtensionsFixture.Windows7;
			using (var file = FileTransacted.Create(uncFilename))
			{
				file.Should().BeOfType<FileStream>();
			}
		}

		[Fact]
		public void CreateFileStreamWhenTransactionalFileSystemUnsupported()
		{
			FileTransacted._operatingSystem = OperatingSystemExtensionsFixture.WindowsXP;
			using (var file = FileTransacted.Create(_filename))
			{
				file.Should().BeOfType<FileStream>();
			}
		}

#if NETFRAMEWORK
		[Fact]
		[SuppressMessage("ReSharper", "SuspiciousTypeConversion.Global")]
		public void MoveWhenAmbientTransactionCompletes()
		{
			using (var writer = File.CreateText(_filename))
			{
				writer.WriteLine("test");
			}

			File.Exists(_filename).Should().BeTrue();
			File.Exists(_filename + ".moved").Should().BeFalse();

			FileTransacted._operatingSystem = OperatingSystemExtensionsFixture.Windows7;
			using (var scope = new TransactionScope())
			{
				var dtcTransaction = TransactionInterop.GetDtcTransaction(Transaction.Current);
				var kernelTransaction = (IKernelTransaction) dtcTransaction;
				FileTransacted.Move(_filename, _filename + ".moved", kernelTransaction);
				// this is the root scope and it has to cast a vote
				scope.Complete();
			}

			File.Exists(_filename).Should().BeFalse();
			File.Exists(_filename + ".moved").Should().BeTrue();
		}
#endif

#if NETFRAMEWORK
		[Fact]
		public void MoveWhenAmbientTransactionDoesNotComplete()
		{
			using (var writer = File.CreateText(_filename))
			{
				writer.WriteLine("test");
			}

			File.Exists(_filename).Should().BeTrue();
			File.Exists(_filename + ".moved").Should().BeFalse();

			FileTransacted._operatingSystem = OperatingSystemExtensionsFixture.Windows7;
			using (new TransactionScope())
			{
				var dtcTransaction = TransactionInterop.GetDtcTransaction(Transaction.Current);
				// ReSharper disable once SuspiciousTypeConversion.Global
				var kernelTransaction = (IKernelTransaction) dtcTransaction;
				FileTransacted.Move(_filename, _filename + ".moved", kernelTransaction);
				// this is the root scope and failing to cast a vote will abort the ambient transaction
			}

			File.Exists(_filename).Should().BeTrue();
			File.Exists(_filename + ".moved").Should().BeFalse();
		}
#endif

		private readonly string _filename;
	}
}
