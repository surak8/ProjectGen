using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;

namespace NSprojectgen {
	static class DefaultProjectGenerator {
		internal static void generate(string outputFile, string szVersion, string asmName, string ns, ProjectType type, bool rebuild, bool phibroStyle, bool devExpress, bool simple, bool generateCode) {
			Project p = null;
			bool generate = false;

			if (File.Exists(outputFile))
				try {
					p = new Project(outputFile);
				} catch (Exception ex) {
					Console.Error.WriteLine("failed to open " + outputFile + " [" + ex.Message + "]");
				}

			if (p == null) {
				p = new Project();
				generate = true;
			}
			if (rebuild) {
				p.Xml.RemoveAllChildren();
				generate = true;
			}
			doStuff(p, szVersion, asmName, ns, type, generate, phibroStyle, devExpress, simple, generateCode);
			p.Save(outputFile);
			Debug.Print("saved: " + outputFile);
		}

		const string APP_CFG = "app.config";

		static bool createConfig = true;
		static bool createAsmInfo = true;

		static void doStuff(Project p, string szVersion, string asmName, string ns, ProjectType type, bool generate, bool phibroStyle, bool devExpress, bool simple, bool generateCode) {
			p.Xml.DefaultTargets = "Build";
			addDefaultProperty(p, type, ns, asmName);

			addPropertyGroup(p, true, asmName);
			addPropertyGroup(p, false, asmName);

			if (generateCode) {
				generateCommonFiles(p, szVersion, asmName, simple);

				if (!simple) {
					generateFiles(p, asmName, ns, type, devExpress);
				}
			}

			var vvv = p.Xml.AddItemGroup();
			vvv.Label = "References";
			vvv.AddItem("Reference", "System");
			if (!simple)
				vvv.AddItem("Reference", "System.Data");
			if (type == ProjectType.WindowsForm) {
				vvv.AddItem("Reference", "System.Windows.Forms");
				vvv.AddItem("Reference", "System.Drawing");
			}
			if (devExpress) {
				vvv.AddItem("Reference", "DevExpress.BonusSkins.v12.2");
				vvv.AddItem("Reference", "DevExpress.Utils.v12.2");
				vvv.AddItem("Reference", "DevExpress.XtraBars.v12.2");
				vvv.AddItem("Reference", "DevExpress.XtraEditors.v12.2");
				vvv.AddItem("Reference", "DevExpress.Data.v12.2");
			}

			if (phibroStyle)
				generatePhibroSection(p);

			var p3 = p.Xml.AddImport("$(MSBuildToolsPath)\\Microsoft.CSharp.targets");
			if (phibroStyle) {
				// G:\MIS\icts\MSBuild\2010
				p.Xml.AddImport("$(MSPEIRules)\\Phibro.CopyBundle.Targets");
				p.Xml.AddImport("$(MSPEIRules)\\Phibro.CopyToShared.Targets");
				p.Xml.AddImport("$(MSPEIRules)\\Phibro.LightWeightAssembly.Targets");
			}
			var v0 = p.Xml.AddPropertyGroup();
			v0.AddProperty("StartupObject", string.Empty);
			if (phibroStyle)
				generatePhibroRules(p);
		}

		static void generateCommonFiles(Project p, string szVersion, string asmName, bool simple) {
			ProjectItemGroupElement pige = null;
			string asmInfoName;

			if (createConfig && !simple) {
				createAppCfgFile(APP_CFG);
				pige = createItemGroup(p, "None");
				pige.AddItem("None", APP_CFG);
			}

			if (createAsmInfo) {
				using (CodeDomProvider cdp = new CSharpCodeProvider()) {
					CodeGeneratorOptions opts = new CodeGeneratorOptions();
					asmInfoName = PROPERTY_DIR + "\\" + "AssemblyInfo." + cdp.FileExtension;
					CSGenerator.createAsmInfoFile(
						Path.Combine(Directory.GetCurrentDirectory(), asmInfoName),
							asmName, szVersion, cdp, opts);

					var avar = createItemGroup(p, "AssemblyInfo");
					avar.AddItem("Compile", asmInfoName);
				}
			}
		}

