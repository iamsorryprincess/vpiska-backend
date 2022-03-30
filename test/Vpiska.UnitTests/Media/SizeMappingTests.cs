using Vpiska.Domain.Media;
using Xunit;

namespace Vpiska.UnitTests.Media
{
    public sealed class SizeMappingTests
    {
        [Theory]
        [InlineData("73.18 KB", "74935")]
        [InlineData("1.52 MB", "1588736")]
        [InlineData("1.48 GB", "1588736937")]
        [InlineData("1.44 TB", "1588736937491")]
        [InlineData("8.77 PB", "9876543211231230")]
        public void TestSizeMapping(string expected, string actual)
        {
            Assert.Equal(expected, long.Parse(actual).MapToSize());
        }
    }
}