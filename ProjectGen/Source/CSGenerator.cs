using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using System.IO;
using System.CodeDom;
using System.Text;
using System;
using System.Diagnostics;
namespace NSprojectgen {
    class CSGenerator {

#if false
        internal static void generateFiles(Project p,ProjectItemGroupElement pige2,string aFile,string asmName,string szVersion,ProjectType type,string ns) {

            using (CodeDomProvider cdp = new CSharpCodeProvider()) {
                CodeGeneratorOptions opts = new CodeGeneratorOptions();
                createAsmInfoFile(aFile,asmName,szVersion,cdp,opts);
                generateMainFiles(p,asmName,type,pige2,cdp,opts,ns);
            }
        }
#endif

        public static void createAsmInfoFile(string aFile, string aName, string szVersion, CodeDomProvider cdp, CodeGeneratorOptions opts) {
            string tmp, dir;

            if (Path.IsPathRooted(aFile))
                tmp = aFile;
            else {
                tmp = Path.Combine(Directory.GetCurrentDirectory(), aFile);
            }
            if (!Directory.Exists(dir = Path.GetDirectoryName(tmp)))
                Directory.CreateDirectory(dir);
            using (TextWriter tw = new StreamWriter(tmp)) {
                cdp.GenerateCodeFromCompileUnit(createCompileUnit(aName, szVersion, cdp, opts), tw, opts);
            }
        }

        static CodeCompileUnit createCompileUnit(string aName, string szVersion, CodeDomProvider cdp, CodeGeneratorOptions opts) {
            var v = new CodeSnippetCompileUnit();
            StringBuilder sb;
            StringWriter sw = new StringWriter(sb = new StringBuilder());
            CodeNamespace ns = new CodeNamespace();

            ns.Imports.AddRange(
                new CodeNamespaceImport[] {
                      new CodeNamespaceImport ("System.Reflection"),
                      new CodeNamespaceImport ("System.Runtime.InteropServices"),
              });

            cdp.GenerateCodeFromNamespace(ns, sw, opts);

            const string COMPANY = "Phibro Trading LLC.";

            var v2 = new CodeSnippetTypeMember(
                string.Join("\n",
                    "[assembly:AssemblyTitle(\"" + aName + "\")]",
                    "[assembly:AssemblyProduct(\"" + aName + "\")]",
                    "[assembly:AssemblyDescription(\"description of " + aName + ".\")]",
                    "[assembly:AssemblyCompany(\"" + COMPANY + "\")]",
                    "[assembly:AssemblyCopyright(\"Copyright � " + DateTime.Now.Year.ToString() + ", " + COMPANY + "\")]",
                    "#if DEBUG",
                    "[assembly:AssemblyConfiguration(\"Debug version\")]",
                    "#else",
                    "[assembly:AssemblyConfiguration(\"Release version\")]",
                    "#endif",
                    "[assembly:ComVisible(false)]",
                    null,
                    "[assembly:AssemblyVersion(\"" + szVersion + "\")]",
                    "[assembly:AssemblyFileVersion(\"" + szVersion + "\")]",
                    "[assembly:AssemblyInformationalVersion(\"" + szVersion + "\")]"));
            cdp.GenerateCodeFromMember(v2, sw, opts);
            sw.Flush();
            sw.Close();
            sw.Dispose();
            sw = null;
            v.Value = sb.ToString();
            return v;
        }

        internal static void generateMainFiles(Project p, string asmName, ProjectType type, ProjectItemGroupElement pige2, CodeDomProvider cdp, CodeGeneratorOptions opts, string ns, bool devExpress) {
            switch (type) {
                case ProjectType.WindowsForm:
                    generateForms(pige2, cdp, opts, ns, asmName, devExpress);
                    break;
                case ProjectType.ConsoleApp:
                    generateMain(pige2, cdp, opts, ns, asmName);
                    break;
                case ProjectType.ClassLibrary:
                    generateLib(pige2, cdp, opts, ns, asmName);
                    break;
                default: throw new InvalidOperationException("unhandled " + type.GetType().Name + ": " + type);
            }
        }

