using System;
using System.Linq;
using AsmResolver.DotNet.Signatures;
using AsmResolver.DotNet.TestCases.Methods;
using Xunit;

namespace AsmResolver.DotNet.Tests.Collections
{
    public class ParameterCollectionTest
    {
        private static MethodDefinition ObtainStaticTestMethod(string name)
        {
            var module = ModuleDefinition.FromFile(typeof(MultipleMethods).Assembly.Location);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(MultipleMethods));
            return type.Methods.First(m => m.Name == name);
        }
        
        private static MethodDefinition ObtainInstanceTestMethod(string name)
        {
            var module = ModuleDefinition.FromFile(typeof(InstanceMethods).Assembly.Location);
            var type = module.TopLevelTypes.First(t => t.Name == nameof(InstanceMethods));
            return type.Methods.First(m => m.Name == name);
        }

        [Fact]
        public void ReadEmptyParametersFromStaticMethod()
        {
            var method = ObtainStaticTestMethod(nameof(MultipleMethods.VoidParameterlessMethod));

            Assert.Empty(method.Parameters);
            Assert.Null(method.Parameters.ThisParameter);
        }

        [Fact]
        public void ReadSingleParameterFromStaticMethod()
        {
            var method = ObtainStaticTestMethod(nameof(MultipleMethods.SingleParameterMethod));

            Assert.Single(method.Parameters);
            Assert.Equal("intParameter", method.Parameters[0].Name);
            Assert.True(method.Parameters[0].ParameterType.IsTypeOf("System", "Int32"));
            Assert.Null(method.Parameters.ThisParameter);
        }

        [Fact]
        public void ReadMultipleParametersFromStaticMethod()
        {
            var method = ObtainStaticTestMethod(nameof(MultipleMethods.MultipleParameterMethod));

            Assert.Equal(new[]
            {
                "intParameter",
                "stringParameter",
                "typeDefOrRefParameter"
            }, method.Parameters.Select(p => p.Name));

            Assert.Equal(new[]
            {
                "System.Int32",
                "System.String",
                typeof(MultipleMethods).FullName
            }, method.Parameters.Select(p => p.ParameterType.FullName));
            
            Assert.Null(method.Parameters.ThisParameter);
        }

        [Fact]
        public void ReadEmptyParametersFromInstanceMethod()
        {
            var method = ObtainInstanceTestMethod(nameof(InstanceMethods.InstanceParameterlessMethod));
            Assert.Empty(method.Parameters);
            Assert.NotNull(method.Parameters.ThisParameter);
            Assert.Equal(nameof(InstanceMethods), method.Parameters.ThisParameter.ParameterType.Name);
        }

        [Fact]
        public void ReadSingleParameterFromInstanceMethod()
        {
            var method = ObtainInstanceTestMethod(nameof(InstanceMethods.InstanceSingleParameterMethod));
            Assert.Single(method.Parameters);
            Assert.Equal(new[]
            {
                "intParameter"
            }, method.Parameters.Select(p => p.Name));
            Assert.NotNull(method.Parameters.ThisParameter);
            Assert.Equal(nameof(InstanceMethods), method.Parameters.ThisParameter.ParameterType.Name);
        }

        [Fact]
        public void ReadMultipleParametersFromInstanceMethod()
        {
            var method = ObtainInstanceTestMethod(nameof(InstanceMethods.InstanceMultipleParametersMethod));
            Assert.Equal(new[]
            {
                "intParameter", 
                "stringParameter",
                "boolParameter"
                
            }, method.Parameters.Select(p => p.Name));
            
            Assert.Equal(new[]
            {
                "System.Int32",
                "System.String",
                "System.Boolean",
            }, method.Parameters.Select(p => p.ParameterType.FullName));
            
            Assert.NotNull(method.Parameters.ThisParameter);
            Assert.Equal(nameof(InstanceMethods), method.Parameters.ThisParameter.ParameterType.Name);
        }

        [Fact]
        public void ReadReturnTypeFromStaticParameterlessMethod()
        {
            var method = ObtainStaticTestMethod(nameof(MultipleMethods.VoidParameterlessMethod));
            Assert.True(method.Parameters.ReturnParameter.ParameterType.IsTypeOf("System", "Void"));
        }

        [Fact]
        public void UpdateReturnTypeFromStaticParameterlessMethodShouldThrow()
        {
            var method = ObtainStaticTestMethod(nameof(MultipleMethods.VoidParameterlessMethod));
            Assert.Throws<InvalidOperationException>(() => method.Parameters.ReturnParameter.ParameterType = method.Module.CorLibTypeFactory.Int32);
        }
        
    }
}