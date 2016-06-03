using System.Xml;

namespace NSprojectgen {
    /// <summary>Interface describing XAML-file generation.</summary>
    public interface IXamlFileGenerationData {
        /// <summary>The element-name of this class</summary>
        string elementName { get; }
        /// <summary>The base of the filename for this class.</summary>
        string fileName { get; }
        /// <summary>The namespace for this class.</summary>
        string nameSpace { get; }
        /// <summary>read-write name of the XAML-file.</summary>
        string xamlName { get; set; }
        /// <summary>read-write name of the code-behind file.</summary>
        string codeBehindName { get; set; }
        /// <summary>read-write name of the view-model file.</summary>
        string viewModelName { get; set; }

        /// <summary>Add object-specific attributes to this XAML object.</summary>
        /// <param name="xw"></param>
        void populateElementAttributes(XmlWriter xw);

        /// <summary>Add content to this element.</summary>
        /// <param name="xw"></param>
        void populateElement(XmlWriter xw);
    }
}