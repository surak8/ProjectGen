using Microsoft.Build.Evaluation;
using System.IO;
using System;
using System.Diagnostics;
using Microsoft.Build.Construction;
using System.Collections.Generic;
using System.Text;
namespace NSprojectgen {
    static class MyProjectGenerator {
        internal static void generate(string outputFile,string szVersion,string asmName,string ns,ProjectType type,bool rebuild) {
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
            doStuff(p,szVersion,asmName,ns,type,generate);
            p.Save(outputFile);
            Debug.Print("saved: " + outputFile);
        }

        static void showStuff(Project p) {
            foreach (var avar in p.AllEvaluatedProperties) {
                Debug.Print(avar.Name + " = " + avar.UnevaluatedValue);
            }
            foreach (var avar2 in p.AllEvaluatedItems) {
                Debug.Print("here");
            }
        }

        static void showTargets(Project p) {
            foreach (var avar in p.Targets) {
                foreach (var avar2 in avar.Value.Children) {
                    Debug.Print("here");
                }
            }
        }

        const string ASM_INFO_FILE = "AssemblyInfo.cs";
        const string RULE_NAME_CREATE_PROPS = "precreateProps";
        const string RULE_NAME_BEFORE_RESOLVE = "BeforeResolveReferences";

        static void doStuff(Project p,string szVersion,string asmName,string ns,ProjectType type,bool generate) {
            string aFile;
            ProjectItemGroupElement pige2 = null;

            aFile = APP_DESIGN_FOLDER + "\\" + ASM_INFO_FILE;

            if (generate) {
                p.Xml.DefaultTargets = "Build";
                createDefaultPropertyGroup(p,asmName,ns,type);
                createConfigPropertyGroup(p,true);
                createConfigPropertyGroup(p,false);

                // references.
                ProjectItemGroupElement pige = p.Xml.AddItemGroup();
                pige.AddItem("Reference","System");
                pige.AddItem("Reference","System.Core");
                pige.AddItem("Reference","System.Xml");
                if (type == ProjectType.WindowsForm) {
                    pige.AddItem("Reference","System.Windows.Forms");
                    pige.AddItem("Reference","System.Drawing");
                }

                pige2 = p.Xml.AddItemGroup();
                pige2.AddItem("Compile",aFile);
            }

            CSGenerator.generateFiles(p,pige2,aFile,asmName,szVersion,type,ns);

            if (generate) {
                const string PEI_RULES = "MSPEIRules";
                // Custom Phibro properties here.
                ProjectPropertyGroupElement ppge = p.Xml.AddPropertyGroup();
                ppge.Label = "PhibroProperties";
                ppge.AddProperty(PEI_RULES,@"G:\MIS\icts\MSBuild\2010");
                ppge.AddProperty("_BaseAsmFolder",@"G:\MIS\phibro\shared\assemblies");
                ppge.AddProperty("SharedDir","$(_BaseAsmFolder)\\$(Configuration)");
                ppge.AddProperty("CopyAllFiles","false");

                const string COMMON_VERSION = "CommonVersion";
                const string ASM_VERSION = "AssemblyVersion";
                ppge.AddProperty(COMMON_VERSION,szVersion);

                var v1 = ppge.AddProperty(ASM_VERSION,"true");
                v1.Condition = "'$(" + ASM_VERSION + ")' == ''";
                var v2 = ppge.AddProperty("AssemblyBundleName","$(" + ASM_NAME + ")");
                ppge.AddProperty("Verbose","true");

                // Custom Phibro items here.
                ProjectItemGroupElement pige3 = p.Xml.AddItemGroup();
                pige3.Label = "BundlesToCopy";

                var v3 = pige3.AddItem("BundleToCopy","refs\\peidb.ref");
                v3.AddMetadata("BundleName","PEIDBProvider");
                v3.AddMetadata("BundleVersion","$(" + COMMON_VERSION + ")");
                v3.AddMetadata("HasMultipleItems","false");

                p.Xml.AddImport(@"$(MSBuildToolsPath)\Microsoft.CSharp.targets");
                p.Xml.AddImport("$(" + PEI_RULES + ")\\Phibro.CopyBundle.targets");
                p.Xml.AddImport("$(" + PEI_RULES + ")\\Phibro.CopyToShared.targets");
                p.Xml.AddImport("$(" + PEI_RULES + ")\\Phibro.LightWeightAssembly.targets");
                addPreCreateProps(p);
            }
        }

