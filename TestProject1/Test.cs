
using MODEL;
using Xunit;
using Assert = Xunit.Assert;

namespace TestProject1;

public class Test
{
    [Fact]
    public void TestTool()
    {
        
        Assert.Equal(Tool.filterName("3d"), "");
    }
}