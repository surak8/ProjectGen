using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using System;
using System.Reflection;

namespace NSprojectgen {
	class CProjectGenerator {

		#region delegates
		delegate void Blah(ProjectRootElement root, ProjectImportGroupElement pige);
		delegate void Blah2(ProjectPropertyGroupElement ppge);
		delegate void Blah3(ProjectItemDefinitionElement pide, bool isDebug);
		delegate void Blah4(ProjectItemGroupElement pige);
		#endregion

		#region constants
		const string DEBUG = "Debug";
		const string RELEASE = "Release";
		const string PLATFORM = "Win32";
		const string COMPILER = "ClCompile";
		const string LINKER = "Link";
		const string FRAMEWORK_4 = "v4.0";
		const string FRAMEWORK_452 = "v4.5.2";
		#endregion

		#region fields
		static bool reuseGuid = false;
		#endregion

		internal static void generate(string filename, string version, string asmName, string ns, ProjectType type) {
			Project p = new Project();
			string typeDesc = null;

			p.Xml.DefaultTargets = "Build";
			createItemGroup(p, "ProjectConfigurations");
			createGlobals(ns, type, p, "Globals");
			p.Xml.AddImport(@"$(VCTargetsPath)\Microsoft.Cpp.Default.props");

			switch (type) {
				case ProjectType.ConsoleApp: typeDesc = "Application"; break;
				case ProjectType.XamlApp: typeDesc = "Application"; break;
				case ProjectType.ClassLibrary: typeDesc = "DynamicLibrary"; break;
				default:
					throw new InvalidOperationException("unhandled projectType: " + type);
			}
			createCfgProp(p.Xml, typeDesc, true);
			createCfgProp(p.Xml, typeDesc, false);
			p.Xml.AddImport(@"$(VCTargetsPath)\Microsoft.Cpp.props");
			addPropertySheetImports(p.Xml);
			addPropertyGroup(p.Xml, makeCfgCondition(DEBUG, PLATFORM), new Blah2(b2));
			addPropertyGroup(p.Xml, makeCfgCondition(RELEASE, PLATFORM), new Blah2(b2));
			addItemDefs(p.Xml);

			const string C_TARGET_RULES = @"$(VCTargetsPath)\Microsoft.Cpp.targets";
			var v99 = p.Xml.CreateImportElement(C_TARGET_RULES);
			p.Xml.AppendChild(v99);
			p.Save(filename);
		}

		static ProjectItemGroupElement addItemGroup(ProjectRootElement root, Blah4 blah4) {
			return addItemGroup(root, null, null, blah4);
		}

		static void fakeC(ProjectItemGroupElement pige) {
			pige.AddItem("ClCompile", "fake.c");
		}
		static void fakeH(ProjectItemGroupElement pige) {
			pige.AddItem("ClInclude", "fake.h");
		}
		static void fakeNone(ProjectItemGroupElement pige) {
			pige.AddItem("None", "fake.txt");
		}

		static ProjectItemGroupElement addItemGroup(ProjectRootElement root, string label, string condition, Blah4 blah4) {
			ProjectItemGroupElement ret = addItemGroup(root);

			if (!string.IsNullOrEmpty(label))
				ret.Label = label;
			if (!string.IsNullOrEmpty(condition))
				ret.Condition = condition;
			if (blah4 != null)
				blah4(ret);
			return ret;
		}

		static ProjectItemGroupElement addItemGroup(ProjectRootElement root) {
			ProjectItemGroupElement avar = root.CreateItemGroupElement();

			root.AppendChild(avar);
			return avar;
		}

		static void addImportGroup(ProjectRootElement root, string p) {
			addImportGroup(root, p, null, null);
		}

		static void addItemDefs(ProjectRootElement root) {
			createItemDef(root, true);
			createItemDef(root, false);
		}

		static void createItemDef(ProjectRootElement root, bool p) {
			var avar = root.CreateItemDefinitionGroupElement();
			root.AppendChild(avar);
			avar.Condition = makeCfgCondition(p ? DEBUG : RELEASE, PLATFORM);
			addItemDef(avar, COMPILER, p, new Blah3(populateCompiler));
			addItemDef(avar, LINKER, p, new Blah3(populateLinker));
		}

		static void addItemDef(ProjectItemDefinitionGroupElement pidge, string toolName, bool isDebug, Blah3 blah3) {
			var avar = pidge.AddItemDefinition(toolName);

			if (blah3 != null)
				blah3(avar, isDebug);
		}

		static void populateLinker(ProjectItemDefinitionElement avar, bool isDebug) {
			if (!isDebug) {
				avar.AddMetadata("EnableCOMDATFolding", "true");
				avar.AddMetadata("OptimizeReferences", "true");
			}
			avar.AddMetadata("GenerateDebugInformation", isDebug.ToString().ToLower());
		}

		static void populateCompiler(ProjectItemDefinitionElement avar, bool isDebug) {
			if (!isDebug)
				avar.AddMetadata("FunctionLevelLinking", "true");
			avar.AddMetadata("PreprocessorDefinitions", (isDebug ? "_DEBUG" : "_NDEBUG") + ";_MBCS;%(PreprocessorDefinitions)");
			if (isDebug) {
				avar.AddMetadata("DebugInformationFormat", "ProgramDatabase");
				avar.AddMetadata("WarningLevel", "EnableAllWarnings");
			}
		}

