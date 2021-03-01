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

using System.IO;
using System.Reflection;
using FluentAssertions;
using Xunit;
using static FluentAssertions.FluentActions;

namespace Be.Stateless.Unit.Resources
{
	public class ResourceManagerFixture
	{
		[Fact]
		public void LoadAssemblyScopedResourceThrowsWhenNotFound()
		{
			var sut = ResourceManagerFactory.Create(Assembly.GetExecutingAssembly());
			Invoking(() => sut.LoadString("Stateless.Resources.Unknown.txt"))
				.Should().Throw<FileNotFoundException>()
				.WithMessage(
					$"Cannot find resource 'Stateless.Resources.Unknown.txt' in assembly '{typeof(ResourceManagerFixture).Assembly.FullName}'.");
		}

		[Fact]
		public void LoadAssemblyScopedResourceWithAbsoluteNameThroughAssemblyScopedResourceManager()
		{
			var sut = ResourceManagerFactory.Create(Assembly.GetExecutingAssembly());
			Invoking(() => sut.LoadString("Be.Stateless.Resources.Resource.txt").Should().Be("This is a resource scoped to an assembly.")).Should().NotThrow();
		}

		[Fact]
		public void LoadAssemblyScopedResourceWithAbsoluteNameThroughTypeScopedResourceManager()
		{
			var sut = ResourceManagerFactory.Create<ResourceManagerFixture>();
			Invoking(() => sut.LoadString("Be.Stateless.Resources.Resource.txt").Should().Be("This is a resource scoped to an assembly.")).Should().NotThrow();
		}

		[Fact]
		public void LoadAssemblyScopedResourceWithPartialNameThroughAssemblyScopedResourceManager()
		{
			var sut = ResourceManagerFactory.Create(Assembly.GetExecutingAssembly());
			sut.LoadString("Stateless.Resources.Resource.txt").Should().Be("This is a resource scoped to an assembly.");
		}

		[Fact]
		public void LoadAssemblyScopedResourceWithPartialNameThroughTypeScopedResourceManagerThrows()
		{
			var sut = ResourceManagerFactory.Create<ResourceManagerFixture>();
			Invoking(() => sut.LoadString("Stateless.Resources.Resource.txt").Should().Be("This is a resource scoped to an assembly."))
				.Should().Throw<FileNotFoundException>()
				.WithMessage(
					$"Cannot find resource 'Stateless.Resources.Resource.txt' scoped to type '{typeof(ResourceManagerFixture).FullName}' in assembly '{typeof(ResourceManagerFixture).Assembly.FullName}'.");
		}

		[Fact]
		public void LoadTypeScopedResource()
		{
			var sut = ResourceManagerFactory.Create<ResourceManagerFixture>();
			sut.LoadString("Resource.txt").Should().Be("This is a resource scoped to a type.");
		}

		[Fact]
		public void LoadTypeScopedResourceThrowsWhenNotFound()
		{
			var sut = ResourceManagerFactory.Create<ResourceManagerFixture>();
			Invoking(() => sut.LoadString("Nonexistent.txt"))
				.Should().Throw<FileNotFoundException>()
				.WithMessage(
					$"Cannot find resource 'Nonexistent.txt' scoped to type '{typeof(ResourceManagerFixture).FullName}' in assembly '{typeof(ResourceManagerFixture).Assembly.FullName}'.");
		}

		[Fact]
		public void LoadTypeSubScopedResource()
		{
			var sut = ResourceManagerFactory.Create<ResourceManagerFixture>();
			sut.LoadString("Data.Resource.txt").Should().Be("This is a resource sub-scoped to a type.");
		}

		[Fact]
		public void LoadTypeSubScopedResourceWithCompositeName()
		{
			var sut = ResourceManagerFactory.Create<ResourceManagerFixture>();
			sut.LoadString("Data.Resource.Composite.Name.txt").Should().Be("This is a resource with a composite name and sub-scoped to a type.");
		}
	}
}
