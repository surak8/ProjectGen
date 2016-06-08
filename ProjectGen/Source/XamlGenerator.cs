using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;

namespace NSprojectgen {
    static class XamlGenerator {
        const string WIN_NAME = "MainWindow";

        internal static void generateFiles(Project p, PGOptions opts1, ProjectItemGroupElement pige, CodeDomProvider cdp, PGOptions opts2) {
            Dictionary<string, string> tmp = new Dictionary<string, string>();
            WinDataProvider wdp = new WinDataProvider(WIN_NAME, opts1.projectNamespace, opts2.xamlType == XamlWindowType.RegularWindow);
            AppDataProvider apd = new AppDataProvider(wdp.fileName, opts1.projectNamespace);
            HomeDataProvider hdp = null;
            string tmp2;

            XamlFileGenerator.generateFile(apd);
            XamlFileGenerator.generateFile(wdp);

            if (opts2.xamlType == XamlWindowType.NavigationWindow) {
                hdp = new HomeDataProvider(wdp.homePage, opts1.projectNamespace);
                XamlFileGenerator.generateFile(hdp);
                generatePageAndModel(pige, hdp);
            }

            if (!string.IsNullOrEmpty(tmp2 = wdp.viewModelName) && File.Exists(tmp2)) {
                generateCompile(pige, tmp2);
            }

            generateApp(pige, apd);
            generatePage(pige, wdp);

            if (opts2.xamlPages.Count > 0) {
                GeneralPage gp;
                foreach (string aPageName in opts2.xamlPages) {
                    gp = new GeneralPage(aPageName, opts2.projectNamespace);
                    XamlFileGenerator.generateFile(gp);
                    generatePageAndModel(pige, gp);
                }
            }
        }

        static void generatePageAndModel(ProjectItemGroupElement pige, IXamlFileGenerationData hdp) {
            generatePage(pige, hdp);
            generateCompile(pige, hdp.viewModelName);
        }

        static void generateDependentCompile(ProjectItemGroupElement pige, string fname, string depName) {
            IDictionary<string, string> tmp = new Dictionary<string, string>();

            if (!string.IsNullOrEmpty(depName))
                tmp.Add("DependentUpon", depName);
            tmp.Add("SubType", "Code");
            generateCompile(pige, fname, tmp);

        }

        static void generatePage(ProjectItemGroupElement pige, IXamlFileGenerationData ixfgd) {

            //            string genType = "Designer";
            //          string itemType = "Page";

            generateNode(pige, ixfgd.xamlName, "Designer", "Page");
            generateDependentCompile(pige, ixfgd.codeBehindName, ixfgd.xamlName);
        }

          static void generateNode(ProjectItemGroupElement pige, string fname, string genType, string itemType) {
            IDictionary<string, string> tmp = new Dictionary<string, string>();

            tmp.Add("Generator", "MSBuild:Compile");
            tmp.Add("SubType", genType);
            pige.AddItem(itemType, fname, tmp);

            Debug.Print("Adding: " + fname);
        }

        static void generateApp(ProjectItemGroupElement pige, IXamlFileGenerationData ixfgd) {
#if true
            generateNode(pige, ixfgd.xamlName, "Generator", "ApplicationDefinition");
#else
            IDictionary<string, string> tmp = new Dictionary<string, string>();

            tmp.Add("Generator", "MSBuild:Compile");
            tmp.Add("SubType", "Generator");
            pige.AddItem("ApplicationDefinition", ixfgd.xamlName, tmp);
            Debug.Print("Adding: " + ixfgd.xamlName);
#endif
            generateDependentCompile(pige, ixfgd.codeBehindName, ixfgd.xamlName);
        }

        static void generateCompile(ProjectItemGroupElement pige, string tmp2) {
            generateCompile(pige, tmp2, null);
        }
        static void generateCompile(ProjectItemGroupElement pige, string tmp2, IDictionary<string, string> tmp) {
            pige.AddItem("Compile", tmp2, tmp);
            Debug.Print("Adding: " + tmp2);
        }
    }
}