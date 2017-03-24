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
            var map =  new MapElement[x,y];
            Assert.IsNotNull(map);
        }
        [Test]
        public void IsArraySizeValid()
        {
            int x = 10, y = 10;
            var map =  new MapElement[x,y];
                Assert.AreEqual(map.GetLength(0), 0);
        }
        [Test, TestCaseSource(typeof(SampleData),"SizeCases")]
        public int IsValidSize(int x, int y, int element)
        {

            var map =  new MapElement[x,y];
            return map.GetLength(element);
        }

        [TestFixtureSetUp]
        public void Configure()
        {
            _generator=new MapGenerator();
        }
    }

    public class SampleData
    {
        public static IEnumerable SizeCases
        {
            get
            {
                yield return new TestCaseData( 10, 10,0 ).Returns( 0 ); //failed
                yield return new TestCaseData( 12, 2 ,0).Returns( 12 ); //success
                yield return new TestCaseData( 12, 4,1).Returns( 0 );
                yield return new TestCaseData( 12, 4,1).Returns( 4 );

            }
        }

    }
}