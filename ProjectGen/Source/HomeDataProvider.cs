using NSprojectgen;
using System;
using System.CodeDom;
using System.IO;
using System.Reflection;
using System.Xml;

class HomeDataProvider : IXamlFileGenerationData {
	//        private string homePage;
	//      private string projectNamespace;
	public string homePage { get; private set; }
	public HomeDataProvider(string aHomePage, string projectNamespace) {
		if (string.IsNullOrEmpty(aHomePage))
			throw new ArgumentNullException("aHomePage", "home-page value is null");
		this.homePage = Path.GetFileNameWithoutExtension(aHomePage);
		this.fileName = this.homePage;
		elementName = "Page";
		this.nameSpace = projectNamespace;
	}

	public string codeBehindName { get; set; }
	public string elementName { get; }
	public string fileName { get; }
	public string nameSpace { get; }
	public string viewModelName { get; set; }
	public string xamlName { get; set; }

	public bool generateViewModel { get { return true; } }

	public void addImports(CodeNamespace ns) {
		ns.Imports.Add(new CodeNamespaceImport("System.Windows.Controls"));
	}

	public void populateElement(XmlWriter xw) {
		Logger.log(MethodBase.GetCurrentMethod());
	}

	public void populateElementAttributes(XmlWriter xw) {
		xw.WriteAttributeString("Name", XamlFileGenerator.NS_X, "zzz");
		xw.WriteAttributeString("Class", XamlFileGenerator.NS_X, this.nameSpace + "." + this.fileName);
	}
}
namespace NSprojectgen {
}