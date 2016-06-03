using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;

namespace NSprojectgen {
    static class XamlGenerator {
        internal static void generateFiles(Project p, string asmName, string ns, ProjectItemGroupElement pige, CodeDomProvider cdp, CodeGeneratorOptions opts) {
            /*
             * need:
             * App.xaml
             * MainWindow.xaml
             * App.xaml.cs
             * MainWindow.xaml.cs
             * */
            Dictionary<string, string> tmp = new Dictionary<string, string>();

            tmp.Add("Generator", "MSBuild:Compile");
            tmp.Add("SubType", "Designer");
            pige.AddItem("ApplicationDefinition", "App.xaml", tmp);
            pige.AddItem("Page", "MainWindow.xaml", tmp);

            tmp.Clear();
            tmp.Add("DependentUpon", "App.xaml");
            tmp.Add("SubType", "Code");
            pige.AddItem("Compile", "App.xaml.cs", tmp);

            tmp.Clear();
            tmp.Add("DependentUpon", "MainWindow.xaml");
            tmp.Add("SubType", "Code");
            pige.AddItem("Compile", "MainWindow.xaml.cs", tmp);
        }
    }
}