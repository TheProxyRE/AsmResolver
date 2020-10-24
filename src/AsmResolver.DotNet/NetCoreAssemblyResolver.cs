using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Provides an implementation of an assembly resolver that includes .NET Core runtime libraries. 
    /// </summary>
    public class NetCoreAssemblyResolver : AssemblyResolverBase
    {
        private readonly string _runtimeDirectory;
        private static string[] CommonUnixDotnetRuntimePaths = new string[]
        {
            "/usr/share/dotnet/shared",
            "/opt/dotnet/shared/",
            "~/share/dotnet/shared",
        };

        /// <summary>
        /// Creates a new .NET Core assembly resolver, by attempting to autodetect the current .NET Core installation
        /// directory.
        /// </summary>
        public NetCoreAssemblyResolver()
            : this (null)
        {
        }

        /// <summary>
        /// Creates a new .NET Core assembly resolver.
        /// </summary>
        /// <param name="runtimeDirectory">The full path to the directory containing the runtime dlls.</param>
        public NetCoreAssemblyResolver(string runtimeDirectory) 
            => _runtimeDirectory = Directory.Exists(runtimeDirectory) 
            ? runtimeDirectory 
            : RuntimeInformation.FrameworkDescription.Contains("Core")
                ? Path.GetDirectoryName(typeof(object).Assembly.Location)
                : null;

        /// <summary>
        /// Creates a new .NET Core assembly resolver.
        /// </summary>
        /// <param name="runtimeName">The full name of the target runtime.</param>
        /// <param name="version">The version string of the target runtime.</param>
        public NetCoreAssemblyResolver(string runtimeName, string version) 
            : this(FindRuntimeBaseDirectory(), runtimeName, version)
        {
        }

        /// <summary>
        /// Creates a new .NET Core assembly resolver.
        /// </summary>
        /// <param name="runtimeBaseDirectory">The installation directory of .NET Core.</param>
        /// <param name="runtimeName">The full name of the target runtime.</param>
        /// <param name="version">The version string of the target runtime.</param>
        public NetCoreAssemblyResolver(string runtimeBaseDirectory, string runtimeName, string version)
            : this(GetSutablePath(Path.Combine(runtimeBaseDirectory, runtimeName),version))
        {
        }
        
        private static string FindRuntimeBaseDirectory()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                using var key64 = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\dotnet\Setup\InstalledVersions\x64");
                if (key64?.GetValue("InstallLocation") is string location64 && Directory.Exists(Path.Combine(location64, "shared")))
                    return Path.Combine(location64, "shared");
                using var key32 = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\dotnet\Setup\InstalledVersions\x86");
                if (key32?.GetValue("InstallLocation") is string location32 && Directory.Exists(Path.Combine(location32, "shared")))
                    return Path.Combine(location32,"shared");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                foreach (var commonPath in CommonUnixDotnetRuntimePaths)
                    if (Directory.Exists(commonPath))
                        return commonPath;
            }
            if (RuntimeInformation.FrameworkDescription.Contains("Core"))
                return Path.GetFullPath(Path.Combine(Path.GetDirectoryName(typeof(object).Assembly.Location), "../../"));
            return string.Empty;
        }

        private static string GetSutablePath(string runtimePath, string runtimeVersion)
        {
            var defaultResult = Path.Combine(runtimePath, runtimeVersion);
            //TODO: Allow rc versions :assing: @Washi
            if (!Version.TryParse(runtimeVersion,out var version))
                return defaultResult;
            var dirVersions = GetAllVersions(runtimePath);
            var greaterVersions = dirVersions
                .Where(v => v.Version >= version)
                .ToArray();
            if (greaterVersions.Length == 0)
                return defaultResult;
            var sameVersion = greaterVersions
                .Where(v => v.Version < new Version(version.Major, version.Minor + 1))
                .ToArray();
            if(sameVersion.Length != 0)
                return sameVersion.Max().Directory;
            return greaterVersions.Min().Directory;
        }

        private static IEnumerable<(Version Version, string Directory)> GetAllVersions(string runtimePath)
        {
            if (!Directory.Exists(runtimePath))
                return Array.Empty<(Version, string)>();
            var list = new List<(Version, string)>();
            foreach(var directory in Directory.GetDirectories(runtimePath))
            {
                //TODO: Allow rc versions :assing: @Washi
                if (!Version.TryParse(Path.GetFileName(directory), out var version))
                    continue;
                list.Add((version, directory));
            }
            return list;
        }

        /// <inheritdoc />
        protected override AssemblyDefinition ResolveImpl(AssemblyDescriptor assembly)
        {
            string path = null;
            
            var token = assembly.GetPublicKeyToken();
            if (token != null && !string.IsNullOrEmpty(_runtimeDirectory))
                path = ProbeDirectory(assembly, _runtimeDirectory);
            if (string.IsNullOrEmpty(path))
                path = ProbeSearchDirectories(assembly);

            AssemblyDefinition assemblyDef = null;
            try
            {
                assemblyDef = LoadAssemblyFromFile(path);
            }
            catch
            {
                // ignore any errors.
            }

            return assemblyDef;
        }
    }
}