		static void b2(ProjectPropertyGroupElement ppge) {
			//            ppge.AddProperty("OutDir", "$(SolutionDir)");
			//            ppge.AddProperty("LinkIncremental", "false");
		}

		static void addPropertyGroup(ProjectRootElement root, string condition, Blah2 blah2) {
			var avar = addPropertyGroup(root, null);

			if (!string.IsNullOrEmpty(condition))
				avar.Condition = condition;
			if (blah2 != null)
				blah2(avar);
		}

		internal static void generate(PGOptions opts) {
			//			Logger.log(MethodBase.GetCurrentMethod());
			generate(opts.projectFileName, opts.assemblyVersion, opts.assemblyVersion, opts.projectNamespace, opts.projectType);
		}

		static ProjectPropertyGroupElement addPropertyGroup(ProjectRootElement root, string p) {
			ProjectPropertyGroupElement v1 = root.CreatePropertyGroupElement();

			root.AppendChild(v1);
			if (!string.IsNullOrEmpty(p))
				v1.Label = p;
			return v1;
		}

		static void addPropertySheetImports(ProjectRootElement root) {
			addImportGroup(root, "PropertySheets", makeCfgCondition(DEBUG, PLATFORM), new Blah(b1));
			addImportGroup(root, "PropertySheets", makeCfgCondition(RELEASE, PLATFORM), new Blah(b1));
		}

		static void b1(ProjectRootElement root, ProjectImportGroupElement pige) {
			const string FILE = @"$(UserRootDir)\Microsoft.Cpp.$(Platform).user.props";
			var avar = pige.AddImport(FILE);
			avar.Condition = makeExistsCondition(FILE);
			avar.Label = "LocalAppDataPlatform";
		}

		static string makeExistsCondition(string astr) {
			return "exists('" + astr + "')";
		}

		static void addImportGroup(ProjectRootElement root, string label, string condition, Blah blah) {
			var avar = root.CreateImportGroupElement();

			root.AppendChild(avar);
			if (!string.IsNullOrEmpty(label))
				avar.Label = label;
			if (!string.IsNullOrEmpty(condition))
				avar.Condition = condition;
			if (blah != null)
				blah(root, avar);
		}

		static void addLabeledGroup(ProjectRootElement root, string p) {
			var v0 = root.CreateImportGroupElement();

			v0.Label = p;
			root.AppendChild(v0);
		}

		static void createCfgProp(ProjectRootElement root, string typeDesc, bool isDebug) {
			var avar = root.CreatePropertyGroupElement();

			avar.Condition = makeCfgCondition(isDebug ? DEBUG : RELEASE, PLATFORM);
			avar.Label = "Configuration";
			root.AppendChild(avar);
			avar.AddProperty("ConfigurationType", typeDesc);
			avar.AddProperty("UseDebugLibraries", isDebug.ToString());
			if (!isDebug)
				avar.AddProperty("WholeProgramOptimization", "true");
			avar.AddProperty("CharacterSet", "MultiByte");
		}

		static void createGlobals(string ns, ProjectType type, Project p, string label) {
			var v2 = p.Xml.CreatePropertyGroupElement();
			Guid guid;
			string framework = type == ProjectType.XamlApp ? FRAMEWORK_452 : FRAMEWORK_4;

			v2.Label = label;
			p.Xml.AppendChild(v2);

			guid = Guid.NewGuid();
			if (reuseGuid)
				guid = new Guid("22662C87-995A-4027-A2A3-B289218B7F62");
			v2.AddProperty("ProjectGuid", "{" + guid + "}");
			if (type == ProjectType.WindowsForm) {
				v2.AddProperty("TargetFrameworkVersion", framework);
				v2.AddProperty("Keyword", "ManagedCProj");
			}
			v2.AddProperty("RootNamespace", string.IsNullOrEmpty(ns) ? "NSNone" : ns);
		}

		static void createItemGroup(Project p, string aLabel) {
			var v1 = p.Xml.CreateItemGroupElement();

			v1.Label = "ProjectConfigurations";
			v1.Label = aLabel;
			p.Xml.AppendChild(v1);
			addProjCfg(v1, DEBUG, PLATFORM);
			addProjCfg(v1, RELEASE, PLATFORM);
		}

		static string makeCfgCondition(string cfg, string plat) {
			return "'$(Configuration)|$(Platform)'=='" + cfg + "|" + plat + "'";
		}

		static void addCfgProp(Project p, string cfg, string plat, bool isDebug, string typeDesc) {
			var avar = p.Xml.AddPropertyGroup();
			avar.Condition = makeCfgCondition(cfg, plat);
			avar.AddProperty("ConfigurationType", typeDesc);
			avar.AddProperty("UseDebugLibraries", isDebug.ToString());
			if (!isDebug)
				avar.AddProperty("WholeProgramOptimization", true.ToString());
			avar.AddProperty("CharacterSet", "MultiByte");
		}

		static void addProjCfg(ProjectItemGroupElement v1, string cfg, string plat) {
			ProjectItemElement pie = v1.AddItem("ProjectConfiguration", cfg + "|" + plat);

			pie.AddMetadata("Configuration", cfg);
			pie.AddMetadata("Platform", plat);
		}
	}
}