        static void addPreCreateProps(Project p) {

            const string LAF = "LocalAssemblyFolder";
            const string HTS = "HasTrailingSlash";
            const string OTIFP = "%(_OutputPathItem.FullPath)";
            const string BTC = "BundleToCopy";
            const string BN = "%(" + BTC + ".BundleName)";

            var avar = p.Xml.CreateTargetElement(RULE_NAME_CREATE_PROPS);
            p.Xml.AppendChild(avar);
            addCreateProperty(avar,p,LAF,OTIFP + "\\assemblies","!" + HTS + "('" + OTIFP + "')");
            addCreateProperty(avar,p,LAF,OTIFP + "assemblies",HTS + "('" + OTIFP + "')");
            addMessage(avar,p,"high",LAF + "=$(" + LAF + ")","'$(Verbose)'=='true'");

            ProjectTargetElement pte1;

            p.Xml.AppendChild(pte1 = p.Xml.CreateTargetElement(RULE_NAME_BEFORE_RESOLVE));
            pte1.DependsOnTargets = RULE_NAME_CREATE_PROPS + ";copyBundles";
            addCreateItem2(pte1,p,
                BN,
                "Reference",
                "'" + BN + "'!='' and '%(" + BTC + ".HasMultipleItems)'!='true'",
                new KeyValuePair<string,string>[] { 
                    new KeyValuePair<string,string>("Private","false"),
                    new KeyValuePair<string,string>("HintPath","$("+LAF+")\\"+BN+"\\%("+BTC+".BundleVersion)\\"+BN+".dll"),
                    new KeyValuePair<string,string>("Name",BN+".dll")
                });
        }

        const string CONFIG = "Configuration";
        const string PLAT = "Platform";
        const string DEFAULT_PLAT = "x86";
        const string ASM_NAME = "AssemblyName";

        static void createConfigPropertyGroup(Project p,bool p_2) {
            ProjectPropertyGroupElement ppge;

            ppge = p.Xml.AddPropertyGroup();
            ppge.Condition = "'$(" + CONFIG + ")|$(" + PLAT + ")' == '" + (p_2 ? "Debug" : "Release") + "|" + DEFAULT_PLAT + "'";

            ppge.AddProperty("PlatformTarget",DEFAULT_PLAT);
            if (p_2)
                ppge.AddProperty("DebugSymbols","true");
            ppge.AddProperty("DebugType",p_2 ? "full" : "none");
            ppge.AddProperty("Optimize",p_2 ? "false" : "true");
            ppge.AddProperty("OutputPath",".");
            ppge.AddProperty("DefineConstants",(p_2 ? "DEBUG;" : string.Empty) + "TRACE");
            ppge.AddProperty("ErrorReport","prompt");
            ppge.AddProperty("WarningLevel","4");
            ppge.AddProperty("UseVSHostingProcess","false");
        }

        static void createDefaultPropertyGroup(Project p,string asmName,string ns,ProjectType type) {
            ProjectPropertyGroupElement ppge;
            ProjectPropertyElement ppe;
            string outType;

            ppge = p.Xml.AddPropertyGroup();
            ppe = ppge.AddProperty(CONFIG,"Debug");
            ppe.Condition = "'$(" + CONFIG + ")' == ''";

            ppe = ppge.AddProperty(PLAT,DEFAULT_PLAT);
            ppe.Condition = "'$(" + PLAT + ")' == ''";

            ppe = ppge.AddProperty("ProductVersion","8.0.30703");
            ppe = ppge.AddProperty("SchemaVersion","2.0");
            ppe = ppge.AddProperty("ProjectGuid","{" + Guid.NewGuid().ToString().ToUpper() + "}");
            // Exe == Console
            // WinExe == WinApp
            // Library == dll
            switch (type) {
                case ProjectType.WindowsForm: outType = "WinExe"; break;
                case ProjectType.ConsoleApp: outType = "Exe"; break;
                case ProjectType.ClassLibrary: outType = "Library"; break;
                default: throw new InvalidOperationException("unhandled ProjectType " + type + "!");
            }
            ppe = ppge.AddProperty("OutputType",outType);
            ppe = ppge.AddProperty("AppDesignerFolder",APP_DESIGN_FOLDER);
            ppe = ppge.AddProperty("RootNamespace",ns);
            ppe = ppge.AddProperty(ASM_NAME,asmName);
            ppe = ppge.AddProperty("TargetFrameworkVersion","v4.0");
            ppe = ppge.AddProperty("FileAlignment","512");
            ppe = ppge.AddProperty("TargetFrameworkProfile",string.Empty);
        }

