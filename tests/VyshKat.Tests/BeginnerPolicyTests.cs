using VyshKat.Services;

namespace VyshKat.Tests;

public class BeginnerPolicyTests
{
    [Theory]
    [InlineData(5, false)]
    [InlineData(6, true)]
    [InlineData(10, true)]
    public void DefaultPolicy_RequiresSimplicityAtLeastSix(int simplicity, bool expected)
    {
        var policy = new SimplicityBeginnerPolicy();

        Assert.Equal(expected, policy.IsSuitable(TestData.Scooter(simplicity: simplicity)));
    }

    [Fact]
    public void CustomThreshold_IsRespected()
    {
        var policy = new SimplicityBeginnerPolicy(minSimplicity: 8);

        Assert.False(policy.IsSuitable(TestData.Bicycle(simplicity: 7)));
        Assert.True(policy.IsSuitable(TestData.Bicycle(simplicity: 8)));
        Assert.Equal("простота ≥ 8", policy.Description);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(11)]
    public void InvalidThreshold_Throws(int threshold)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new SimplicityBeginnerPolicy(threshold));
    }
}
