using System;

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

    // variance check
    interface ICovariance<out TSelf> { }
    interface IContravariance<in TSelf> { }
    class Something { }
    public class VariantBase { }
    // covariant
    public class CovariantOK : ICovariance<object> { }
    public class CovariantOK2 : ICovariance<Object> { }
    public class CovariantOK3 : ICovariance<System.Object> { }
    public class CovariantOK4 : ICovariance<CovariantOK4> { }
    public class CovariantOK5 : VariantBase, ICovariance<VariantBase> { }
    public class CovariantNG : ICovariance<VariantBase> { }
    public class CovariantNG2 : ICovariance<CovariantNG3> { }
    public class CovariantNG3 : CovariantNG2, ICovariance<Something> { }
    // contravariant
    public class ContravariantOK : IContravariance<ContravariantOK2> { }
    public class ContravariantOK2 : ContravariantOK, IContravariance<ContravariantOK2> { }
    public class ContravariantNG : IContravariance<ContravariantOK2> { }
    public class ContravariantNG2 : IContravariance<Something> { }
    public class ContravariantNG3 : IContravariance<object> { }
    // invariant
    public interface ITypeArgConstraint<TSelf> where TSelf : ITypeArgConstraint<TSelf> { }
    public class InvariantOK : ITypeArgConstraint<InvariantOK> { };
    public class InvariantNG : ITypeArgConstraint<InvariantOK> { };

    // namespace WON'T be considered
    public class NamespaceNotConsidered
        : TSelfBase<int, AnalyzerCheck.NamespaceNotConsidered>, ITest<NamespaceNotConsidered, long>
    {
    }

    // weird trivia
    public class TSelfWeirdSyntaxTester
        :
        TSelfBase<
            int,
            /**/ TSelfWeirdSyntaxTester /**/ >,
        ITest</**/ TSelfWeirdSyntaxTester                        /**/, long
            >

    {
    }
}
