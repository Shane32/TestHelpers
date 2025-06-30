using System.Linq.Expressions;

namespace Tests;

public class ExpressionExtensionsTests
{
    [Fact]
    public void DebugViewWorks()
    {
        Expression<Func<int, int, int>> expr = (int a, int b) => a + b;
        var debugView = expr.GetDebugView();
        debugView.ShouldBe("""
            .Lambda #Lambda1<System.Func`3[System.Int32,System.Int32,System.Int32]>(
                System.Int32 $a,
                System.Int32 $b) {
                $a + $b
            }
            """, StringCompareShould.IgnoreLineEndings);
    }
}
