using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace NSprojectgen {
    static class DefaultProjectGenerator {
        #region constants
        const string KEY_CFG = "Configuration";
        const string KEY_PLAT = "Platform";
        const string KEY_PLAT_TARG = "PlatformTarget";

        const string KEY_DEBUG = "Debug";
        const string KEY_RELEASE = "Release";
        const string KEY_PLAT_DEF = "x86";
        const string KEY_PLAT_ANY = "AnyCPU";
        const string KEY_PROD_VERSION = "8.0.30703";
        const string KEY_SCHEMA_VERSION = "2.0";

        const string TARGET_35 = "v3.5";
        const string TARGET_40 = "v4.0";
        const string TARGET_452 = "v4.5.2";
        const string TARGET_45 = "v4.5";

        public const string PROPERTY_DIR = "Properties";
        const string APP_CFG = "app.config";
        #endregion

        #region fields
        static bool createConfig = true;
        static bool createAsmInfo = true;
        #endregion

        internal static void generate(PGOptions opts, bool rebuild) {
            Project p = null;
            bool generate = false;

            if (File.Exists(opts.projectFileName))
                try {
                    if (opts.verbose)
                        Console.WriteLine("[verbose] rewriting: " + opts.projectFileName);
                    p = new Project(opts.projectFileName);
                } catch (Exception ex) {
                    Console.Error.WriteLine("failed to open " + opts.projectFileName + " [" + ex.Message + "]");
                }

            if (p == null) {
                p = new Project();
                generate = true;
            }
            if (rebuild) {
                p.Xml.RemoveAllChildren();
                generate = true;
            }
            doStuff(p, opts, generate);
            p.Save(opts.projectFileName);
            Console.WriteLine("saved: " + opts.projectFileName);
        }

        static void doStuff(Project p, PGOptions opts, bool generate) {
            string asmName = opts.assemblyName;

            p.Xml.DefaultTargets = "Build";
            addDefaultProperty(p, opts.projectType, opts.projectNamespace, opts.assemblyName);
            if (opts.projectType == ProjectType.XamlApp)
                p.Xml.AddImport("$(MSBuildExtensionsPath)\\$(MSBuildToolsVersion)\\Microsoft.Common.props");

            addPropertyGroup(p, true, opts.assemblyName, opts.projectType);
            addPropertyGroup(p, false, opts.assemblyName, opts.projectType);

            if (opts.projectType == ProjectType.XamlApp) {
                var vrefs = p.Xml.AddItemGroup();
                vrefs.Label = "references_here";
                // add refs here.
                string[] refNames = new string[] {
                    "System",
                    "System.Core",
                    "System.Data",
                    "System.Data.DataSetExtensions",
                    "System.Xml",
                    "System.Xml.Linq",
                    "System.Xaml",
                    "System.Net.Http",
                    "Microsoft.CSharp",
                    "WindowsBase",
                    "PresentationCore",
                    "PresentationFramework",
                };
                foreach (string refName in refNames)
                    vrefs.AddItem("Reference", refName);
                IDictionary<string, string> blah = new Dictionary<string, string>();
                blah.Add("RequiredTargetFramework", "4.0");
                vrefs.AddItem("Reference", "System.Xaml", blah);
            }
            if (opts.generateCode) {
                generateCommonFiles(p, opts);
                if (!opts.simplyProject)
                    generateFiles(p, opts);
            }

            if (opts.projectType == ProjectType.XamlApp) {
            } else {
                var vvv = p.Xml.AddItemGroup();
                vvv.Label = "References";
                vvv.AddItem("Reference", "System");
                if (!opts.simplyProject)
                    vvv.AddItem("Reference", "System.Data");
                if (opts.projectType == ProjectType.WindowsForm) {
                    vvv.AddItem("Reference", "System.Windows.Forms");
                    vvv.AddItem("Reference", "System.Drawing");
                }
                if (opts.doDevExpress) {
                    vvv.AddItem("Reference", "DevExpress.BonusSkins.v12.2");
                    vvv.AddItem("Reference", "DevExpress.Utils.v12.2");
                    vvv.AddItem("Reference", "DevExpress.XtraBars.v12.2");
                    vvv.AddItem("Reference", "DevExpress.XtraEditors.v12.2");
                    vvv.AddItem("Reference", "DevExpress.Data.v12.2");
                }
            }

            var p3 = p.Xml.AddImport("$(MSBuildToolsPath)\\Microsoft.CSharp.targets");
            if (opts.projectType != ProjectType.XamlApp) {
                var v0 = p.Xml.AddPropertyGroup();
                v0.AddProperty("StartupObject", string.Empty);
            }
        }

        static void generateCommonFiles(Project p, PGOptions opts) {
            ProjectItemGroupElement pige = null;
            string asmInfoName;

            if (createConfig && !opts.simplyProject) {
                createAppCfgFile(APP_CFG, opts);
                pige = createItemGroup(p, "None");
                pige.AddItem("None", APP_CFG);
            }

            if (createAsmInfo) {
                asmInfoName = PROPERTY_DIR + "\\" + "AssemblyInfo." + opts.provider.FileExtension;
                CSGenerator.createAsmInfoFile(
                    Path.Combine(Directory.GetCurrentDirectory(), asmInfoName),
                    opts.assemblyName, opts.assemblyVersion, opts);

                var avar = createItemGroup(p, "AssemblyInfo");
                avar.AddItem("Compile", asmInfoName);
            }
        }

        static ProjectItemGroupElement createItemGroup(Project p, string label) {
            ProjectItemGroupElement ret = p.Xml.AddItemGroup();

            if (!string.IsNullOrEmpty(label))
                ret.Label = label;
            return ret;
        }

        static void generateFiles(Project p, PGOptions opts) {
            ProjectItemGroupElement pige;

            pige = p.Xml.AddItemGroup();
            pige.Label = "IDK";

            if (opts.projectType == ProjectType.XamlApp) {
                XamlGenerator.generateFiles(p, opts, pige);
            } else {
                CSGenerator.generateMainFiles(p, opts, pige);
            }
        }

        static void createAppCfgFile(string filename, PGOptions opts) {
            XmlWriterSettings xws;

            if (File.Exists(filename))
                File.Delete(filename);

            xws = new XmlWriterSettings();
            xws.Indent = true;
            xws.IndentChars = new string(' ', 4);
            xws.Encoding = Encoding.ASCII;
            xws.Encoding = Encoding.UTF8;
            if (showOverwriteStatus(filename, opts))
                return;

            using (XmlWriter xw = XmlWriter.Create(filename, xws)) {
                xw.WriteStartDocument();
                xw.WriteStartElement("configuration");
                xw.WriteStartElement("startup");
                xw.WriteStartElement("supportedRuntime");
                string the_version = TARGET_40;
                xw.WriteAttributeString("sku", ".NETFramework,Version=" + (opts.projectType == ProjectType.XamlApp ? TARGET_45 : TARGET_452));
                if (opts.projectType == ProjectType.XamlApp)
                    the_version = TARGET_40;
                xw.WriteAttributeString("version", the_version);
                xw.WriteEndElement();
                xw.WriteEndElement();
                xw.WriteEndElement();
                xw.WriteEndDocument();
            }
        }

        internal static bool showOverwriteStatus(string filename, PGOptions opts) {
            if (File.Exists(filename)) {
                if (opts.forceNo) {
                    Console.WriteLine("not over-writing:" + filename);
                    return true;
                } else if (!opts.forceYes) {
                    if (dontOverwriteFile(filename)) {
                        Console.WriteLine("NOT overwriting " + filename);
                        return true;
                    }
                }
            }
            return false;
        }

        static bool dontOverwriteFile(string filename) {
            ConsoleKeyInfo response;
            char c = char.MinValue;

            do {
                if (c != char.MinValue)
                    Console.Out.WriteLine("Please respond with Y/N");
                Console.Out.Write("file " + filename + " already exists.  Overwrite (Y/N)? ");
                response = Console.ReadKey();
                Console.Out.WriteLine();
            } while ((c = char.ToUpper(response.KeyChar)) != 'Y' && c != 'N');
            return c == 'N';
        }

        static void addCreateProperty(ProjectTargetElement targetElement, string taskValue, string taskCondition, string taskOutputValue) {
            var v0 = targetElement.AddTask("CreateProperty");

            v0.SetParameter("Value", taskValue);
            if (!string.IsNullOrEmpty(taskCondition))
                v0.Condition = taskCondition;
            v0.AddOutputProperty("Value", taskOutputValue);
        }

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

        static void addPropertyGroup(Project p, bool isDebug, string aName, ProjectType type) {
            var p0 = p.Xml.AddPropertyGroup();
            string platform = type == ProjectType.XamlApp ? KEY_PLAT_ANY : KEY_PLAT_DEF;
            p0.Condition = " '$(" + KEY_CFG + ")|$(" + KEY_PLAT + ")' == '" + (isDebug ? KEY_DEBUG : KEY_RELEASE) + "|" + platform + "' ";

            p0.AddProperty(KEY_PLAT_TARG, platform);
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

        static void addDefaultProperty(Project p, ProjectType type, string ns, string aName) {
            string outType, platform;

            platform = (type == ProjectType.XamlApp) ? KEY_PLAT_ANY : KEY_PLAT_DEF;
            var p0 = p.Xml.AddPropertyGroup();
            var v0 = p0.AddProperty(KEY_CFG, KEY_DEBUG);
            v0.Condition = " '$(" + KEY_CFG + ")' == '' ";

            var v1 = p0.AddProperty(KEY_PLAT, platform);
            v1.Condition = " '$(" + KEY_PLAT + ")' == '' ";

            if (type != ProjectType.XamlApp) {
                p0.AddProperty("ProductVersion", KEY_PROD_VERSION);
                p0.AddProperty("SchemaVersion", KEY_SCHEMA_VERSION);
            }
            p0.AddProperty("ProjectGuid", "{" + Guid.NewGuid().ToString().ToUpper() + "}");
            switch (type) {
                case ProjectType.WindowsForm:
                case ProjectType.XamlApp: outType = "WinExe"; break;
                case ProjectType.ConsoleApp: outType = "Exe"; break;
                case ProjectType.ClassLibrary: outType = "Library"; break;
                default: throw new InvalidOperationException("unhandled project-projectType: " + type);
            }
            p0.AddProperty("OutputType", outType);
            p0.AddProperty("AppDesignerFolder", PROPERTY_DIR);
            p0.AddProperty("RootNamespace", string.IsNullOrEmpty(ns) ? "nsdefault" : ns);
            p0.AddProperty("AssemblyName", aName);
            p0.AddProperty("TargetFrameworkVersion", type == ProjectType.XamlApp ? TARGET_45 : TARGET_40);
            p0.AddProperty("FileAlignment", "512");
            if (type == ProjectType.XamlApp) {
                p0.AddProperty("ProjectTypeGuids", "{60dc8134-eba5-43b8-bcc9-bb4bc16c2548};{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}");
                p0.AddProperty("WarningLevel", "4");
                p0.AddProperty("AutoGenerateBindingRedirects", "true");
            } else {
                p0.AddProperty("TargetFrameworkProfile", string.Empty);
            }
        }
    }
}