		static ProjectItemGroupElement createItemGroup(Project p, string label) {
			ProjectItemGroupElement ret = p.Xml.AddItemGroup();

			if (!string.IsNullOrEmpty(label))
				ret.Label = label;
			return ret;
		}

		static void generateFiles(Project p, string asmName, string ns, ProjectType type, bool devExpress) {
			CodeGeneratorOptions opts;
			ProjectItemGroupElement pige;

			opts = new CodeGeneratorOptions();
			opts.BlankLinesBetweenMembers = false;
			opts.ElseOnClosing = true;
			pige = p.Xml.AddItemGroup();
			pige.Label = "IDK";

			CSGenerator.generateMainFiles(p, asmName, type, pige, new CSharpCodeProvider(), opts, ns, devExpress);
		}

		static void createAppCfgFile(string filename) {
			XmlWriterSettings xws;

			if (File.Exists(filename))
				File.Delete(filename);

			xws = new XmlWriterSettings();
			xws.Indent = true;
			xws.IndentChars = new string(' ', 4);
			xws.Encoding = Encoding.ASCII;
			xws.OmitXmlDeclaration = true;
			using (XmlWriter xw = XmlWriter.Create(filename, xws)) {
				xw.WriteStartDocument();
				xw.WriteStartElement("configuration");
				xw.WriteStartElement("runtime");
				xw.WriteStartElement("assemblyBinding", "urn:schemas-microsoft-com:asm.v1");
				xw.WriteStartElement("probing");
				xw.WriteAttributeString("privatePath", "assemblies\\" + PEI_PROVIDER_NAME + "\\" + PEI_COMMON_DEFAULT_VERSION);
				xw.WriteEndElement();
				xw.WriteEndElement();
				xw.WriteEndElement();
				xw.WriteEndElement();
				xw.WriteEndDocument();
			}

		}

		const string PEI_RULES_DIR = "MSPEIRules";
		const string PEI_BASE_ASM_FOLDER = "_BaseAsmFolder";
		const string PEI_SHARED_DIR = "SharedDir";
		const string PEI_COPY_ALL_FILES = "CopyAllFiles";
		const string PEI_COMMON_VERSION = "CommonVersion";
		const string PEI_COMMON_DEFAULT_VERSION = "4.1.0.0";
		const string PEI_ASM_VERSION = "AssemblyVersion";
		const string PEI_ASM_BUNDLE = "AssemblyBundleName";
		const string PEI_VERBOSE = "Verbose";

		static void generatePhibroSection(Project p) {
			Debug.Print("generatePhibroSection");

			var avar = p.Xml.AddPropertyGroup();
			avar.Label = "PhibroProperties";
			avar.AddProperty(PEI_RULES_DIR, @"G:\MIS\icts\MSBuild\2010");
			avar.AddProperty(PEI_BASE_ASM_FOLDER, @"G:\MIS\phibro\shared\assemblies");
			avar.AddProperty(PEI_SHARED_DIR, "$(" + PEI_BASE_ASM_FOLDER + ")\\$(" + KEY_CFG + ")");

			avar.AddProperty(PEI_COMMON_VERSION, PEI_COMMON_DEFAULT_VERSION);
			createNullProperty1(avar, PEI_ASM_VERSION, PEI_COMMON_VERSION);
			createNullProperty1(avar, PEI_ASM_BUNDLE, "AssemblyName");

			createNullProperty(avar, PEI_COPY_ALL_FILES, "false");
			createNullProperty(avar, PEI_VERBOSE, "false");

			createPhibroItems(p);
		}

		const string PEI_ITEM_NAME = "BundleToCopy";
		const string PEI_ITEM_META_NAME = "BundleName";
		const string PEI_ITEM_META_VERSION = "BundleVersion";
		const string PEI_ITEM_META_MULTI = "HasMultipleItems";

