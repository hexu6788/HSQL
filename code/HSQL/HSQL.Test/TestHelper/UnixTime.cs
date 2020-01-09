using System;
using System.Collections.Generic;
using System.Text;

namespace HSQL.Test.TestHelper
{
    public class UnixTime
    {
        /// <summary>
        /// 将DateTime转换为Unix时间戳
        /// 说明：该Unix时间戳为 1970-1-1 08:00:00 到DateTime的秒数
        /// </summary>
        /// <returns>Unix时间戳</returns>
        public static long ToUnixTimeSecond(DateTime time)
        {
            var span = time - new DateTime(0x7b2, 1, 1, 8, 0, 0);
            return Convert.ToInt64(span.TotalSeconds);
        }

        /// <summary>
        /// 将Unix时间戳转换为DateTime时间
        /// 说明：该UnixTime时间戳应为 1970-1-1 08:00:00 到DateTime的秒数
        /// </summary>
        /// <returns>DateTime时间</returns>
        public static DateTime ToDateTime(long unixTime)
        {
            DateTime time2 = new DateTime(0x7b2, 1, 1, 8, 0, 0);
            return time2.AddSeconds(unixTime);
        }

        /// <summary>
        /// 将Unix时间戳转换成时间格式的字符串
        /// 说明：yyyy-MM-dd
        /// </summary>
        /// <returns>时间格式的字符串</returns>
        public static string ToDateTimeString(long unixTime)
        {
            if (unixTime != 0)
                return ToDateTime(unixTime).ToString("yyyy-MM-dd");
            else
                return string.Empty;
        }

        /// <summary>
        /// 将Unix时间戳转换成时间格式的字符串
        /// 说明：yyyy-MM-dd HH:mm:ss
        /// </summary>
        /// <returns>时间格式的字符串</returns>
        public static string ToDateTimeStringHasHourMinuteSecond(long unixTime)
        {
            if (unixTime != 0)
                return ToDateTime(unixTime).ToString("yyyy-MM-dd HH:mm:ss");
            else
                return string.Empty;
        }


    }
}
