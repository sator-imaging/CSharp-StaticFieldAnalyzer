using System.ComponentModel;

namespace AnalyzerCheck
{
    /// <summary>
    /// <see cref="ITest{TSelf, T}"/>
    /// <see cref="TSelfWeirdSyntaxTester"/>
    /// <inheritdoc cref="TSelfBase{T, TSelf}"/>
    public interface ITest<TSelf, T> { }
    public class TSelfBase<T, TSelf> { }

    public class ClassOK : TSelfBase<byte, ClassOK> { }
    public class ClassNG : TSelfBase<byte, ClassOK> { }
    public struct IFaceOK : ITest<IFaceOK, short> { }
    public struct IFaceNG : ITest<IFaceOK, short> { }
    public class BothOK : TSelfBase<int, BothOK>, ITest<BothOK, long> { }
    public class BothNG : TSelfBase<int, BothOK>, ITest<BothOK, long> { }
    public class IFaceIFaceOK : ITest<IFaceIFaceOK, int> { }
    public class IFaceIFaceNG : ITest<IFaceIFaceOK, int> { }

    public class ITypeArgRestriction<TSelf> where TSelf : ITypeArgRestriction<TSelf> { }

    [Category]
    [DesignerCategory]
    [DescriptionAttribute("" + nameof(TSelfWeirdSyntaxTester) + "")]
    public class TSelfWeirdSyntaxTester
        :
        TSelfBase<
            int,
            /**/ TSelfWeirdSyntaxTester /**/ >,
        ITest</**/ TSelfWeirdSyntaxTester                        /**/, long
            >

    {
    }

    // namespace WON'T be considered
    public class NamespaceNotConsidered
        : TSelfBase<int, AnalyzerCheck.NamespaceNotConsidered>, ITest<NamespaceNotConsidered, long>
    { }
}
