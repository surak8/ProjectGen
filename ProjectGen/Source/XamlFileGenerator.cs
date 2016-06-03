using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;
using Microsoft.CSharp;

namespace NSprojectgen {
    /// <summary></summary>
    public static class XamlFileGenerator {
        #region constants
        /// <summary></summary>
        public const string NS_X = "http://schemas.microsoft.com/winfx/2006/xaml";
        /// <summary></summary>
        public const string NS_DEFAULT = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
        /// <summary></summary>
        public const string NS_BLEND = "http://schemas.microsoft.com/expression/blend/2008";
        #endregion

        #region fields
        static XmlWriterSettings _xws;
        static readonly CodeExpression ceThis = new CodeThisReferenceExpression();
        static readonly CodeExpression ceNull = new CodePrimitiveExpression();
        static readonly CodeStatement csBlank = new CodeSnippetStatement();
        static readonly CodeExpression ceZero = new CodePrimitiveExpression(0);
        static readonly CodeExpression ceThree = new CodePrimitiveExpression(3);
        static readonly CodeExpression ceTrue = new CodePrimitiveExpression(true);
        #endregion

        #region properties
        static XmlWriterSettings settings {
            get {
                if (_xws == null) {
                    _xws = new XmlWriterSettings();
                    _xws.Indent = true;
                    _xws.IndentChars = "\t";
                    _xws.OmitXmlDeclaration = true;
                    _xws.NewLineOnAttributes = true;
                    _xws.NewLineHandling = NewLineHandling.None;
                }
                return _xws;
            }
        }
        #endregion

        /// <summary></summary>
        /// <param name="ixfgd"></param>
        public static void generateFile(IXamlFileGenerationData ixfgd) {
            generateFile(ixfgd, false);
        }

        /// <summary>do it.</summary>
        /// <param name="ixfgd"></param>
        /// <param name="generateViewModel"></param>
        public static void generateFile(IXamlFileGenerationData ixfgd, bool generateViewModel) {
            StringBuilder sb;
            CodeDomProvider cdp = new CSharpCodeProvider();
            CodeGeneratorOptions opts;
            string fname, ename, ns, modelName;
            if (ixfgd == null)
                throw new ArgumentNullException("ixfgd", "generation-object is null!");
            if (string.IsNullOrEmpty(ename = ixfgd.elementName))
                throw new ArgumentNullException("ename", "element-name is null!");
            if (string.IsNullOrEmpty(fname = ixfgd.fileName))
                if (string.IsNullOrEmpty(fname))
                    throw new ArgumentNullException("fname", "file-name is null!");
            ixfgd.xamlName = fname + ".xaml";
            ns = ixfgd.nameSpace;
            sb = new StringBuilder();
            using (StringWriter sw = new StringWriter(sb)) {
                using (XmlWriter xw = XmlWriter.Create(sw, settings)) {
                    xw.WriteStartElement(ename, NS_DEFAULT);
                    xw.WriteAttributeString("xmlns", "x", null, NS_X);
                    xw.WriteAttributeString("xmlns", "d", null, NS_BLEND);
                    if (!string.IsNullOrEmpty(ns))
                        xw.WriteAttributeString("xmlns", "local", null, "clr-namespace:" + ns);
                    ixfgd.populateElementAttributes(xw);
                    ixfgd.populateElement(xw);
                    xw.WriteEndDocument();
                }
            }
            Debug.Print(sb.ToString());
            File.WriteAllText(ixfgd.xamlName, sb.ToString());

            opts = new CodeGeneratorOptions();
            opts.BlankLinesBetweenMembers = false;
            opts.ElseOnClosing = true;

            ixfgd.codeBehindName = fname + ".xaml." + cdp.FileExtension;
            modelName = fname + "ViewModel";
            ixfgd.viewModelName = modelName + "." + cdp.FileExtension;
            createMainFile(ixfgd.codeBehindName, ns, fname, ename, cdp, modelName, opts, generateViewModel);
            if (generateViewModel)
                createModelfile(ixfgd.viewModelName, ns, modelName, cdp, opts);
        }