		static readonly string BNAME = PEI_ITEM_NAME + "." + PEI_ITEM_META_NAME;
		static readonly string BNAME_VERSION = PEI_ITEM_NAME + "." + PEI_ITEM_META_VERSION;
		static readonly string BNAME_MULTI = PEI_ITEM_NAME + "." + PEI_ITEM_META_MULTI;

		const string PEI_PROVIDER_NAME = "PEIDBProvider";
		static void createPhibroItems(Project p) {
			var avar2 = p.Xml.AddItemGroup();
			avar2.Label = "PhibroItems";

			var avar3 = avar2.AddItem(PEI_ITEM_NAME, "refs\\peidb.ref");
			avar3.AddMetadata(PEI_ITEM_META_NAME, PEI_PROVIDER_NAME);
			avar3.AddMetadata(PEI_ITEM_META_VERSION, propRef(PEI_COMMON_VERSION));
			avar3.AddMetadata(PEI_ITEM_META_MULTI, "false");
		}

		const string PEI_RULE_BRR = "BeforeResolveReferences";
		const string PEI_RULE_PCP = "precreateProps";
		const string PEI_PROP_LOCAL_ASM_FOLDER = "LocalAssemblyFolder";

		static void generatePhibroRules(Project p) {
			var v0 = p.Xml.AddTarget(PEI_RULE_BRR);
			v0.DependsOnTargets = PEI_RULE_PCP + ";copyBundles";
			addCreateItem(
				v0,
			  itemRef(BNAME),
				nullCondItemRef(BNAME, false) + " and " +
				makeCondition(itemRef(BNAME_MULTI), "true", false),
				"Private=false;" +
				"HintPath=" + propRef(PEI_PROP_LOCAL_ASM_FOLDER) + "\\" + itemRef(BNAME) + "\\" +
					itemRef(BNAME_VERSION) + "\\" + itemRef(BNAME) + ".dll;" +
				"Name=" + itemRef(BNAME) + ".dll;",
				"Reference");
			var v1 = p.Xml.AddTarget(PEI_RULE_PCP);
			string tmp = itemRef("_OutputPathItem.FullPath");
			addCreateProperty(v1,
				tmp + "\\assemblies",
				"!HasTrailingSlash('" + tmp + "')",
			   PEI_PROP_LOCAL_ASM_FOLDER);
			addCreateProperty(v1,
				tmp + "assemblies",
				"HasTrailingSlash('" + tmp + "')",
				PEI_PROP_LOCAL_ASM_FOLDER);

			var v2 = v1.AddTask("Message");

			v2.SetParameter("Importance", "high");
			v2.SetParameter("Text", PEI_PROP_LOCAL_ASM_FOLDER + "=" + propRef(PEI_PROP_LOCAL_ASM_FOLDER));
			v2.Condition = makeCondition(propRef(PEI_VERBOSE), "true", true);
		}

		static void addCreateProperty(ProjectTargetElement v1, string p, string p_2, string p_3) {
			var v0 = v1.AddTask("CreateProperty");

			v0.SetParameter("Value", p);
			if (!string.IsNullOrEmpty(p_2))
				v0.Condition = p_2;
			v0.AddOutputProperty("Value", p_3);
		}

		#region done
		static string makeCondition(string str, string p, bool p_2) {
			return "'" + str + "'" + (p_2 ? "==" : "!=") + "'" + p + "'";
		}

		static string itemRef(string refVal) {
			return "%(" + refVal + ")";
		}

		static string propRef(string refVal) {
			return "$(" + refVal + ")";
		}

		static string nullCondItemRef(string refVal, bool isEqual) {
			return "'" + itemRef(refVal) + "'" + (isEqual ? "==" : "!=") + "''";
		}
		#endregion done

		static void addCreateItem(ProjectTargetElement v0, string includeValue, string condition, string meta, string outItemName) {
			var v2 = v0.AddTask("CreateItem");
			v2.SetParameter("Include", includeValue);
			if (!string.IsNullOrEmpty(meta))
				v2.SetParameter("AdditionalMetadata", meta);
			if (!string.IsNullOrEmpty(condition))
				v2.Condition = condition;
			v2.AddOutputItem("Include", outItemName);
		}

