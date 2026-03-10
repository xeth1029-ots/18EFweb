using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Versioning;

namespace WDAIIP.WEB.Commons
{
    public class RuntimeVer
    {
        /// <summary>
        /// .NET Framework 版本
        /// </summary>
        /// <returns></returns>
        public static string QueryDotnetVer()
        {
            string rst = "無法判斷 .NET 版本";
#if NETFRAMEWORK
            //Console.WriteLine($".NET Framework 版本: {Environment.Version}");
            rst = $".NET Framework 版本: {Environment.Version}";
#elif NETCOREAPP || NET
            //Console.WriteLine($".NET 版本: {System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}");
            rst = $".NET 版本: {System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription}";
#else
            //Console.WriteLine("無法判斷 .NET 版本");
            rst = "無法判斷 .NET 版本";
#endif
            return rst;
        }

        /// <summary>
        /// .NET Framework 版本
        /// </summary>
        /// <returns></returns>
        public static string GetCurrentDotNetVersion()
        {
            Version version = Environment.Version;
            return version.ToString();
        }

        /// <summary>
        /// .NET Framework 版本 也可以取得更詳細的資訊
        /// </summary>
        /// <returns></returns>
        public static string GetCurrentDotNetVersionDetails()
        {
            Version version = Environment.Version;
            return $"主版本: {version.Major}, 副版本: {version.Minor}, Build: {version.Build}, Revision: {version.Revision}";
        }


    }
}
