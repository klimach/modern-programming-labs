using System.Reflection;
using System.Runtime.Versioning;

namespace OnlineStore.App.Utils;

public static class FrameworkUtils
{
    public static int GetDotNetVersion()
    {
        var targetFramework = Assembly.GetEntryAssembly()!
            .GetCustomAttribute<TargetFrameworkAttribute>()!
            .FrameworkName;

        return new FrameworkName(targetFramework).Version.Major;
    }
}
