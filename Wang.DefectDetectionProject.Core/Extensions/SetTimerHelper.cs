using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wang.DefectDetectionProject.Core.Extensions
{
    public static class SetTimerHelper
    {
        /// <summary>
        /// 执行委托并返回其耗时
        /// </summary>
        /// <param name="action">要执行的委托</param>
        /// <returns>该委托执行的耗时</returns>
        public static double SetTimer(this Action action)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            action();
            stopwatch.Stop();
            return stopwatch.ElapsedMilliseconds;
        }
    }
}
