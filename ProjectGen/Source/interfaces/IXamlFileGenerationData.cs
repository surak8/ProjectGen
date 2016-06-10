using System.CodeDom;
using System.Xml;

namespace NSprojectgen {
    /// <summary>Interface describing XAML-file generation.</summary>
    internal interface IXamlFileGenerationData {
        /// <summary>The element-name of this class</summary>
        string elementName { get; }
        /// <summary>The base of the projectFileName for this class.</summary>
        string fileName { get; }
        /// <summary>The namespace for this class.</summary>
        string nameSpace { get; }

		/// <summary>Type of file being generated.</summary>
		GenFileType generationType { get; }

		/// <summary>blah</summary>
		bool generateViewModel { get; }

		/// <summary>read-write name of the XAML-file.</summary>
		string xamlName { get; set; }
        /// <summary>read-write name of the code-behind file.</summary>
        string codeBehindName { get; set; }
        /// <summary>read-write name of the view-model file.</summary>
        string viewModelName { get; set; }


		/// <summary>any imports required to support this class.</summary>
		/// <param name="ns"></param>
		void addImports(CodeNamespace ns);

        /// <summary>Add object-specific attributes to this XAML object.</summary>
        /// <param name="xw"></param>
        void populateElementAttributes(XmlWriter xw);

        /// <summary>Add content to this element.</summary>
        /// <param name="xw"></param>
        void populateElement(XmlWriter xw);

        /// <summary>Generate additional code, if desired.</summary>
        /// <param name="ns"></param>
        /// <param name="ctd"></param>
        /// <param name="cc"></param>
        void generateCode(CodeNamespace ns, CodeTypeDeclaration ctd, CodeConstructor cc);

        /// <summary>Generate cide for the view-model, if desired.</summary>
        /// <param name="ns"></param>
        /// <param name="ctd"></param>
        void generateModelCode(CodeNamespace ns, CodeTypeDeclaration ctd);

    }
}