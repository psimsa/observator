using Xunit;
using Observator.Abstractions;

namespace TestLib
{
    public class ObservatorTraceAttributeTests
    {
        [Fact]
        public void Default_Properties_Are_Correct()
        {
            var attr = new ObservatorTraceAttribute();
            Assert.True(attr.IncludeParameters);
            Assert.True(attr.IncludeReturnValue);
        }

        [Fact]
        public void Can_Set_IncludeParameters_And_IncludeReturnValue()
        {
            var attr = new ObservatorTraceAttribute
            {
                IncludeParameters = false,
                IncludeReturnValue = false
            };
            Assert.False(attr.IncludeParameters);
            Assert.False(attr.IncludeReturnValue);
        }
    }
}