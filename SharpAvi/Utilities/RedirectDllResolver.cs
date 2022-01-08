using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;

namespace SharpAvi.Utilities
{
#if NET5_0_OR_GREATER
    internal static class RedirectDllResolver
    {
        private static readonly object sync = new();
        private static readonly Dictionary<string, string> redirects = new();

        static RedirectDllResolver()
        {
            NativeLibrary.SetDllImportResolver(Assembly.GetExecutingAssembly(), ResolveDllImport);
        }

        public static void SetRedirect(string libraryName, string targetLibraryName)
        {
            lock (sync)
            {
                redirects[libraryName] = targetLibraryName;
            }
        }

        private static IntPtr ResolveDllImport(string libraryName, Assembly assembly, DllImportSearchPath? searchPath)
        {
            string targetLibraryName;
            lock (sync)
            {
                if (!redirects.TryGetValue(libraryName, out targetLibraryName))
                {
                    // Fall back to default resolver
                    return IntPtr.Zero;
                }
            }
            return NativeLibrary.Load(targetLibraryName, assembly, searchPath);
        }
    }
#endif
}
