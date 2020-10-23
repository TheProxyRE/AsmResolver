using System;
using System.IO;
using System.Runtime.InteropServices;

namespace AsmResolver.DotNet
{
    /// <summary>
    /// Provides an implementation of an assembly resolver that includes .NET Core runtime libraries. 
    /// </summary>
    public class NetCoreAssemblyResolver : AssemblyResolverBase
    {
        private readonly string _runtimeDirectory;
        
        /// <summary>
        /// Creates a new .NET Core assembly resolver, by attempting to autodetect the current .NET Core installation
        /// directory.
        /// </summary>
        public NetCoreAssemblyResolver()
            : this (RuntimeInformation.FrameworkDescription.Contains("Core")
                ? Path.GetDirectoryName(typeof(object).Assembly.Location)
                : null)
        {
        }

        /// <summary>
        /// Creates a new .NET Core assembly resolver.
        /// </summary>
        /// <param name="runtimeDirectory">The full path to the directory containing the runtime dlls.</param>
        public NetCoreAssemblyResolver(string runtimeDirectory)
            => _runtimeDirectory = runtimeDirectory;

        /// <summary>
        /// Creates a new .NET Core assembly resolver.
        /// </summary>
        /// <param name="runtimeName">The full name of the target runtime.</param>
        /// <param name="version">The version string of the target runtime.</param>
        public NetCoreAssemblyResolver(string runtimeName, string version) 
            : this(Path.Combine(FindRuntimeBaseDirectory(), runtimeName, version))
        {
        }

        /// <summary>
        /// Creates a new .NET Core assembly resolver.
        /// </summary>
        /// <param name="runtimeBaseDirectory">The installation directory of .NET Core.</param>
        /// <param name="runtimeName">The full name of the target runtime.</param>
        /// <param name="version">The version string of the target runtime.</param>
        public NetCoreAssemblyResolver(string runtimeBaseDirectory, string runtimeName, string version)
            : this(Path.Combine(runtimeBaseDirectory, runtimeName,version))
        {
        }

        private static string FindRuntimeBaseDirectory()
        {
            throw new NotSupportedException();
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