        static void generateLib(ProjectItemGroupElement pige2, CodeDomProvider cdp, CodeGeneratorOptions opts, string ns, string asmName) {
            string fname, tmp, relName, className;

            className = asmName.Substring(0, 1).ToUpper() + asmName.Substring(1) + "Class";
            fname = Path.Combine(Directory.GetCurrentDirectory(), relName = "Source\\" + className + "." + cdp.FileExtension);
            if (!Directory.Exists(tmp = Path.GetDirectoryName(fname)))
                Directory.CreateDirectory(tmp);

            using (TextWriter tw = new StreamWriter(fname)) {
                cdp.GenerateCodeFromCompileUnit(createClass(ns, className), tw, opts);
            }
            if (pige2 != null)
                pige2.AddItem("Compile", relName);

        }

        static CodeCompileUnit createClass(string ns, string asmName) {
            CodeCompileUnit ret = new CodeCompileUnit();
            CodeNamespace ns0, nsGlobal;
            CodeTypeDeclaration ctd;

            ret.Namespaces.AddRange(new CodeNamespace[] {
                nsGlobal=new CodeNamespace(),
                ns0=new CodeNamespace(ns)
            });
            ns0.Types.Add(ctd = new CodeTypeDeclaration(asmName));
            return ret;
        }

        static void generateForms(ProjectItemGroupElement pige2, CodeDomProvider cdp, CodeGeneratorOptions opts, string ns, string asmName, bool devExpress) {
            string fname, tmp, relName, relName2;

            fname = Path.Combine(Directory.GetCurrentDirectory(), relName = "Source\\UI\\" + asmName + "Form." + cdp.FileExtension);
            if (!Directory.Exists(tmp = Path.GetDirectoryName(fname)))
                Directory.CreateDirectory(tmp);

            using (TextWriter tw = new StreamWriter(fname)) {
                cdp.GenerateCodeFromCompileUnit(createCCUForm(ns, asmName, devExpress), tw, opts);
            }

            fname = Path.Combine(Directory.GetCurrentDirectory(), relName2 = "Source\\UI\\" + asmName + "Form.Designer." + cdp.FileExtension);

            using (TextWriter tw = new StreamWriter(fname)) {
                cdp.GenerateCodeFromCompileUnit(createCCUDesigner(ns, asmName, devExpress), tw, opts);
            }
            if (pige2 != null) {
                pige2.AddItem("Compile", relName);
                var v2 = pige2.AddItem("Compile", relName2);
                v2.AddMetadata("DependentUpon", Path.GetFileName(relName));
            }
        }

