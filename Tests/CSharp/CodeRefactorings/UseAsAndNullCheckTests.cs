using NUnit.Framework;
using RefactoringEssentials.CSharp.CodeRefactorings;

namespace RefactoringEssentials.Tests.CSharp.CodeRefactorings
{
    [TestFixture]
    public class UseAsAndNullCheckTests : CSharpCodeRefactoringTestBase
    {

        [Test]
        public void EmptyCase()
        {
            Test<UseAsAndNullCheckCodeRefactoringProvider>(@"
class Bar
{
    public Bar Baz (object foo)
    {
        if (foo $is Bar) {
        }
        return null;
    }
}
", @"
class Bar
{
    public Bar Baz (object foo)
    {
        var bar = foo as Bar;
        if (bar != null) {
        }
        return null;
    }
}
");
        }


        [Test]
        public void SimpleCase()
        {
            Test<UseAsAndNullCheckCodeRefactoringProvider>(@"
class Bar
{
    public Bar Baz (object foo)
    {
        if (foo $is Bar) {
            Baz ((Bar)foo);
            return (Bar)foo;
        }
        return null;
    }
}
", @"
class Bar
{
    public Bar Baz (object foo)
    {
        var bar = foo as Bar;
        if (bar != null) {
            Baz (bar);
            return bar;
        }
        return null;
    }
}
");
        }

        [Test]
        public void SimpleCaseWithKeywordVarNames()
        {
            Test<UseAsAndNullCheckCodeRefactoringProvider>(@"
class Int
{
    public Int Baz (object foo)
    {
        if (foo $is Int) {
            Baz ((Int)foo);
            return (Int)foo;
        }
        return null;
    }
}
", @"
class Int
{
    public Int Baz (object foo)
    {
        var @int = foo as Int;
        if (@int != null) {
            Baz (@int);
            return @int;
        }
        return null;
    }
}
");
        }

        [Test]
        public void NegatedCase()
        {
            Test<UseAsAndNullCheckCodeRefactoringProvider>(@"
class Bar
{
    public Bar Baz (object foo)
    {
        if (!(foo $is Bar)) {
            Baz ((Bar)foo);
        }
        return null;
    }
}
", @"
class Bar
{
    public Bar Baz (object foo)
    {
        var bar = foo as Bar;
        if (bar == null) {
            Baz ((Bar)foo);
        }
        return null;
    }
}
");
        }


        [Test]
        public void NegatedEmbeddedCase()
        {
            Test<UseAsAndNullCheckCodeRefactoringProvider>(@"
class Bar
{
    public Bar Baz (object foo)
    {
        if (true) {
        } else if (!(foo $is Bar)) {
            Baz ((Bar)foo);
        }
        return null;
    }
}
", @"
class Bar
{
    public Bar Baz (object foo)
    {
        if (true) {
        } else
        {
            var bar = foo as Bar;
            if (bar == null)
            {
                Baz((Bar)foo);
            }
        }

        return null;
    }
}
");
        }

        [Test]
        public void ComplexCase()
        {
            Test<UseAsAndNullCheckCodeRefactoringProvider>(@"
class Bar
{
    public IDisposable Baz (object foo)
    {
        if (((foo) $is Bar)) {
            Baz ((Bar)foo);
            Baz (foo as Bar);
            Baz (((foo) as Bar));
            Baz ((Bar)(foo));
            return (IDisposable)foo;
        }
        return null;
    }
}
", @"
class Bar
{
    public IDisposable Baz (object foo)
    {
        var bar = (foo) as Bar;
        if (bar != null) {
            Baz (bar);
            Baz (bar);
            Baz (bar);
            Baz (bar);
            return (IDisposable)foo;
        }
        return null;
    }
}
");
        }

        [Test]
        public void IfElseCase()
        {
            Test<UseAsAndNullCheckCodeRefactoringProvider>(@"
class Bar
{
    public Bar Baz (object foo)
    {
        if (foo $is Bar) {
            Baz ((Bar)foo);
            return (Bar)foo;
        } else {
            Console.WriteLine (""Hello World "");
        }
        return null;
    }
}
", @"
class Bar
{
    public Bar Baz (object foo)
    {
        var bar = foo as Bar;
        if (bar != null) {
            Baz (bar);
            return bar;
        } else {
            Console.WriteLine (""Hello World "");
        }
        return null;
    }
}
");
        }

        [Test]
        public void NestedIf()
        {
            Test<UseAsAndNullCheckCodeRefactoringProvider>(@"
class Bar
{
    public Bar Baz (object foo)
    {
        if (foo is string) {
        } else if (foo $is Bar) {
            Baz ((Bar)foo);
            return (Bar)foo;
        }

        return null;
    }
}
", @"
class Bar
{
    public Bar Baz (object foo)
    {
        if (foo is string) {
        } else
        {
            var bar = foo as Bar;
            if (bar != null)
            {
                Baz(bar);
                return bar;
            }
        }

        return null;
    }
}
");
        }

        [Test]
        public void TestNegatedCaseWithReturn()
        {
            Test<UseAsAndNullCheckCodeRefactoringProvider>(@"
class Bar
{
    public Bar Baz (object foo)
    {
        if (!(foo $is Bar))
            return null;
        Baz ((Bar)foo);
        return (Bar)foo;
    }
}
", @"
class Bar
{
    public Bar Baz (object foo)
    {
        var bar = foo as Bar;
        if (bar == null)
            return null;
        Baz (bar);
        return bar;
    }
}
");
        }

        [Test]
        public void TestNegatedCaseWithBreak()
        {
            Test<UseAsAndNullCheckCodeRefactoringProvider>(@"
class Bar
{
    public Bar Baz (object foo)
    {
        for (int i = 0; i < 10; i++) {
            if (!(foo $is Bar))
                break;
            Baz ((Bar)foo);
        }
        return (Bar)foo;
    }
}
", @"
class Bar
{
    public Bar Baz (object foo)
    {
        for (int i = 0; i < 10; i++) {
            var bar = foo as Bar;
            if (bar == null)
                break;
            Baz (bar);
        }
        return (Bar)foo;
    }
}
");
        }

        [Test]
        public void TestCaseWithContinue()
        {
            Test<UseAsAndNullCheckCodeRefactoringProvider>(@"
class Bar
{
    public Bar Baz (object foo)
    {
        for (int i = 0; i < 10; i++) {
            if (!(foo $is Bar)) {
                continue;
            } else {
                foo = new Bar ();
            }
            Baz ((Bar)foo);
        }
        return (Bar)foo;
    }
}
", @"
class Bar
{
    public Bar Baz (object foo)
    {
        for (int i = 0; i < 10; i++) {
            var bar = foo as Bar;
            if (bar == null) {
                continue;
            } else {
                foo = new Bar ();
            }
            Baz (bar);
        }
        return (Bar)foo;
    }
}
");
        }


        [Test]
        public void ConditionalCase()
        {
            Test<UseAsAndNullCheckCodeRefactoringProvider>(@"
class Bar
{
    public Bar Baz (object foo)
    {
        if (((Bar)foo).y && foo $is Bar && ((Bar)foo).x) {
            Baz ((Bar)foo);
        }
        return null;
    }
}
", @"
class Bar
{
    public Bar Baz (object foo)
    {
        var bar = foo as Bar;
        if (((Bar)foo).y && bar != null && bar.x) {
            Baz (bar);
        }
        return null;
    }
}
");
        }

        [Test]
        public void InvalidCase()
        {
            TestWrongContext<UseAsAndNullCheckCodeRefactoringProvider>(@"
class Bar
{
    public int Baz (object foo)
    {
        if (foo $is int) {
            Baz ((int)foo);
            return (int)foo;
        }
        return 0;
    }
}
");
        }

        [Test]
        public void InvalidCase2()
        {
            TestWrongContext<UseAsAndNullCheckCodeRefactoringProvider>(@"
class Bar
{
    public int Baz (object foo)
    {
        if (!(foo $is int)) {
        }
        return 0;
    }
}
");
        }

    }
}