        const string APP_DESIGN_FOLDER = "Properties";

        static ProjectTaskElement addCreateItem2(
            ProjectTargetElement pte1,
            Project p,
            string value,
            string name,string condition,KeyValuePair<string,string>[] metaPairs) {
            ProjectTaskElement ret = pte1.AddTask("CreateItem");
            string tmp;

            ret.SetParameter("Include",value);
            if (!string.IsNullOrEmpty(condition))
                ret.Condition = condition;
            if (!string.IsNullOrEmpty(tmp = catenate(metaPairs,';')))
                ret.SetParameter("AdditionalMetadata",tmp);
            ret.AddOutputItem("Include",name);
            return ret;
        }

        static string catenate(KeyValuePair<string,string>[] metaPairs,char csep) {
            string ret = string.Empty;
            StringBuilder sb;
            int i = 0;

            if (metaPairs != null && metaPairs.Length > 0) {
                sb = new StringBuilder();

                foreach (KeyValuePair<string,string> kvp in metaPairs) {
                    if (i > 0 && csep != char.MinValue)
                        sb.Append(csep,1);
                    sb.Append(kvp.Key + "=" + kvp.Value);
                    i++;
                }
                ret = sb.ToString();
            }
            return ret;
        }

        static ProjectTaskElement addMessage(ProjectTargetElement avar,Project p,string priority,string text) {
            return addMessage(avar,p,priority,text,null);
        }

        static ProjectTaskElement addMessage(ProjectTargetElement avar,Project p,string priority,string text,string condition) {
            ProjectTaskElement ret = avar.AddTask("Message");

            ret.SetParameter("Importance",priority);
            ret.SetParameter("Text",text);
            if (!string.IsNullOrEmpty(condition))
                ret.Condition = condition;
            return ret;
        }

        static ProjectTaskElement addCreateProperty(ProjectTargetElement avar,Project p,string propName,string propValue) {
            return addCreateProperty(avar,p,propName,propValue,null);
        }

        static ProjectTaskElement addCreateProperty(ProjectTargetElement avar,Project p,string propName,string propValue,string condition) {
            var avar2 = avar.AddTask("CreateProperty");
            avar2.SetParameter("Value",propValue);
            avar2.AddOutputProperty("Value",propName);
            if (!string.IsNullOrEmpty(condition))
                avar2.Condition = condition;
            return avar2;
        }

        private static void addCreateItem(ProjectTargetElement avar,Project p) {
            throw new NotImplementedException();
        }

        static void addProperty(ProjectPropertyGroupElement ppge,string p,bool p_2,bool p_3) {
            addProperty(ppge,p,p_2 ? "true" : "false",p_3);
        }

        static void addProperty(ProjectPropertyGroupElement ppge,string p,Version version) {
            addProperty(ppge,p,version.ToString());
        }

        static void addProperty(ProjectPropertyGroupElement ppge,string key,string avlue,bool p_3) {
            ProjectPropertyElement ppe = addProperty(ppge,key,avlue);

            if (p_3)
                ppe.Condition = "'$(" + key + ")'==''";
        }

        static ProjectPropertyElement addProperty(ProjectPropertyGroupElement ppge,string key,string value) {
            ProjectPropertyElement ppe = ppge.AddProperty(key,value);
            return ppe;
        }
    }
}