using System.CodeDom;
using System.Xml;

namespace NSprojectgen {
    class AppDataProvider : IXamlFileGenerationData {
        #region ctor
        public AppDataProvider(string winClass, string nameSpace) {
            windowClassName = winClass;
            elementName = "Application";
            fileName = "App";
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
        public string codeBehindName { get; set; }
        public string viewModelName { get; set; }
        public string xamlName { get; set; }
        public bool generateViewModel { get { return false; } }
        #endregion

        #region methods
        void IXamlFileGenerationData.populateElement(XmlWriter xw) {
            xw.WriteStartElement(this.elementName + ".Resources");
            xw.WriteFullEndElement();
        }
        void IXamlFileGenerationData.populateElementAttributes(XmlWriter xw) {
            xw.WriteAttributeString("Class", XamlFileGenerator.NS_X, this.nameSpace + "." + this.elementName);
            xw.WriteAttributeString("StartupUri", "/Source/Views/"+this.windowClassName + ".xaml");
        }
        void IXamlFileGenerationData.addImports(CodeNamespace ns) { }
        void IXamlFileGenerationData.generateModelCode(CodeNamespace ns, CodeTypeDeclaration ctd) { }
        void IXamlFileGenerationData.generateCode(CodeNamespace ns, CodeTypeDeclaration ctd, CodeConstructor cc) { }
		#endregion
		#endregion
		GenFileType IXamlFileGenerationData.generationType { get { return GenFileType.View; } }

	}
}