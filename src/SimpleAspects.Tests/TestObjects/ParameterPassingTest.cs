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

        void Test4Void();
        int Test4Int();
        string Test4String();

        void Teste5(out int v1, out string v2);
    }

    class ParameterPassingTest : IParameterPassingTest
    {
        public int PInt { get; private set; }
        public string PString { get; private set; }
        public DateTime PDateTime { get; private set; }
        public long PLong { get; private set; }
        public bool PBool { get; private set; }
        public int[] PArray { get; private set; }

        public ParameterPassingTest()
        {

        }
        public ParameterPassingTest(int pInt = 0, string pString = null, DateTime pDateTime = default(DateTime), long pLong = 0, bool pBool = false, int[] pArray = null)
        {
            this.PInt = pInt;
            this.PString = pString;
            this.PDateTime = pDateTime;
            this.PLong = pLong;
            this.PBool = pBool;
            this.PArray = pArray;
        }

        public bool Test(int p1, string p2, DateTime p3, long p4, bool p5, int[] p6)
        {
            return this.PInt == p1 && this.PString == p2 && this.PDateTime == p3 && this.PLong == p4 && this.PBool == p5 && this.PArray.SequenceEqual(p6 ?? new int[0]);
        }

        public void Test2(string p1, int p2)
        {
        }

        public void Test3()
        {
        }


        public void Test4Void()
        {
        }

        public int Test4Int()
        {
            return this.PInt;
        }

        public string Test4String()
        {
            return this.PString;
        }


        public void Teste5(out int v1, out string v2)
        {
            v1 = this.PInt;
            v2 = this.PString;
        }
    }   
}
