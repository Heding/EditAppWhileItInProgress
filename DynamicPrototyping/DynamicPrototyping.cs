using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CSharp;

namespace DynamicPrototyping
{
    public class DynamicCompiler
    {
        internal Assembly JitCompile(string sourceCode, string compilePlatformVersion = "v4.5")
        {
            CompilerParameters cp = PrepareCompileParameters();

            Dictionary<string, string> temp = new Dictionary<string, string> { { "CompileVersion", compilePlatformVersion } };

            CSharpCodeProvider provider = new CSharpCodeProvider();
            System.CodeDom.Compiler.CompilerResults result = provider.CompileAssemblyFromSource(cp, new[] { sourceCode });
            //System.CodeDom.Compiler.CompilerResults
            if (result.Errors.HasErrors) 
            {
                StringBuilder builder = new StringBuilder();
                foreach (var e in result.Errors) 
                {
                    builder.Append(e.ToString());
                }
                throw new Exception(builder.ToString());
            }

            return result.CompiledAssembly;

        }

        private CompilerParameters PrepareCompileParameters()
        {
            var compileParameters = new CompilerParameters { GenerateInMemory = true,GenerateExecutable=true};

            foreach(var reference in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    compileParameters.ReferencedAssemblies.Add(reference.Location);
                }
                catch(Exception e)
                {
                    Debug.WriteLine(e.Message);
                }
            }

            return compileParameters;
        }

        private object CompileAndCreateInstanceOfType(string sourceCode, string compilePlatformVersion = "v4.5") 
        {
            Type baseType = CompileAndGetType(sourceCode, compilePlatformVersion);
            return Activator.CreateInstance(baseType);
        }

        private Type CompileAndGetType(string sourceCode, string compilePlatformVersion = "v4.5") 
        {
            var assembly = JitCompile(sourceCode, compilePlatformVersion);
            var allTypes = assembly.GetTypes();
            if (allTypes == null || allTypes.Length == 0)
            {
                throw new Exception("no types");
            }
            else if (allTypes.Length != 1) 
            {
                throw new Exception("you must have one type in you assembly");
            }
            else
            {
                return allTypes[0];
            }
        }
    }
}
