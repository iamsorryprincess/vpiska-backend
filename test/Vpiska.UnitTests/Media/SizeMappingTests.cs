using Vpiska.Domain.Media;
using Xunit;

namespace Vpiska.UnitTests.Media
{
    public sealed class SizeMappingTests
    {
        [Fact]
        public void TestKb() => Assert.Equal("73.18 KB", 74935.MapToSize());
        
        [Fact]
        public void TestMb() => Assert.Equal("1.52 MB", 1588736.MapToSize());
        
        [Fact]
        public void TestGb() => Assert.Equal("1.48 GB", 1588736937.MapToSize());
        
        [Fact]
        public void TestTb() => Assert.Equal("1.44 TB", 1588736937491.MapToSize());
        
        [Fact]
        public void TestPb() => Assert.Equal("8.77 PB", 9876543211231230.MapToSize());
    }
}