		static void createNullProperty1(ProjectPropertyGroupElement ppge, string key, string valRef) {
			createNullProperty(ppge, key, propRef(valRef));
		}

		static void createNullProperty(ProjectPropertyGroupElement ppge, string key, string val) {
			var avar = ppge.AddProperty(key, val);
			avar.Condition = "'" + propRef(key) + "'==''";
		}

		#region complete
		static void addPropertyGroup(Project p, bool isDebug, string aName) {
			var p0 = p.Xml.AddPropertyGroup();
			p0.Condition = " '$(" + KEY_CFG + ")|$(" + KEY_PLAT + ")' == '" + (isDebug ? KEY_DEBUG : KEY_RELEASE) + "|" + KEY_PLAT_DEF + "' ";

			p0.AddProperty(KEY_PLAT_TARG, KEY_PLAT_DEF);
			if (isDebug)
				p0.AddProperty("DebugSymbols", isDebug ? "true" : "false");
			p0.AddProperty("DebugType", isDebug ? "full" : "none");
			p0.AddProperty("Optimize", isDebug ? "false" : "true");
			p0.AddProperty("OutputPath", ".\\");
			p0.AddProperty("DefineConstants", (isDebug ? "DEBUG;" : string.Empty) + "TRACE");
			p0.AddProperty("ErrorReport", "prompt");
			p0.AddProperty("WarningLevel", "4");
			p0.AddProperty("DocumentationFile", aName + ".xml");
			p0.AddProperty("UseVSHostingProcess", "false");
			p0.AddProperty("NoWarn", "1591");
		}

		const string KEY_CFG = "Configuration";
		const string KEY_PLAT = "Platform";
		const string KEY_PLAT_TARG = "PlatformTarget";

		const string KEY_DEBUG = "Debug";
		const string KEY_RELEASE = "Release";
		const string KEY_PLAT_DEF = "x86";
		const string KEY_PROD_VERSION = "8.0.30703";
		const string KEY_SCHEMA_VERSION = "2.0";

		const string TARGET_35 = "v3.5";
		const string TARGET_40 = "v4.0";

		static void addDefaultProperty(Project p, ProjectType type, string ns, string aName) {
			string outType;

			var p0 = p.Xml.AddPropertyGroup();
			var v0 = p0.AddProperty(KEY_CFG, KEY_DEBUG);
			v0.Condition = " '$(" + KEY_CFG + ")' == '' ";

			var v1 = p0.AddProperty(KEY_PLAT, KEY_PLAT_DEF);
			v1.Condition = " '$(" + KEY_PLAT + ")' == '' ";

			p0.AddProperty("ProductVersion", KEY_PROD_VERSION);
			p0.AddProperty("SchemaVersion", KEY_SCHEMA_VERSION);
			p0.AddProperty("ProjectGuid", "{" + Guid.NewGuid().ToString().ToUpper() + "}");
			switch (type) {
				case ProjectType.WindowsForm:
					outType = "WinExe";
					break;
				case ProjectType.ConsoleApp:
					outType = "Exe";
					break;
				case ProjectType.ClassLibrary:
					outType = "Library";
					break;
				default:
					throw new InvalidOperationException("unhandled project-type: " + type);
			}
			p0.AddProperty("OutputType", outType);
			p0.AddProperty("AppDesignerFolder", PROPERTY_DIR);
			p0.AddProperty("RootNamespace", string.IsNullOrEmpty(ns) ? "nsdefault" : ns);
			p0.AddProperty("AssemblyName", aName);
			p0.AddProperty("TargetFrameworkVersion", TARGET_40);
			p0.AddProperty("FileAlignment", "512");
			p0.AddProperty("TargetFrameworkProfile", string.Empty);
		}
		public const string PROPERTY_DIR = "Properties";
		#endregion complete
	}
}