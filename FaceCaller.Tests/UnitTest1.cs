using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FaceCaller.Tests
{
    [TestClass]
    public class FCTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            FaceCaller ck = new FaceCaller();
            var res= System.IO.File.ReadAllText("mvplist.csv");
            ck.FillMVPPhotos(res);
        }
    }
}
