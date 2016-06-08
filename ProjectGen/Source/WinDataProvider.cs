using System;
using System.CodeDom;
using System.Xml;

namespace NSprojectgen {
    class WinDataProvider : IXamlFileGenerationData {
        #region ctor
        public WinDataProvider(string v, string nameSpace, bool isRegularWindow) {
            this.elementName = isRegularWindow ? "Window" : "NavigationWindow";
            this.fileName = v;
            this.nameSpace = nameSpace;
            this.isRegularWindow = isRegularWindow;
            homePage = isRegularWindow ? string.Empty : "Home.xaml";
        }
        #endregion

        #region properties
        public string homePage { get; private set; }
        internal bool isRegularWindow { get; private set; }
        #endregion

        #region IXamlFileGenerationData implementation
        #region properties
        public string elementName { get; private set; }
        public string fileName { get; private set; }
        public string nameSpace { get; private set; }
        public string codeBehindName { get; set; }
        public string viewModelName { get; set; }
        public string xamlName { get; set; }
        public bool generateViewModel { get { return true; } }
        #endregion

        #region methods
        void IXamlFileGenerationData.populateElement(XmlWriter xw) {
            if (this.isRegularWindow) {
                xw.WriteStartElement("Grid");
                xw.WriteEndElement();
            }
        }
        void IXamlFileGenerationData.populateElementAttributes(XmlWriter xw) {
            xw.WriteAttributeString("Name", XamlFileGenerator.NS_X, "window1");
            xw.WriteAttributeString("Class", XamlFileGenerator.NS_X, this.nameSpace + "." + this.fileName);
            xw.WriteAttributeString("Title", "MainWindow");
            xw.WriteAttributeString("Width", "350");
            xw.WriteAttributeString("Height", "525");
            if (!this.isRegularWindow) {
                xw.WriteAttributeString("Source", this.homePage);
            }
        }
        void IXamlFileGenerationData.addImports(CodeNamespace ns) {
            CodeNamespaceImport anImport;
            if (this.isRegularWindow)
                anImport = new CodeNamespaceImport("System.Windows");
            else
                anImport = new CodeNamespaceImport("System.Windows.Navigation");
            ns.Imports.Add(anImport);
        }
        void IXamlFileGenerationData.generateModelCode(CodeNamespace ns, CodeTypeDeclaration ctd) { }
        void IXamlFileGenerationData.generateCode(CodeNamespace ns, CodeTypeDeclaration ctd, CodeConstructor cc) { }
        #endregion
        #endregion
    }
}