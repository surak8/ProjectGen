using System;

namespace NSprojectgen {
	class PGOptions {

		#region constants
		public const string LISTENER_NAME = "dummy";
		#endregion

		#region fields
		bool explicitNamespace;
		#endregion

		#region ctor
		public PGOptions() {
			projectType = ProjectType.ConsoleApp;
			this.usePhibroStyle = false;
			assemblyName = LISTENER_NAME;
			projectFileName = assemblyName + ".csproj";
			assemblyVersion = "1.0.0.0";
			calculateNamespace();
			xamlType = XamlWindowType.NONE;
		}
		#endregion

		#region properties
		public string assemblyName { get; set; }
		public ProjectType projectType { get; set; }
		public bool doDevExpress { get; set; }
		public bool simplyProject { get; set; }
		public bool generateCode { get; set; }
		public bool isCPPProject { get; set; }
		public bool usePhibroStyle { get; set; }
		public string projectFileName { get; set; }
		public string assemblyVersion { get; set; }
		public string projectNamespace { get; private set; }
		public XamlWindowType xamlType { get; set; }

		#endregion

		#region methods
		internal void calculateNamespace() {
			if (!explicitNamespace)
				projectNamespace = "NS" + assemblyName.Substring(0, 1).ToUpper() + assemblyName.Substring(1);
		}

		internal void setNamespace(string v) {
			projectNamespace = v;
			explicitNamespace = true;
		}

		#endregion
	}
	enum XamlWindowType {
		NONE=0,
		RegularWindow=1,
		NavigationWindow=2,
	}
}