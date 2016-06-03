using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Xml;
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

            WinDataProvider wdp = new WinDataProvider("MainWindow", ns);

            AppDataProvider apd = new AppDataProvider(wdp.fileName, ns);
            XamlFileGenerator.generateFile(apd, false);
            XamlFileGenerator.generateFile(wdp, true);
            //        Debug.Print("here");
            string tmp2;
            if (!string.IsNullOrEmpty(tmp2 = wdp.viewModelName) && System.IO.File.Exists(tmp2))
                pige.AddItem("Compile", tmp2);

        }
    }

    class WinDataProvider : IXamlFileGenerationData {
        public WinDataProvider(string v, string nameSpace) {
            this.elementName = "Window";
            this.fileName = v;
            this.nameSpace = nameSpace;
        }
        #region IXamlFileGenerationData implementation
        #region properties
        public string elementName { get; private set; }
        public string fileName { get; private set; }
        public string nameSpace { get; private set; }

        public string codeBehindName { get; set; }
        public string viewModelName { get; set; }
        public string xamlName { get; set; }
        #endregion

        #region methods
        void IXamlFileGenerationData.populateElement(XmlWriter xw) {
            xw.WriteStartElement("Grid");
            xw.WriteEndElement();

        }

        void IXamlFileGenerationData.populateElementAttributes(XmlWriter xw) {
            xw.WriteAttributeString("Name", XamlFileGenerator.NS_X, "window1");
            xw.WriteAttributeString("Class", XamlFileGenerator.NS_X, this.nameSpace + "." + this.elementName);
            xw.WriteAttributeString("Title", "MainWindow");
            xw.WriteAttributeString("Width", "350");
            xw.WriteAttributeString("Height", "525");

        }

        #endregion
        #endregion
    }

    class AppDataProvider : IXamlFileGenerationData {
        #region ctor
        public AppDataProvider(string winClass, string nameSpace) {
            windowClassName = winClass;
            elementName = "Application";
            fileName = "App";
            //       nameSpace = "NSTest";
            this.nameSpace = nameSpace;
        }
        #endregion

        #region properties
        public string windowClassName { get; private set; }
        #endregion

        #region IXamlFileGenerationData implementation
        #region properties
        public string elementName { get; private set; }
        public string fileName { get; private set; }
        public string nameSpace { get; private set; }

        string IXamlFileGenerationData.codeBehindName { get; set; }
        string IXamlFileGenerationData.viewModelName { get; set; }
        string IXamlFileGenerationData.xamlName { get; set; }
        #endregion

        #region methods
        void IXamlFileGenerationData.populateElement(XmlWriter xw) {
            xw.WriteStartElement(this.elementName + ".Resources");
            xw.WriteFullEndElement();
        }

        void IXamlFileGenerationData.populateElementAttributes(XmlWriter xw) {
            xw.WriteAttributeString("Class", XamlFileGenerator.NS_X, this.nameSpace + "." + this.elementName);
            xw.WriteAttributeString("StartupUri", this.windowClassName + ".xaml");
        }
        #endregion        
        #endregion
    }
}