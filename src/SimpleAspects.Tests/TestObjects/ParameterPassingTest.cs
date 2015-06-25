using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Simple.Tests
{
    public interface IParameterPassingTest
    {
        [TestAspect]
        bool Test(int p1, string p2, DateTime p3, long p4, bool p5, int[] p6);

        [TestAspect]
        [TestAspect2]
        void Test2(string p1, int p2);

        void Test3();
    }


    class ParameterPassingTest : IParameterPassingTest
    {
        public int P1 { get; private set; }
        public string P2 { get; private set; }
        public DateTime P3 { get; private set; }
        public long P4 { get; private set; }
        public bool P5 { get; private set; }
        public int[] P6 { get; private set; }

        public ParameterPassingTest()
        {

        }
        public ParameterPassingTest(int p1, string p2, DateTime p3, long p4, bool p5, int[] p6)
        {
            this.P1 = p1;
            this.P2 = p2;
            this.P3 = p3;
            this.P4 = p4;
            this.P5 = p5;
            this.P6 = p6;
        }

        public bool Test(int p1, string p2, DateTime p3, long p4, bool p5, int[] p6)
        {
            return this.P1 == p1 && this.P2 == p2 && this.P3 == p3 && this.P4 == p4 && this.P5 == p5 && this.P6.SequenceEqual(p6);
        }

        public void Test2(string p1, int p2)
        {
        }

        public void Test3()
        {
        }
    }   
}
