using System;
using System.CodeDom;
using System.IO;
using System.Xml;
using NSprojectgen;

class HomeDataProvider : IXamlFileGenerationData {
    #region ctor
    public HomeDataProvider(string aHomePage, string projectNamespace) {
        if (string.IsNullOrEmpty(aHomePage))
            throw new ArgumentNullException("aHomePage", "home-page value is null");
        this.homePage = Path.GetFileNameWithoutExtension(aHomePage);
        this.fileName = this.homePage;
        elementName = "Page";
        this.nameSpace = projectNamespace;
    }
    #endregion

    #region properties
    public string homePage { get; private set; }
    #endregion

    #region IXamlFileGenerationData implementation
    public string codeBehindName { get; set; }
    public string elementName { get; }
    public string fileName { get; }
    public string nameSpace { get; }
    public string viewModelName { get; set; }
    public string xamlName { get; set; }
    public bool generateViewModel { get { return true; } }

    void IXamlFileGenerationData.addImports(CodeNamespace ns) {
        ns.Imports.Add(new CodeNamespaceImport("System"));
        ns.Imports.Add(new CodeNamespaceImport("System.Windows"));
        ns.Imports.Add(new CodeNamespaceImport("System.Windows.Controls"));
    }
    void IXamlFileGenerationData.populateElement(XmlWriter xw) {

        /*
        < Grid >
            < Button x: Name = "button" Content = "Button" HorizontalAlignment = "Left" Height = "48"  VerticalAlignment = "Top" Width = "143" Click = "button_Click" />
        </ Grid >
        */
        xw.WriteStartElement("Grid");
        xw.WriteStartElement("Button");
        xw.WriteAttributeString("Name", XamlFileGenerator.NS_X, "btnNext");
        xw.WriteAttributeString("Click", NEXT_METHOD_NAME);
        xw.WriteAttributeString("Content", "Next");
        xw.WriteEndElement();
        xw.WriteEndElement();
    }
    void IXamlFileGenerationData.populateElementAttributes(XmlWriter xw) {
        xw.WriteAttributeString("Name", XamlFileGenerator.NS_X, "zzz");
        xw.WriteAttributeString("Class", XamlFileGenerator.NS_X, this.nameSpace + "." + this.fileName);
    }
    void IXamlFileGenerationData.generateModelCode(CodeNamespace ns, CodeTypeDeclaration ctd) { }
    void IXamlFileGenerationData.generateCode(CodeNamespace ns, CodeTypeDeclaration ctd, CodeConstructor cc) {
        ctd.Members.Add(createNextButtonClick("Page0.xaml"));
    }
	#endregion
	GenFileType IXamlFileGenerationData.generationType { get { return GenFileType.View; } }

	#region constants
	const string NEXT_METHOD_NAME = "nextClicked";
    #endregion

    #region methods
    CodeMemberMethod createNextButtonClick(string nextPage) {
        CodeMemberMethod ret = new CodeMemberMethod();

        CodeArgumentReferenceExpression ar0, ar1;
        ar0 = new CodeArgumentReferenceExpression("sender");
        ar1 = new CodeArgumentReferenceExpression("e");
        ret.Parameters.AddRange(new CodeParameterDeclarationExpression[] {
                new CodeParameterDeclarationExpression(typeof(object),ar0.ParameterName),
                new CodeParameterDeclarationExpression("RoutedEventArgs",ar1.ParameterName),

            });
        ret.Name = NEXT_METHOD_NAME;
        ret.Attributes = 0;
        ret.Statements.Add(
            new CodeMethodInvokeExpression(
                    new CodeMethodReferenceExpression(new CodeTypeReferenceExpression("NavigationService"), "Navigate"),
                    new CodeObjectCreateExpression(new CodeTypeReference("Uri"),
                        new CodePrimitiveExpression(nextPage),
                        new CodeFieldReferenceExpression(new CodeTypeReferenceExpression("UriKind"), "RelativeOrAbsolute"))));
        return ret;
    }

    #endregion
}