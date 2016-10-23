using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using YTY;

namespace YTY.Test
{
  [TestClass]
  public class WebClientExTest
  {
    [TestMethod]
    public void TestMethod1()
    {
      var wc= new WebClientEx();
      wc.AddRange(101, 200);
      wc.Timeout = 1000;
      Assert.AreEqual( wc.DownloadData("http://www.baidu.com/").Length ,100);
    }
  }
}
