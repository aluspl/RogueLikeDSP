using System;
using System.CodeDom.Compiler;
using System.Collections;
using Enums;
using Interfaces;
using NUnit.Framework;

namespace MapGeneratorTest
{
    [TestFixture]
    public class MapGeneratorTest
    {
        private IMapGenerator _generator;

        [Test]
        public void IsGeneratorExist()
        {
            Assert.IsNotNull(_generator);
        }

        [Test]
        public void IsReturnMap()
        {
            int x = 10, y = 10;
            var map = new MapElement[x, y];
            Assert.IsNotNull(map);
        }

        [Test]
        public void IsArraySizeValid()
        {
            int x = 10, y = 10;
            var map = new MapElement[x, y];
            Assert.AreEqual(map.GetLength(0), 0);
        }

        [Test, TestCaseSource(typeof(SampleData), "SizeCases")]
        public int IsValidSize(int x, int y, int element)
        {

            var map = new MapElement[x, y];
            return map.GetLength(element);
        }

        [Test, TestCaseSource(typeof(SampleData), "GenerateWallSample")]
        public int GenerateWWallSize(int WallSize, int LeftSize)
        {
            return _generator.GetWallSize(ref LeftSize);
        }

        [TestFixtureSetUp]
        public void Configure()
        {
            _generator = new MapGenerator();
        }
    }

    public class SampleData
    {
        public static IEnumerable SizeCases
        {
            get
            {
                yield return new TestCaseData(10, 10, 0).Returns(0); //failed
                yield return new TestCaseData(10, 10, 0).Returns(10); //success
                yield return new TestCaseData(10, 10, 1).Returns(0);
                yield return new TestCaseData(10, 10, 1).Returns(10);

            }
        }


        public static IEnumerable GenerateWallSample
        {
            get
            {
                yield return new TestCaseData(10, 15).Returns(0); //failed
                yield return new TestCaseData(10, 15).Returns(10); //success
                yield return new TestCaseData(10, 15).Returns(0);
                yield return new TestCaseData(10, 15).Returns(10);
            }
        }

    }
}
