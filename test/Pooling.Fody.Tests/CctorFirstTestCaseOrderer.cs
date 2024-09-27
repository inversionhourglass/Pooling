using System.Collections.Generic;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Pooling.Fody.Tests
{
    class CctorFirstTestCaseOrderer : ITestCaseOrderer
    {
        public IEnumerable<TTestCase> OrderTestCases<TTestCase>(IEnumerable<TTestCase> testCases) where TTestCase : ITestCase
        {
            var sortedTestCases = new List<TTestCase>(testCases);
            sortedTestCases.Sort((x, y) =>
            {
                if (x.TestMethod.Method.Name == nameof(SingleFeatureTests.StaticCtorTest))
                    return -1;
                if (y.TestMethod.Method.Name == nameof(SingleFeatureTests.StaticCtorTest))
                    return 1;
                return string.Compare(x.TestMethod.Method.Name, y.TestMethod.Method.Name);
            });
            return sortedTestCases;
        }
    }
}