        static CodeCompileUnit createCCUMain(string ns, string aName, bool devExpress) {
            CodeCompileUnit ret = new CodeCompileUnit();
            CodeNamespace ns0, nsNamed;
            CodeTypeDeclaration ctd;
            CodeMemberMethod m;

            ret.Namespaces.AddRange(new CodeNamespace[] {
                ns0=new CodeNamespace(),
                nsNamed=new CodeNamespace(ns)
            });
            ns0.Imports.Add(new CodeNamespaceImport("System"));

            nsNamed.Types.Add(ctd = new CodeTypeDeclaration(aName + "Driver"));
#if false
            ctd.Members.Add(m=new CodeEntryPointMethod());
            m.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(new CodeTypeReference(typeof(string)),1),"args"));
#else
            m = addMain(ctd);

#endif
            return ret;
        }

        static CodeCompileUnit createCCUDesigner(string ns, string asmName, bool devExpress) {
            CodeCompileUnit ret = new CodeCompileUnit();
            CodeNamespace ns0, nsNamed;
            CodeTypeDeclaration ctd;
            CodeMemberMethod m;
            CodeMemberField fContainer, f;

            ret.Namespaces.AddRange(new CodeNamespace[] {
                ns0=new CodeNamespace(),
                nsNamed=new CodeNamespace(ns)
            });

            ns0.Imports.AddRange(new CodeNamespaceImport[]{
                new CodeNamespaceImport ("System"),
                new CodeNamespaceImport ("System.ComponentModel"),
            });
            if (devExpress)
                ns0.Imports.AddRange(new CodeNamespaceImport[] {
					new CodeNamespaceImport("System.Diagnostics"),
					new CodeNamespaceImport("DevExpress.XtraEditors"),
					new CodeNamespaceImport("DevExpress.XtraBars"),
					new CodeNamespaceImport("DevExpress.XtraBars.Ribbon"),
				});
            else
                ns0.Imports.AddRange(new CodeNamespaceImport[] {
                    new CodeNamespaceImport ("System.Windows.Forms")
                });

            nsNamed.Types.Add(ctd = new CodeTypeDeclaration(asmName + "Form"));
            ctd.IsPartial = true;
            if (devExpress)
                //                ctd.BaseTypes.Add(new CodeTypeReference("XtraForm"));
                ctd.BaseTypes.Add(new CodeTypeReference("RibbonForm"));
            else
                ctd.BaseTypes.Add(new CodeTypeReference("System.Windows.Forms.Form"));

            ctd.Members.Add(fContainer = new CodeMemberField("IContainer", "components"));
            fContainer.InitExpression = new CodePrimitiveExpression();
            fContainer.Comments.Add(new CodeCommentStatement("<summary>\n Required designer variable.\n </summary>", true));

            if (devExpress) {
                ctd.Members.Add(new CodeMemberField("RibbonControl", FN_RIB_CTRL));
                ctd.Members.Add(new CodeMemberField("RibbonPage", FN_RIB_PAGE));
                ctd.Members.Add(new CodeMemberField("RibbonPageGroup", FN_RIB_PAGE_GRP));
                ctd.Members.Add(new CodeMemberField("BarButtonItem", FN_BAR));
                ctd.Members.Add(f = new CodeMemberField("BarButtonItem", FN_BAR2));
                ctd.Members.Add(f = new CodeMemberField("RibbonStatusBar", FN_RIB_STAT_BAR));
            } else {
                ctd.Members.Add(new CodeMemberField("MenuStrip", KEY_MENU_STRIP));
                ctd.Members.Add(new CodeMemberField("ToolStripMenuItem", KEY_FILE_ITEM));
                ctd.Members.Add(f = new CodeMemberField("ToolStripMenuItem", KEY_FILE_ITEM_EXIT));
            }

            fContainer.StartDirectives.Add(new CodeRegionDirective(CodeRegionMode.Start, "fields"));
            f.EndDirectives.Add(new CodeRegionDirective(CodeRegionMode.End, "fields"));

            foreach (var avar in ctd.Members)
                if (avar.GetType().Equals(typeof(CodeMemberField)))
                    ((CodeMemberField)avar).Attributes = 0;
            m = addInitComp(ctd, devExpress);
            m = addDispose(ctd, m, fContainer);

            return ret;
        }

        const string FN_RIB_CTRL = "ribbonControl";
        const string FN_RIB_PAGE = "ribbonPage";
        const string FN_RIB_PAGE_GRP = "ribbonPageGroup";
        const string FN_RIB_STAT_BAR = "ribbonStatusBar";
        const string FN_BAR = "bbExit";
        const string FN_BAR2 = "bbAbout";

        const string KEY_MENU_STRIP = "ms1";
        const string KEY_FILE_ITEM = "tsmiFile";
        const string KEY_FILE_ITEM_EXIT = "tsmiFileExit";
        static readonly CodeExpression ceThis = new CodeThisReferenceExpression();
        static readonly CodeExpression ceBase = new CodeBaseReferenceExpression();
        static readonly CodeExpression ceNull = new CodePrimitiveExpression();

        const string REGION_NAME = "Windows Form Designer generated code";

        static CodeMemberMethod addInitComp(CodeTypeDeclaration ctd, bool devExpress) {
            CodeMemberMethod m;

            ctd.Members.Add(m = new CodeMemberMethod());
            m.Name = WIN_FORM_INIT;
            m.Attributes = 0;
            m.Comments.Add(new CodeCommentStatement("<summary>Required method for Designer support</summary>", true));
            m.StartDirectives.Add(new CodeRegionDirective(CodeRegionMode.Start, REGION_NAME));
            m.EndDirectives.Add(new CodeRegionDirective(CodeRegionMode.End, REGION_NAME));
            if (devExpress)
                devExpInitComp(ctd, m);
            else
                normalInitComp(ctd, m);
            return m;
        }

        static void devExpInitComp(CodeTypeDeclaration ctd, CodeMemberMethod m) {
            CodeExpression ceRibbon, cePage, cePageGrp, ceBar, ceBar2, ceStatBar;
            CodeTypeReference ctrRibbon, ctrPage, ctrPageGrp, ctrBar, ctrStatBar;

            ceRibbon = findField(ctd, FN_RIB_CTRL, out ctrRibbon);
            cePage = findField(ctd, FN_RIB_PAGE, out ctrPage);
            cePageGrp = findField(ctd, FN_RIB_PAGE_GRP, out ctrPageGrp);
            ceBar = findField(ctd, FN_BAR, out ctrBar);
            ceBar2 = findField(ctd, FN_BAR2, out ctrBar);
            ceStatBar = findField(ctd, FN_RIB_STAT_BAR, out ctrStatBar);

            m.Statements.AddRange(
                new CodeStatement[] {
					new CodeAssignStatement (ceRibbon ,new CodeObjectCreateExpression(ctrRibbon)),
					new CodeAssignStatement (cePage ,new CodeObjectCreateExpression(ctrPage)),
					new CodeAssignStatement (cePageGrp ,new CodeObjectCreateExpression(ctrPageGrp)),
					new CodeAssignStatement (ceBar ,new CodeObjectCreateExpression(ctrBar)),
					new CodeAssignStatement (ceBar2 ,new CodeObjectCreateExpression(ctrBar)),
					new CodeAssignStatement (ceStatBar ,new CodeObjectCreateExpression(ctrStatBar)),
					new CodeExpressionStatement(
						new CodeMethodInvokeExpression (
							new CodePropertyReferenceExpression(cePage,"Groups"),"Add",cePageGrp)),
					new CodeExpressionStatement(
						new CodeMethodInvokeExpression (
							new CodePropertyReferenceExpression(ceRibbon,"Pages"),"Add",cePage)),
					new CodeExpressionStatement(
						new CodeMethodInvokeExpression (
							new CodePropertyReferenceExpression(ceRibbon,"Items"),"Add",ceBar)),
					new CodeExpressionStatement(
						new CodeMethodInvokeExpression (
							new CodePropertyReferenceExpression(ceRibbon,"Items"),"Add",ceBar2)),

					new CodeAssignStatement(
						new CodePropertyReferenceExpression(cePage ,"Text"),
						new CodePrimitiveExpression ("File")),
					new CodeAssignStatement(
						new CodePropertyReferenceExpression(cePageGrp ,"Text"),
						new CodePrimitiveExpression ("File")),
					new CodeExpressionStatement(
						new CodeMethodInvokeExpression (
							new CodePropertyReferenceExpression(ceRibbon,"PageHeaderItemLinks"),"Add",ceBar2)),
					new CodeExpressionStatement(
						new CodeMethodInvokeExpression (
							new CodePropertyReferenceExpression(cePageGrp,"ItemLinks"),"Add",ceBar)),
					new CodeAssignStatement(
						new CodePropertyReferenceExpression(ceBar,"Caption"),
						new CodePrimitiveExpression("Exit")),

					new CodeAssignStatement(
						new CodePropertyReferenceExpression(ceBar,"Name"),
						new CodePrimitiveExpression(FN_BAR)),
					new CodeAttachEventStatement(
						ceBar,"ItemClick",
						new CodeDelegateCreateExpression(new CodeTypeReference ("ItemClickEventHandler"),
							ceThis ,DX_EXIT_NAME)),

					new CodeAssignStatement(
						new CodePropertyReferenceExpression(ceBar2,"Name"),
						new CodePrimitiveExpression(FN_BAR2)),

					new CodeAttachEventStatement(
						ceBar2,"ItemClick",
						new CodeDelegateCreateExpression(new CodeTypeReference ("ItemClickEventHandler"),
							ceThis ,DX_ABOUT_NAME)),
					new CodeExpressionStatement(
						new CodeMethodInvokeExpression (
							new CodePropertyReferenceExpression(ceThis,"Controls"),"Add",ceRibbon)),
					new CodeExpressionStatement(
						new CodeMethodInvokeExpression (
							new CodePropertyReferenceExpression(ceThis,"Controls"),"Add",ceStatBar)),

                    createLoadStatement(ceThis,"Load","formLoad",new CodeTypeReference(typeof(EventHandler))),

                    new CodeAssignStatement(
                        new CodePropertyReferenceExpression(ceThis,"Ribbon"),
                        ceRibbon),

                    new CodeAssignStatement(
                        new CodePropertyReferenceExpression(ceThis,"StatusBar"),
                        ceStatBar ),

                    new CodeAssignStatement (
                        new CodePropertyReferenceExpression(
                            ceStatBar,"Ribbon"),
                            ceRibbon),


                    new CodeAssignStatement (
                        new CodePropertyReferenceExpression(ceThis,"AutoHideRibbon"),
                        new CodePrimitiveExpression(false)),
                    

			});
        }

        static CodeStatement createLoadStatement(CodeExpression ce, string eventName, string methodName, CodeTypeReference ctr) {
            return new CodeAttachEventStatement(ce, eventName,
                new CodeDelegateCreateExpression(ctr, ce, methodName));
        }

        static CodeMemberMethod createActionMethod(string methodName, string eventArgName) {
            return createActionMethod(methodName, eventArgName, true);
        }

        static CodeMemberMethod createActionMethod(string methodName, string eventArgName, bool genDummy) {
            CodeMemberMethod m2;

            m2 = new CodeMemberMethod();
            m2.Name = methodName;
            m2.Attributes = 0;
            m2.Parameters.AddRange(new CodeParameterDeclarationExpression[] {
				new CodeParameterDeclarationExpression(new CodeTypeReference(typeof(object)),"sender"),
				new CodeParameterDeclarationExpression(eventArgName,"e"),
			});
            if (genDummy)
                m2.Statements.Add(
                    new CodeExpressionStatement(
                        new CodeMethodInvokeExpression(
                    // new CodeTypeReferenceExpression("System.Diagnostics.Debug"),
                    // new CodeTypeReferenceExpression(typeof(Debug)),
                            new CodeTypeReferenceExpression("Debug"),
                            "Print",
                            new CodePrimitiveExpression("here"))));
            return m2;
        }

        const string DX_EXIT_NAME = "exitApp";
        const string DX_ABOUT_NAME = "showAbout";

        static void normalInitComp(CodeTypeDeclaration ctd, CodeMemberMethod m) {
            CodeTypeReference ctrMenu, ctrFile, ctrFileExit;
            CodeExpression ceMenu, ceFile, ceTmp, ceFileExit;

            ceMenu = findField(ctd, KEY_MENU_STRIP, out ctrMenu);
            ceFile = findField(ctd, KEY_FILE_ITEM, out ctrFile);
            ceFileExit = findField(ctd, KEY_FILE_ITEM_EXIT, out ctrFileExit);

            if (ceMenu != null && ctrMenu != null &&
                ceFile != null && ctrFile != null) {
                ceTmp = new CodePrimitiveExpression(KEY_MENU_STRIP);
                m.Statements.AddRange(
                    new CodeStatement[] {
                        new CodeAssignStatement (
                            ceMenu,
                            new CodeObjectCreateExpression(ctrMenu)),
                        new CodeAssignStatement (
                            ceFile,
                            new CodeObjectCreateExpression(ctrFile)),
                        new CodeAssignStatement (
                            ceFileExit,
                            new CodeObjectCreateExpression(ctrFileExit)),

                        new CodeExpressionStatement(
                            new CodeMethodInvokeExpression(
                                new CodePropertyReferenceExpression(
                                    ceMenu ,"Items"),"AddRange",
                                    new CodeArrayCreateExpression(
                                        "ToolStripItem",
                                        new CodeExpression[] {
                                            ceFile ,
                                        }))),

                        new CodeExpressionStatement(
                            new CodeMethodInvokeExpression(
                                new CodePropertyReferenceExpression(
                                    ceFile ,"DropDownItems"),"AddRange",
                                    new CodeArrayCreateExpression(
                                        "ToolStripItem",
                                        new CodeExpression[] {
                                            ceFileExit
                                        }))),

                        new CodeAssignStatement(
                            new CodePropertyReferenceExpression(ceMenu,"Name"),
                            ceTmp),
                        new CodeAssignStatement(
                            new CodePropertyReferenceExpression(ceMenu,"Text"),
                            ceTmp),

                        new CodeAssignStatement(
                            new CodePropertyReferenceExpression(ceFile,"Name"),
                            new CodePrimitiveExpression (KEY_FILE_ITEM)),
                        new CodeAssignStatement(
                            new CodePropertyReferenceExpression(ceFile,"Text"),
                            new CodePrimitiveExpression ("File")),

                        new CodeAssignStatement(
                            new CodePropertyReferenceExpression(ceFileExit,"Name"),
                            new CodePrimitiveExpression (KEY_FILE_ITEM_EXIT)),
                        new CodeAssignStatement(
                            new CodePropertyReferenceExpression(ceFileExit,"Text"),
                            new CodePrimitiveExpression ("Exit")),

                        new CodeAttachEventStatement (
                            ceFileExit,
                            "Click",
                            new CodeDelegateCreateExpression(
                                new CodeTypeReference (typeof(EventHandler)),ceThis,METHOD_NAME_EXIT_CLICK)),

                        new CodeExpressionStatement(
                            new CodeMethodInvokeExpression (
                                new CodeFieldReferenceExpression(
                                    ceThis,"Controls"),
                                "Add",
                                ceMenu )),

                        new CodeAssignStatement(
                            new CodePropertyReferenceExpression (ceThis,"MainMenuStrip"),
                            ceMenu ),

                        createLoadStatement(ceThis,"Load","formLoad",new CodeTypeReference(typeof(EventHandler))),

                    });
            }
        }

        static CodeTypeMember createLoadMethod(string name, CodeTypeReference ctr) {
            CodeMemberMethod m = new CodeMemberMethod();


            m.Name = name;
            m.Attributes = 0;
            m.Parameters.AddRange(
                new CodeParameterDeclarationExpression[] {
                    new CodeParameterDeclarationExpression(typeof(object),"sender"),
                    new CodeParameterDeclarationExpression(ctr,"ea")
                });
            return m;
        }

        const string METHOD_NAME_EXIT_CLICK = "exitClick";

        static CodeFieldReferenceExpression findField(CodeTypeDeclaration ctd, string fldName, out CodeTypeReference tr) {
            CodeMemberField f;

            tr = null;
            foreach (var avar in ctd.Members)
                if (avar.GetType().Equals(typeof(CodeMemberField)) &&
                    ((CodeMemberField)avar).Name.CompareTo(fldName) == 0) {
                    f = avar as CodeMemberField;
                    tr = f.Type;
                    return new CodeFieldReferenceExpression(ceThis, f.Name);
                }
            return null;
        }

        static CodeMemberMethod addDispose(CodeTypeDeclaration ctd, CodeMemberMethod m, CodeMemberField fContainer) {
            CodeArgumentReferenceExpression ar0;

            ar0 = new CodeArgumentReferenceExpression("disposing");

            ctd.Members.Add(m = new CodeMemberMethod());
            m.Name = "Dispose";
            m.Attributes = MemberAttributes.Override | MemberAttributes.Family;
            m.Parameters.Add(new CodeParameterDeclarationExpression(typeof(bool), ar0.ParameterName));

            CodeConditionStatement ccs, ccs1;

            CodeFieldReferenceExpression fr;

            fr = new CodeFieldReferenceExpression(null, fContainer.Name);
            ccs = new CodeConditionStatement(ar0, ccs1 = new CodeConditionStatement());
            ccs1.Condition = new CodeBinaryOperatorExpression(
                fr,
                CodeBinaryOperatorType.IdentityInequality,
                new CodePrimitiveExpression());
            ccs1.TrueStatements.Add(
                new CodeExpressionStatement(
                    new CodeMethodInvokeExpression(fr, "Dispose")));
            m.Statements.AddRange(
                new CodeStatement[] {
                    ccs,
                    new CodeExpressionStatement(
                        new CodeMethodInvokeExpression(
                            new CodeBaseReferenceExpression(),
                            "Dispose",ar0))
            });
            m.Comments.AddRange(
                new CodeCommentStatement[]{
                    new CodeCommentStatement("<summary>\n Clean up any resources being used.\n </summary>",true)
                }
                );
            return m;
        }

        const string WIN_FORM_INIT = "InitializeComponent";

        static readonly CodeStatement csBlank = new CodeSnippetStatement();

        static CodeCompileUnit createCCUForm(string ns, string asmName, bool devExpress) {
            CodeCompileUnit ret = new CodeCompileUnit();
            CodeNamespace ns0, nsNamed;
            CodeTypeDeclaration ctd;
            CodeConstructor cc;
            string formName;

            ret.Namespaces.AddRange(new CodeNamespace[] {
                ns0=new CodeNamespace(),
                nsNamed=new CodeNamespace(ns)
            });

            ns0.Imports.AddRange(new CodeNamespaceImport[] {
                new CodeNamespaceImport ("System"),
                new CodeNamespaceImport ("System.Windows.Forms"),
                new CodeNamespaceImport ("System.ComponentModel"),
                new CodeNamespaceImport ("System.Diagnostics"),
            });
            if (devExpress)
                ns0.Imports.AddRange(
                    new CodeNamespaceImport[] {
						new CodeNamespaceImport ("DevExpress.Skins"),
						new CodeNamespaceImport ("DevExpress.UserSkins"),
						new CodeNamespaceImport ("DevExpress.XtraBars"),
					});
            nsNamed.Types.Add(ctd = new CodeTypeDeclaration(formName = asmName + "Form"));
            ctd.IsPartial = true;

            ctd.Members.Add(cc = new CodeConstructor());
            cc.Attributes = MemberAttributes.Public;

            cc.Statements.Add(
                    new CodeExpressionStatement(
                        new CodeMethodInvokeExpression(
                            new CodeMethodReferenceExpression(null, WIN_FORM_INIT))));

            if (devExpress) {
                CodeMemberMethod m2;

                ctd.Members.Add(createActionMethod(DX_ABOUT_NAME, "ItemClickEventArgs"));
                ctd.Members.Add(m2 = createActionMethod(DX_EXIT_NAME, "ItemClickEventArgs", false));
                genAppExitCode(m2);
            } else {
                CodeMemberMethod m;

                ctd.Members.Add(m = new CodeMemberMethod());
                m.Attributes = 0;
                m.Name = METHOD_NAME_EXIT_CLICK;
                m.Parameters.AddRange(
                    new CodeParameterDeclarationExpression[] {
                    new CodeParameterDeclarationExpression(typeof(object),"sender"),
                    new CodeParameterDeclarationExpression(ctrEA,"ea"),
                });
                genAppExitCode(m);
            }
            ctd.Members.Add(createLoadMethod("formLoad", ctrEA));
            addMain(ctd, true, formName, devExpress);
            return ret;
        }

        static readonly CodeTypeReference ctrEA = new CodeTypeReference("EventArgs");

        static void genAppExitCode(CodeMemberMethod m) {
            CodeVariableReferenceExpression vr = new CodeVariableReferenceExpression("cea");
            CodeTypeReference ctr = new CodeTypeReference("CancelEventArgs");
            m.Statements.AddRange(

                new CodeStatement[] {
                    new CodeVariableDeclarationStatement (
                        ctr,vr.VariableName ,
                        new CodeObjectCreateExpression(ctr)),
                    csBlank,

                    new CodeExpressionStatement(
                        new CodeMethodInvokeExpression (
                            new CodeTypeReferenceExpression("Application"),"Exit",vr)),

                    new CodeConditionStatement (
                        new CodePropertyReferenceExpression (vr,"Cancel"),new CodeMethodReturnStatement()),

                    new CodeExpressionStatement(
                        new CodeMethodInvokeExpression (
                            new CodeTypeReferenceExpression("Application"),"Exit"))
                });
        }

        static CodeMemberMethod addMain(CodeTypeDeclaration ctd) {
            return addMain(ctd, false, null, false);
        }

        static CodeMemberMethod addMain(CodeTypeDeclaration ctd, bool isForm, string formName, bool devExpress) {
            CodeMemberMethod m;
            CodeExpression e;

            ctd.Members.Add(m = new CodeMemberMethod());
            m.Name = "Main";
            m.Attributes = MemberAttributes.Public | MemberAttributes.Static;
            m.Parameters.Add(new CodeParameterDeclarationExpression(new CodeTypeReference(new CodeTypeReference(typeof(string)), 1), "args"));
            m.CustomAttributes.Add(new CodeAttributeDeclaration("STAThread"));
            if (isForm) {
                if (string.IsNullOrEmpty(formName))
                    throw new ArgumentNullException("formName", "form-name is null!");
                e = new CodeTypeReferenceExpression("System.Windows.Forms.Application");
                e = new CodeTypeReferenceExpression("Application");
                CodeExpression ce1, ce2;

#if true
                ce1 = new CodeTypeReferenceExpression("BonusSkins");
                ce2 = new CodeTypeReferenceExpression("SkinManager");
#else
				ce1 = new CodeTypeReferenceExpression("DevExpress.UserSkins.BonusSkins");
				ce2 = new CodeTypeReferenceExpression("DevExpress.Skins.SkinManager");
#endif

                if (devExpress)
                    m.Statements.AddRange(
                        new CodeStatement[] {
							/*
							 * DevExpress.UserSkins.BonusSkins.Register();
							 * DevExpress.Skins.SkinManager.EnableFormSkins();
							 * */
						new CodeExpressionStatement(
							new CodeMethodInvokeExpression (ce1,"Register")),
						new CodeExpressionStatement(
							new CodeMethodInvokeExpression (ce2,"EnableFormSkins")),
					});
                m.Statements.AddRange(
                    new CodeStatement[] {
                        new CodeExpressionStatement(
                            new CodeMethodInvokeExpression (e,"EnableVisualStyles")),
                        new CodeExpressionStatement(
                            new CodeMethodInvokeExpression (e,"SetCompatibleTextRenderingDefault",
                                new CodePrimitiveExpression(false))),
                        new CodeExpressionStatement(
                            new CodeMethodInvokeExpression (e,"Run",
                                new CodeObjectCreateExpression(formName)))
                });
            }
            return m;
        }

        static void generateMain(ProjectItemGroupElement pige2, CodeDomProvider cdp, CodeGeneratorOptions opts, string ns, string aName) {
            string fname, tmp, relName;

            fname = Path.Combine(Directory.GetCurrentDirectory(), relName = "Source\\adriver." + cdp.FileExtension);
            if (!Directory.Exists(tmp = Path.GetDirectoryName(fname)))
                Directory.CreateDirectory(tmp);

            using (TextWriter tw = new StreamWriter(fname)) {
                cdp.GenerateCodeFromCompileUnit(createCCUMain(ns, aName, false), tw, opts);
            }
            pige2.AddItem("Compile", relName);
        }
    }
}