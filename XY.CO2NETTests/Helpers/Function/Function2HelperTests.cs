using Microsoft.VisualStudio.TestTools.UnitTesting;
using XY.CO2NET.Helpers.Function;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XY.CO2NET.Helpers.Function.Tests
{
    [TestClass()]
    public class Function2HelperTests
    {
        [TestMethod()]
        public async Task DebounceTest()
        {

            Console.WriteLine("开始时间：" + DateTime.Now);

            var function2Helper = Function2Helper.Instance;
            var i = 0;
            while (i++ < 10)
            {
                //System.Threading.Thread.Sleep(5);
                Console.WriteLine("方法调用，时间：" + DateTime.Now.ToNow() + "当前方法：" + i);
                function2Helper.DebounceByTimer(2000, () =>
                {
                    Console.WriteLine("方法执行123，时间：" + DateTime.Now + "当前方法：" + i);
                });
            }
            await Task.Delay(3000);
            function2Helper.DebounceByTimer(10, () =>
            {
                Console.WriteLine("方法执行2，时间：" + DateTime.Now + "当前方法：" + i);
            });
            await Task.Delay(3000);
            //FunctionHelper.Debounce("",1000, () =>
            //{
            //    Console.WriteLine("方法执行2，时间：" + DateTime.Now + "当前方法：" + i);
            //});
            Assert.IsTrue(true);
        }
    }
}