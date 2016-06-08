using System.CodeDom;
using System.Diagnostics;
using System.Reflection;
using System.Xml;

namespace NSprojectgen {
	class GeneralPage : IXamlFileGenerationData {
		//	private readonly string filenamespace;

		//		private readonly string filenamespace;
		#region ctor
		public GeneralPage(string aPageName, string projectNamespace) {
			//			this.aPageName = aPageName;
			//			this.projectNamespace = projectNamespace;///
			//		Debug.Print("here");
			this.pageName = aPageName;
			this.filenamespace = projectNamespace;
			Debug.Print("here");
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
		public void addImports(CodeNamespace ns) {
			Logger.log(MethodBase.GetCurrentMethod());
		}

		public void populateElement(XmlWriter xw) {
			Logger.log(MethodBase.GetCurrentMethod());
		}

		public void populateElementAttributes(XmlWriter xw) {
			Logger.log(MethodBase.GetCurrentMethod());
		}


		#endregion
		#endregion
	}
}