using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Xml;

namespace NSprojectgen {
	static class XamlGenerator {
		const string WIN_NAME = "MainWindow";

		internal static void generateFiles(Project p, PGOptions opts1, ProjectItemGroupElement pige, CodeDomProvider cdp, PGOptions opts2) {
			Dictionary<string, string> tmp = new Dictionary<string, string>();
			WinDataProvider wdp = new WinDataProvider(WIN_NAME, opts1.projectNamespace, opts2.xamlType == XamlWindowType.RegularWindow);
			AppDataProvider apd = new AppDataProvider(wdp.fileName, opts1.projectNamespace);
			string tmp2;

			XamlFileGenerator.generateFile(apd, false);
			XamlFileGenerator.generateFile(wdp, true);

			if (!string.IsNullOrEmpty(tmp2 = wdp.viewModelName) && System.IO.File.Exists(tmp2)) {
				pige.AddItem("Compile", tmp2);
				Debug.WriteLine("added: " + tmp2);
			}

			tmp.Add("Generator", "MSBuild:Compile");
			tmp.Add("SubType", "Designer");
			pige.AddItem("ApplicationDefinition", apd.xamlName, tmp);
			pige.AddItem("Page", wdp.xamlName, tmp);

			tmp.Clear();
			tmp.Add("DependentUpon", apd.xamlName);
			tmp.Add("SubType", "Code");
			pige.AddItem("Compile", apd.codeBehindName, tmp);
			Debug.WriteLine("added: " + apd.codeBehindName);

			tmp.Clear();
			tmp.Add("DependentUpon", wdp.xamlName);
			tmp.Add("SubType", "Code");
			pige.AddItem("Compile", wdp.codeBehindName, tmp);
			Debug.WriteLine("added: " + wdp.codeBehindName);

			if (!wdp.isRegularWindow) {
				pige.AddItem("Compile", wdp.homePage);
				Debug.WriteLine("added: " + wdp.homePage);

			}
		}
	}

	class WinDataProvider : IXamlFileGenerationData {
		public string homePage { get; private set; }

		public WinDataProvider(string v, string nameSpace, bool isRegularWindow) {
			this.elementName = isRegularWindow ? "Window" : "NavigationWindow";
			this.fileName = v;
			this.nameSpace = nameSpace;
			this.isRegularWindow = isRegularWindow;
			homePage = isRegularWindow ? string.Empty : "Home.xaml";
		}

		internal bool isRegularWindow { get; private set; }

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
				xw.WriteAttributeString("Source",this.homePage);
			}
		}

		public void addImports(CodeNamespace ns) {
			CodeNamespaceImport anImport;
			if (this.isRegularWindow)
				anImport = new CodeNamespaceImport("System.Windows");
			else
				anImport = new CodeNamespaceImport("System.Windows.Navigation");
			ns.Imports.Add(anImport);
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

		public string codeBehindName { get; set; }
		public string viewModelName { get; set; }
		public string xamlName { get; set; }
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

		void IXamlFileGenerationData.addImports(CodeNamespace ns) {
			Logger.log(MethodBase.GetCurrentMethod());

		}
		#endregion
		#endregion
	}
}