        static void createModelfile(string outModelName, string nameSpace, string modelName, CodeDomProvider cdp, CodeGeneratorOptions opts) {
            CodeCompileUnit ccu = null;
            CodeNamespace ns0, ns;
            CodeTypeDeclaration ctd;
            CodeConstructor cc;
            CodeMemberEvent cme0;
            CodeMemberMethod cmm0, cmm3;
            CodeEventReferenceExpression cere;

            ccu = new CodeCompileUnit();
            ccu.Namespaces.Add(ns = ns0 = new CodeNamespace());
            if (!string.IsNullOrEmpty(nameSpace))
                ccu.Namespaces.Add(ns = new CodeNamespace(nameSpace));

            ns0.Imports.Add(new CodeNamespaceImport("System.ComponentModel"));
            ns0.Imports.Add(new CodeNamespaceImport("System.Reflection"));
            ns.Types.Add(ctd = new CodeTypeDeclaration(modelName));
            if (cdp.Supports(GeneratorSupport.PartialTypes))
                ctd.IsPartial = true;

            ctd.BaseTypes.Add("INotifyPropertyChanged");
            cere = new CodeEventReferenceExpression(ceThis, "PropertyChanged");
            ctd.Members.AddRange(new CodeTypeMember[] {
                cme0=createEvent(cere),
                cc=new CodeConstructor(),
                cmm0=createMethod1("firePropertyChanged",cere,new CodeArgumentReferenceExpression ("propertyName")),
                cmm3=createMethod2("firePropertyChanged",cere,new CodeArgumentReferenceExpression ("mb"))
            }); ;
            cc.Attributes = MemberAttributes.Public;
            outputFile(ccu, ns, cdp, outModelName, opts);
        }

        static void createMainFile(string outCSName, string nameSpace, string fname, string ename, CodeDomProvider cdp, string modelName, CodeGeneratorOptions opts, bool generateViewModel) {
            CodeCompileUnit ccu = null;
            CodeNamespace ns0, ns;
            CodeTypeDeclaration ctd;
            CodeConstructor cc;
            CodeMemberField f;
            CodeFieldReferenceExpression fr = null;

            ccu = new CodeCompileUnit();
            ccu.Namespaces.Add(ns = ns0 = new CodeNamespace());
            if (!string.IsNullOrEmpty(nameSpace))
                ccu.Namespaces.Add(ns = new CodeNamespace(nameSpace));

            ns0.Imports.Add(new CodeNamespaceImport("System.Windows"));
            ns.Types.Add(ctd = new CodeTypeDeclaration(fname));
            if (cdp.Supports(GeneratorSupport.PartialTypes))
                ctd.IsPartial = true;
            ctd.BaseTypes.Add(ename);

            if (generateViewModel) {
                fr = new CodeFieldReferenceExpression(null, "_vm");
                ctd.Members.Add(f = new CodeMemberField(modelName, fr.FieldName));
                f.Attributes = 0;
                ctd.Members.Add(cc = new CodeConstructor());
                cc.Attributes = MemberAttributes.Public;
                cc.Statements.Add(new CodeAssignStatement(fr, new CodeObjectCreateExpression(modelName)));
                cc.Statements.Add(
                    new CodeExpressionStatement(
                        new CodeMethodInvokeExpression(null, "InitializeComponent", new CodeExpression[0])));
            }
            outputFile(ccu, ns, cdp, outCSName, opts);
        }

        static void outputFile(CodeCompileUnit ccu, CodeNamespace ns, CodeDomProvider cdp, string outModelName, CodeGeneratorOptions opts) {
            StringBuilder sb;

            using (TextWriter sw = new StringWriter(sb = new StringBuilder())) {
                if (ccu != null)
                    cdp.GenerateCodeFromCompileUnit(ccu, sw, opts);
                else
                    cdp.GenerateCodeFromNamespace(ns, sw, opts);
            }
            File.WriteAllText(outModelName, sb.ToString());
            Debug.Print(sb.ToString());
            sb.Clear();
            sb = null;
        }

        #region code-generation methods
        static CodeMemberEvent createEvent(CodeEventReferenceExpression cere) {
            CodeMemberEvent cme = new CodeMemberEvent();

            cme.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            cme.Type = new CodeTypeReference("PropertyChangedEventHandler");
            cme.Name = cere.EventName;
            return cme;
        }

