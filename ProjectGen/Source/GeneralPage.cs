using System.CodeDom;
using System.Xml;

namespace NSprojectgen {
    class GeneralPage : IXamlFileGenerationData {
        #region constants
        const string PREV_BUTTON_NAME = "btnPrev";
        const string NEXT_BUTTON_NAME = "btnNext";
        const string PREV_METHOD_NAME = "prevClicked";
        const string NEXT_METHOD_NAME = "nextClicked";
        #endregion

        #region ctor
        public GeneralPage(string aPageName, string projectNamespace) {
            this.fileName = this.pageName = aPageName;
            this.filenamespace = projectNamespace;
            this.elementName = "Page";
            this.generateViewModel = true;
            this.nameSpace = projectNamespace;
        }

        #endregion

        #region IXamlFileGenerationData implementation
        #region IXamlFileGenerationData properties
        public string codeBehindName { get; set; }
        public string elementName { get; }
        public string fileName { get; }
        public string filenamespace { get; private set; }
        public bool generateViewModel { get; set; }
        public string nameSpace { get; private set; }
        public string pageName { get; private set; }
        public string viewModelName { get; set; }
        public string xamlName { get; set; }
        #endregion

        #region IXamlFileGenerationData properties
        void IXamlFileGenerationData.addImports(CodeNamespace ns) {
            ns.Imports.Add(new CodeNamespaceImport("System"));
            ns.Imports.Add(new CodeNamespaceImport("System.Windows"));
            ns.Imports.Add(new CodeNamespaceImport("System.Windows.Controls"));
        }
        void IXamlFileGenerationData.populateElement(XmlWriter xw) {
            //            Logger.log(MethodBase.GetCurrentMethod());
            /*
             *   <DockPanel>
                    <Label Content="Page0" DockPanel.Dock="Top" HorizontalAlignment="Center" FontWeight="Bold"/>
                    <StackPanel Orientation="Horizontal" DockPanel.Dock="Bottom" HorizontalAlignment="Center">
                        <Button Content="Prev"   Width="50"/>
                        <Button Content="Next"   Width="50"/>
                    </StackPanel>
                    <TextBox Text="textbox" />
                </DockPanel>
             * */
            xw.WriteStartElement("DockPanel");

            xw.WriteStartElement("Label");
            xw.WriteAttributeString("Content", this.fileName);
            xw.WriteAttributeString("DockPanel.Dock", "Top");
            xw.WriteAttributeString("HorizontalAlignment", "Center");
            xw.WriteAttributeString("FontWeight", "Bold");
            xw.WriteEndElement();

            xw.WriteStartElement("StackPanel");
            xw.WriteAttributeString("DockPanel.Dock", "Bottom");
            xw.WriteAttributeString("HorizontalAlignment", "Center");
            xw.WriteAttributeString("Orientation", "Horizontal");

            xw.WriteStartElement("Button");
            xw.WriteAttributeString("Name", XamlFileGenerator.NS_X, PREV_BUTTON_NAME);
            xw.WriteAttributeString("Click", PREV_METHOD_NAME);
            xw.WriteAttributeString("Content", "Prev");
            xw.WriteAttributeString("Width", "50");
            xw.WriteEndElement();

            xw.WriteStartElement("Button");
            xw.WriteAttributeString("Name", XamlFileGenerator.NS_X, NEXT_BUTTON_NAME);
            xw.WriteAttributeString("Click", NEXT_METHOD_NAME);
            xw.WriteAttributeString("Content", "Next");
            xw.WriteAttributeString("Width", "50");
            xw.WriteEndElement();

            xw.WriteEndElement();

            xw.WriteStartElement("TextBox");
            xw.WriteAttributeString("Text", "textbox");
            xw.WriteEndElement();

            xw.WriteEndElement();
        }
        void IXamlFileGenerationData.populateElementAttributes(XmlWriter xw) {
            xw.WriteAttributeString("Name", XamlFileGenerator.NS_X, "zzz");
            xw.WriteAttributeString("Class", XamlFileGenerator.NS_X, this.nameSpace + "." + this.fileName);
            xw.WriteAttributeString("Width", "200");
            xw.WriteAttributeString("Height", "200");
        }
        void IXamlFileGenerationData.generateModelCode(CodeNamespace ns, CodeTypeDeclaration ctd) { }
        void IXamlFileGenerationData.generateCode(CodeNamespace ns, CodeTypeDeclaration ctd, CodeConstructor cc) {
            ctd.Members.Add(createNextButtonClick("PageN.xaml"));
            ctd.Members.Add(createPrfevButtonClick());
        }
		#endregion
		#endregion
		GenFileType IXamlFileGenerationData.generationType { get { return GenFileType.View; } }

		#region methods
		CodeMemberMethod createNextButtonClick(string nextPage) {
            CodeMemberMethod ret = new CodeMemberMethod();

            addParms(ret);
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

        static void addParms(CodeMemberMethod ret) {
            CodeArgumentReferenceExpression ar0, ar1;

            ar0 = new CodeArgumentReferenceExpression("sender");
            ar1 = new CodeArgumentReferenceExpression("e");
            ret.Parameters.AddRange(new CodeParameterDeclarationExpression[] {
                new CodeParameterDeclarationExpression(typeof(object),ar0.ParameterName),
                new CodeParameterDeclarationExpression("RoutedEventArgs",ar1.ParameterName),

            });
        }

        CodeMemberMethod createPrfevButtonClick() {
            CodeMemberMethod ret = new CodeMemberMethod();

            CodeExpression ceNav = new CodeTypeReferenceExpression("NavigationService");

            addParms(ret);
            ret.Name = PREV_METHOD_NAME;
            ret.Attributes = 0;

            ret.Statements.Add(
                new CodeConditionStatement(
                    new CodePropertyReferenceExpression(ceNav, "CanGoBack"),
                    new CodeExpressionStatement(
                        new CodeMethodInvokeExpression(ceNav, "GoBack"))));
            return ret;
        }

        #endregion
    }
}