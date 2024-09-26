using SingleFeatureCases;
using SingleFeatureCases.Cases.Interfaces.I;
using SingleFeatureCases.Cases.Interfaces.II;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Pooling.Fody.Tests
{
    public class SingleFeatureTests
    {
        private const string CONFIG = """
            <Pooling enabled="true" composite-accessibility="false">
              <Inspect>
              </Inspect>
              <NotInspect>
              </NotInspect>
              <Items>
              </Items>
            </Pooling>
            """;

        private static readonly WeavedAssembly Assembly;

        static SingleFeatureTests()
        {
            Assembly = new("SingleFeatureCases.dll", "SingleFeatureCases", CONFIG);
            Pool.Set((IPool)Assembly.GetInstance(typeof(StatefulPool).FullName!, true));
        }

        [Fact]
        public void StaticCtorTest()
        {
            var sNonPattern1 = Assembly.GetStaticInstance(typeof(NonPattern1).FullName!, true);
            var sNonPattern2 = Assembly.GetStaticInstance(typeof(NonPattern2).FullName!, true);
            var sNonTypes1 = Assembly.GetStaticInstance(typeof(NonTypes1).FullName!, true);
            var sNonTypes2 = Assembly.GetStaticInstance(typeof(NonTypes2).FullName!, true);
            var sOther1 = Assembly.GetStaticInstance(typeof(Other1).FullName!, true);
            var sOther2 = Assembly.GetStaticInstance(typeof(Other2).FullName!, true);

            _ = sNonPattern1.Activator;
            AssertPoolingResult(sNonPattern1.PoolingResult);

            _ = sNonPattern2.Activator;
            AssertPoolingResult(sNonPattern2.PoolingResult);

            _ = sNonTypes1.Activator;
            AssertPoolingResult(sNonTypes1.PoolingResult);

            _ = sNonTypes2.Activator;
            AssertPoolingResult(sNonTypes2.PoolingResult);

            _ = sOther1.Activator;
            AssertPoolingResult(sOther1.PoolingResult);

            _ = sOther2.Activator;
            AssertPoolingResult(sOther2.PoolingResult);
        }

        [Fact]
        public void InstanceCtorTest()
        {
            var sNonPattern1 = Assembly.GetStaticInstance(typeof(NonPattern1).FullName!, true);
            var sNonPattern2 = Assembly.GetStaticInstance(typeof(NonPattern2).FullName!, true);
            var sNonTypes1 = Assembly.GetStaticInstance(typeof(NonTypes1).FullName!, true);
            var sNonTypes2 = Assembly.GetStaticInstance(typeof(NonTypes2).FullName!, true);
            var sOther1 = Assembly.GetStaticInstance(typeof(Other1).FullName!, true);
            var sOther2 = Assembly.GetStaticInstance(typeof(Other2).FullName!, true);

            var nonPattern1 = Assembly.GetInstance(typeof(NonPattern1).FullName!, true);
            var nonPattern2 = Assembly.GetInstance(typeof(NonPattern2).FullName!, true);
            var nonTypes1 = Assembly.GetInstance(typeof(NonTypes1).FullName!, true);
            var nonTypes2 = Assembly.GetInstance(typeof(NonTypes2).FullName!, true);
            var other1 = Assembly.GetInstance(typeof(Other1).FullName!, true);
            var other2 = Assembly.GetInstance(typeof(Other2).FullName!, true);

            AssertPoolingResult(sNonPattern1, sNonPattern2, sNonTypes1, sNonTypes2, sOther1, sOther2);
        }

        [Fact]
        public void StaticGetterTest()
        {
            var sNonPattern1 = Assembly.GetStaticInstance(typeof(NonPattern1).FullName!, true);
            var sNonPattern2 = Assembly.GetStaticInstance(typeof(NonPattern2).FullName!, true);
            var sNonTypes1 = Assembly.GetStaticInstance(typeof(NonTypes1).FullName!, true);
            var sNonTypes2 = Assembly.GetStaticInstance(typeof(NonTypes2).FullName!, true);
            var sOther1 = Assembly.GetStaticInstance(typeof(Other1).FullName!, true);
            var sOther2 = Assembly.GetStaticInstance(typeof(Other2).FullName!, true);

            _ = sNonPattern1.StaticProp;
            _ = sNonPattern2.StaticProp;
            _ = sNonTypes1.StaticProp;
            _ = sNonTypes2.StaticProp;
            _ = sOther1.StaticProp;
            _ = sOther2.StaticProp;

            AssertPoolingResult(sNonPattern1, sNonPattern2, sNonTypes1, sNonTypes2, sOther1, sOther2);
        }

        [Fact]
        public void InstanceGetterTest()
        {
            var sNonPattern1 = Assembly.GetStaticInstance(typeof(NonPattern1).FullName!, true);
            var sNonPattern2 = Assembly.GetStaticInstance(typeof(NonPattern2).FullName!, true);
            var sNonTypes1 = Assembly.GetStaticInstance(typeof(NonTypes1).FullName!, true);
            var sNonTypes2 = Assembly.GetStaticInstance(typeof(NonTypes2).FullName!, true);
            var sOther1 = Assembly.GetStaticInstance(typeof(Other1).FullName!, true);
            var sOther2 = Assembly.GetStaticInstance(typeof(Other2).FullName!, true);

            var nonPattern1 = Assembly.GetInstance(typeof(NonPattern1).FullName!, true);
            var nonPattern2 = Assembly.GetInstance(typeof(NonPattern2).FullName!, true);
            var nonTypes1 = Assembly.GetInstance(typeof(NonTypes1).FullName!, true);
            var nonTypes2 = Assembly.GetInstance(typeof(NonTypes2).FullName!, true);
            var other1 = Assembly.GetInstance(typeof(Other1).FullName!, true);
            var other2 = Assembly.GetInstance(typeof(Other2).FullName!, true);

            _ = nonPattern1.Prop;
            _ = nonPattern2.Prop;
            _ = nonTypes1.Prop;
            _ = nonTypes2.Prop;
            _ = other1.Prop;
            _ = other2.Prop;

            AssertPoolingResult(sNonPattern1, sNonPattern2, sNonTypes1, sNonTypes2, sOther1, sOther2);
        }

        [Fact]
        public void StaticSetterTest()
        {
            var sNonPattern1 = Assembly.GetStaticInstance(typeof(NonPattern1).FullName!, true);
            var sNonPattern2 = Assembly.GetStaticInstance(typeof(NonPattern2).FullName!, true);
            var sNonTypes1 = Assembly.GetStaticInstance(typeof(NonTypes1).FullName!, true);
            var sNonTypes2 = Assembly.GetStaticInstance(typeof(NonTypes2).FullName!, true);
            var sOther1 = Assembly.GetStaticInstance(typeof(Other1).FullName!, true);
            var sOther2 = Assembly.GetStaticInstance(typeof(Other2).FullName!, true);

            sNonPattern1.set_StaticProp(null);
            sNonPattern2.set_StaticProp(null);
            sNonTypes1.set_StaticProp(null);
            sNonTypes2.set_StaticProp(null);
            sOther1.set_StaticProp(null);
            sOther2.set_StaticProp(null);

            AssertPoolingResult(sNonPattern1, sNonPattern2, sNonTypes1, sNonTypes2, sOther1, sOther2);
        }

        [Fact]
        public void InstanceSetterTest()
        {
            var sNonPattern1 = Assembly.GetStaticInstance(typeof(NonPattern1).FullName!, true);
            var sNonPattern2 = Assembly.GetStaticInstance(typeof(NonPattern2).FullName!, true);
            var sNonTypes1 = Assembly.GetStaticInstance(typeof(NonTypes1).FullName!, true);
            var sNonTypes2 = Assembly.GetStaticInstance(typeof(NonTypes2).FullName!, true);
            var sOther1 = Assembly.GetStaticInstance(typeof(Other1).FullName!, true);
            var sOther2 = Assembly.GetStaticInstance(typeof(Other2).FullName!, true);

            var nonPattern1 = Assembly.GetInstance(typeof(NonPattern1).FullName!, true);
            var nonPattern2 = Assembly.GetInstance(typeof(NonPattern2).FullName!, true);
            var nonTypes1 = Assembly.GetInstance(typeof(NonTypes1).FullName!, true);
            var nonTypes2 = Assembly.GetInstance(typeof(NonTypes2).FullName!, true);
            var other1 = Assembly.GetInstance(typeof(Other1).FullName!, true);
            var other2 = Assembly.GetInstance(typeof(Other2).FullName!, true);

            nonPattern1.Prop = null;
            nonPattern2.Prop = null;
            nonTypes1.Prop = null;
            nonTypes2.Prop = null;
            other1.Prop = null;
            other2.Prop = null;

            AssertPoolingResult(sNonPattern1, sNonPattern2, sNonTypes1, sNonTypes2, sOther1, sOther2);
        }

        [Fact]
        public void StaticSyncTest()
        {
            var sNonPattern1 = Assembly.GetStaticInstance(typeof(NonPattern1).FullName!, true);
            var sNonPattern2 = Assembly.GetStaticInstance(typeof(NonPattern2).FullName!, true);
            var sNonTypes1 = Assembly.GetStaticInstance(typeof(NonTypes1).FullName!, true);
            var sNonTypes2 = Assembly.GetStaticInstance(typeof(NonTypes2).FullName!, true);
            var sOther1 = Assembly.GetStaticInstance(typeof(Other1).FullName!, true);
            var sOther2 = Assembly.GetStaticInstance(typeof(Other2).FullName!, true);

            sNonPattern1.StaticSync();
            sNonPattern2.StaticSync();
            sNonTypes1.StaticSync();
            sNonTypes2.StaticSync();
            sOther1.StaticSync();
            sOther2.StaticSync();

            AssertPoolingResult(sNonPattern1, sNonPattern2, sNonTypes1, sNonTypes2, sOther1, sOther2);
        }

        [Fact]
        public void InstanceSyncTest()
        {
            var sNonPattern1 = Assembly.GetStaticInstance(typeof(NonPattern1).FullName!, true);
            var sNonPattern2 = Assembly.GetStaticInstance(typeof(NonPattern2).FullName!, true);
            var sNonTypes1 = Assembly.GetStaticInstance(typeof(NonTypes1).FullName!, true);
            var sNonTypes2 = Assembly.GetStaticInstance(typeof(NonTypes2).FullName!, true);
            var sOther1 = Assembly.GetStaticInstance(typeof(Other1).FullName!, true);
            var sOther2 = Assembly.GetStaticInstance(typeof(Other2).FullName!, true);

            var nonPattern1 = Assembly.GetInstance(typeof(NonPattern1).FullName!, true);
            var nonPattern2 = Assembly.GetInstance(typeof(NonPattern2).FullName!, true);
            var nonTypes1 = Assembly.GetInstance(typeof(NonTypes1).FullName!, true);
            var nonTypes2 = Assembly.GetInstance(typeof(NonTypes2).FullName!, true);
            var other1 = Assembly.GetInstance(typeof(Other1).FullName!, true);
            var other2 = Assembly.GetInstance(typeof(Other2).FullName!, true);

            nonPattern1.Sync();
            nonPattern2.Sync();
            nonTypes1.Sync();
            nonTypes2.Sync();
            other1.Sync();
            other2.Sync();

            AssertPoolingResult(sNonPattern1, sNonPattern2, sNonTypes1, sNonTypes2, sOther1, sOther2);
        }

        [Fact]
        public async Task StaticAsyncTest()
        {
            var sNonPattern1 = Assembly.GetStaticInstance(typeof(NonPattern1).FullName!, true);
            var sNonPattern2 = Assembly.GetStaticInstance(typeof(NonPattern2).FullName!, true);
            var sNonTypes1 = Assembly.GetStaticInstance(typeof(NonTypes1).FullName!, true);
            var sNonTypes2 = Assembly.GetStaticInstance(typeof(NonTypes2).FullName!, true);
            var sOther1 = Assembly.GetStaticInstance(typeof(Other1).FullName!, true);
            var sOther2 = Assembly.GetStaticInstance(typeof(Other2).FullName!, true);

            await (Task)sNonPattern1.StaticAsync();
            await (Task)sNonPattern2.StaticAsync();
            await (Task)sNonTypes1.StaticAsync();
            await (Task)sNonTypes2.StaticAsync();
            await (Task)sOther1.StaticAsync();
            await (Task)sOther2.StaticAsync();

            AssertPoolingResult(sNonPattern1, sNonPattern2, sNonTypes1, sNonTypes2, sOther1, sOther2);
        }

        [Fact]
        public async Task InstanceAsyncTest()
        {
            var sNonPattern1 = Assembly.GetStaticInstance(typeof(NonPattern1).FullName!, true);
            var sNonPattern2 = Assembly.GetStaticInstance(typeof(NonPattern2).FullName!, true);
            var sNonTypes1 = Assembly.GetStaticInstance(typeof(NonTypes1).FullName!, true);
            var sNonTypes2 = Assembly.GetStaticInstance(typeof(NonTypes2).FullName!, true);
            var sOther1 = Assembly.GetStaticInstance(typeof(Other1).FullName!, true);
            var sOther2 = Assembly.GetStaticInstance(typeof(Other2).FullName!, true);

            var nonPattern1 = Assembly.GetInstance(typeof(NonPattern1).FullName!, true);
            var nonPattern2 = Assembly.GetInstance(typeof(NonPattern2).FullName!, true);
            var nonTypes1 = Assembly.GetInstance(typeof(NonTypes1).FullName!, true);
            var nonTypes2 = Assembly.GetInstance(typeof(NonTypes2).FullName!, true);
            var other1 = Assembly.GetInstance(typeof(Other1).FullName!, true);
            var other2 = Assembly.GetInstance(typeof(Other2).FullName!, true);

            await (Task)nonPattern1.Async();
            await (Task)nonPattern2.Async();
            await (Task)nonTypes1.Async();
            await (Task)nonTypes2.Async();
            await (Task)other1.Async();
            await (Task)other2.Async();

            AssertPoolingResult(sNonPattern1, sNonPattern2, sNonTypes1, sNonTypes2, sOther1, sOther2);
        }

        [Fact]
        public void StaticIteratorTest()
        {
            var sNonPattern1 = Assembly.GetStaticInstance(typeof(NonPattern1).FullName!, true);
            var sNonPattern2 = Assembly.GetStaticInstance(typeof(NonPattern2).FullName!, true);
            var sNonTypes1 = Assembly.GetStaticInstance(typeof(NonTypes1).FullName!, true);
            var sNonTypes2 = Assembly.GetStaticInstance(typeof(NonTypes2).FullName!, true);
            var sOther1 = Assembly.GetStaticInstance(typeof(Other1).FullName!, true);
            var sOther2 = Assembly.GetStaticInstance(typeof(Other2).FullName!, true);

            ((IEnumerable<object?>)sNonPattern1.StaticIteraor()).ToArray();
            ((IEnumerable<object?>)sNonPattern2.StaticIteraor()).ToArray();
            ((IEnumerable<object?>)sNonTypes1.StaticIteraor()).ToArray();
            ((IEnumerable<object?>)sNonTypes2.StaticIteraor()).ToArray();
            ((IEnumerable<object?>)sOther1.StaticIteraor()).ToArray();
            ((IEnumerable<object?>)sOther2.StaticIteraor()).ToArray();

            AssertPoolingResult(sNonPattern1, sNonPattern2, sNonTypes1, sNonTypes2, sOther1, sOther2);
        }

        [Fact]
        public void InstanceIteratorTest()
        {
            var sNonPattern1 = Assembly.GetStaticInstance(typeof(NonPattern1).FullName!, true);
            var sNonPattern2 = Assembly.GetStaticInstance(typeof(NonPattern2).FullName!, true);
            var sNonTypes1 = Assembly.GetStaticInstance(typeof(NonTypes1).FullName!, true);
            var sNonTypes2 = Assembly.GetStaticInstance(typeof(NonTypes2).FullName!, true);
            var sOther1 = Assembly.GetStaticInstance(typeof(Other1).FullName!, true);
            var sOther2 = Assembly.GetStaticInstance(typeof(Other2).FullName!, true);

            var nonPattern1 = Assembly.GetInstance(typeof(NonPattern1).FullName!, true);
            var nonPattern2 = Assembly.GetInstance(typeof(NonPattern2).FullName!, true);
            var nonTypes1 = Assembly.GetInstance(typeof(NonTypes1).FullName!, true);
            var nonTypes2 = Assembly.GetInstance(typeof(NonTypes2).FullName!, true);
            var other1 = Assembly.GetInstance(typeof(Other1).FullName!, true);
            var other2 = Assembly.GetInstance(typeof(Other2).FullName!, true);

            ((IEnumerable<object?>)nonPattern1.StaticIteraor()).ToArray();
            ((IEnumerable<object?>)nonPattern2.StaticIteraor()).ToArray();
            ((IEnumerable<object?>)nonTypes1.StaticIteraor()).ToArray();
            ((IEnumerable<object?>)nonTypes2.StaticIteraor()).ToArray();
            ((IEnumerable<object?>)other1.StaticIteraor()).ToArray();
            ((IEnumerable<object?>)other2.StaticIteraor()).ToArray();

            AssertPoolingResult(sNonPattern1, sNonPattern2, sNonTypes1, sNonTypes2, sOther1, sOther2);
        }

        [Fact]
        public async Task StaticAsyncIteratorTest()
        {
            var sNonPattern1 = Assembly.GetStaticInstance(typeof(NonPattern1).FullName!, true);
            var sNonPattern2 = Assembly.GetStaticInstance(typeof(NonPattern2).FullName!, true);
            var sNonTypes1 = Assembly.GetStaticInstance(typeof(NonTypes1).FullName!, true);
            var sNonTypes2 = Assembly.GetStaticInstance(typeof(NonTypes2).FullName!, true);
            var sOther1 = Assembly.GetStaticInstance(typeof(Other1).FullName!, true);
            var sOther2 = Assembly.GetStaticInstance(typeof(Other2).FullName!, true);

            await ((IAsyncEnumerable<object?>)sNonPattern1.StaticAsyncIteraor()).ToArrayAsync();
            await ((IAsyncEnumerable<object?>)sNonPattern2.StaticAsyncIteraor()).ToArrayAsync();
            await ((IAsyncEnumerable<object?>)sNonTypes1.StaticAsyncIteraor()).ToArrayAsync();
            await ((IAsyncEnumerable<object?>)sNonTypes2.StaticAsyncIteraor()).ToArrayAsync();
            await ((IAsyncEnumerable<object?>)sOther1.StaticAsyncIteraor()).ToArrayAsync();
            await ((IAsyncEnumerable<object?>)sOther2.StaticAsyncIteraor()).ToArrayAsync();

            AssertPoolingResult(sNonPattern1, sNonPattern2, sNonTypes1, sNonTypes2, sOther1, sOther2);
        }

        [Fact]
        public async Task InstanceAsyncIteratorTest()
        {
            var sNonPattern1 = Assembly.GetStaticInstance(typeof(NonPattern1).FullName!, true);
            var sNonPattern2 = Assembly.GetStaticInstance(typeof(NonPattern2).FullName!, true);
            var sNonTypes1 = Assembly.GetStaticInstance(typeof(NonTypes1).FullName!, true);
            var sNonTypes2 = Assembly.GetStaticInstance(typeof(NonTypes2).FullName!, true);
            var sOther1 = Assembly.GetStaticInstance(typeof(Other1).FullName!, true);
            var sOther2 = Assembly.GetStaticInstance(typeof(Other2).FullName!, true);

            var nonPattern1 = Assembly.GetInstance(typeof(NonPattern1).FullName!, true);
            var nonPattern2 = Assembly.GetInstance(typeof(NonPattern2).FullName!, true);
            var nonTypes1 = Assembly.GetInstance(typeof(NonTypes1).FullName!, true);
            var nonTypes2 = Assembly.GetInstance(typeof(NonTypes2).FullName!, true);
            var other1 = Assembly.GetInstance(typeof(Other1).FullName!, true);
            var other2 = Assembly.GetInstance(typeof(Other2).FullName!, true);

            await ((IAsyncEnumerable<object?>)nonPattern1.AsyncIteraor()).ToArrayAsync();
            await ((IAsyncEnumerable<object?>)nonPattern2.AsyncIteraor()).ToArrayAsync();
            await ((IAsyncEnumerable<object?>)nonTypes1.AsyncIteraor()).ToArrayAsync();
            await ((IAsyncEnumerable<object?>)nonTypes2.AsyncIteraor()).ToArrayAsync();
            await ((IAsyncEnumerable<object?>)other1.AsyncIteraor()).ToArrayAsync();
            await ((IAsyncEnumerable<object?>)other2.AsyncIteraor()).ToArrayAsync();

            AssertPoolingResult(sNonPattern1, sNonPattern2, sNonTypes1, sNonTypes2, sOther1, sOther2);
        }

        private void AssertPoolingResult(params dynamic[] items)
        {
            foreach (var item in items)
            {
                AssertPoolingResult(item.PoolingResult);
            }
        }

        private void AssertPoolingResult(dynamic poolingResult)
        {
            string[] unexpectedPooled = (string[])poolingResult.UnexpectedPooled;
            string[] unexpectedNotPooled = (string[])poolingResult.UnexpectedNotPooled;
            Assert.Empty(unexpectedPooled);
            Assert.Empty(unexpectedNotPooled);
        }
    }
}