        static CodeMemberMethod createMethod1(string methodName, CodeEventReferenceExpression cere, CodeArgumentReferenceExpression ar) {
            CodeMemberMethod ret = new CodeMemberMethod();

            /*
            * void firePropertyChanged(string v) {
            *      PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(v));
            * }
            * */

            ret.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            ret.Name = methodName;
            ret.Parameters.Add(new CodeParameterDeclarationExpression(typeof(string), ar.ParameterName));
            ret.Statements.Add(new CodeConditionStatement(
                new CodeBinaryOperatorExpression(cere, CodeBinaryOperatorType.IdentityInequality, ceNull),
                new CodeExpressionStatement(
                    new CodeDelegateInvokeExpression(cere, ceThis,
                        new CodeObjectCreateExpression("PropertyChangedEventArgs", ar)))));
            return ret;
        }

        static CodeMemberMethod createMethod2(string methodName, CodeEventReferenceExpression cere, CodeArgumentReferenceExpression ar2) {
            CodeMemberMethod ret = new CodeMemberMethod();
            CodeExpression ce4 = new CodePrimitiveExpression(4);
            CodePropertyReferenceExpression mbName = new CodePropertyReferenceExpression(ar2, "Name");
            CodeExpression mie0 = makeSubstr(mbName, ceZero, ceThree);
            CodeMethodReferenceExpression mr1 = new CodeMethodReferenceExpression(new CodeTypeReferenceExpression(typeof(string)), "Compare");
            CodeExpression ceStrComp2 = new CodeMethodInvokeExpression(mr1, mie0, new CodePrimitiveExpression("set"), ceTrue);
            CodeExpression ceStrComp1 = new CodeMethodInvokeExpression(mr1, mie0, new CodePrimitiveExpression("get"), ceTrue);
            CodePropertyReferenceExpression mbNameLen = new CodePropertyReferenceExpression(mbName, "Length");
            CodeVariableReferenceExpression vr = new CodeVariableReferenceExpression("n");

            /*
             * void firePropertyChanged(MethodBase mb) {
             *      Debug.Print("here");
             *      int n;
             *      if ((n = mb.Name.Length) > 4) {
             *          if (string.Compare(mb.Name.Substring(0, 3), "set", true) == 0 ||
             *              string.Compare(mb.Name.Substring(0, 3), "get", true) == 0)
             *              firePropertyChanged(mb.Name.Substring(4));
             *      }
             * }
             * */
            ret.Parameters.Add(new CodeParameterDeclarationExpression("MethodBase", ar2.ParameterName));
            ret.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            ret.Name = methodName;
            ret.Statements.AddRange(new CodeStatement[] {
                new CodeVariableDeclarationStatement (typeof(int),vr.VariableName),
                csBlank,
                new CodeConditionStatement (
                    new CodeBinaryOperatorExpression (
                        new CodeBinaryOperatorExpression(vr,CodeBinaryOperatorType.Assign ,mbNameLen),CodeBinaryOperatorType.GreaterThan ,ce4),
                            new CodeConditionStatement(new CodeBinaryOperatorExpression(eq(ceStrComp1, ceZero) ,  CodeBinaryOperatorType.BooleanOr ,eq(ceStrComp2,ceZero)),
                                new CodeExpressionStatement(new CodeMethodInvokeExpression (null,ret.Name,makeSubstr (mbName,ce4))))),
            });
            return ret;
        }

        static CodeExpression eq(CodeExpression ceLeft, CodeExpression ceRight) {
            return new CodeBinaryOperatorExpression(ceLeft, CodeBinaryOperatorType.IdentityEquality, ceRight);
        }

        static CodeExpression ne(CodeExpression ceLeft, CodeExpression ceRight) {
            return new CodeBinaryOperatorExpression(ceLeft, CodeBinaryOperatorType.IdentityInequality, ceRight);
        }

        static CodeExpression binOp(CodeExpression ceLeft, CodeBinaryOperatorType type, CodeExpression ceRight) {
            return new CodeBinaryOperatorExpression(ceLeft, type, ceRight);
        }

        static CodeExpression makeSubstr(CodeExpression ceTarget, CodeExpression ceLen) {
            return new CodeMethodInvokeExpression(ceTarget, "Substring", ceLen);
        }

        static CodeExpression makeSubstr(CodeExpression ceTarget, CodeExpression ceStart, CodeExpression ceLen) {
            return new CodeMethodInvokeExpression(ceTarget, "Substring", ceStart, ceLen);
        }

        #endregion

    }
}