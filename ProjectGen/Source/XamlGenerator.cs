using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;

namespace NSprojectgen {
    static class XamlGenerator {
        const string WIN_NAME = "MainWindow";

        internal static void generateFiles(Project p, PGOptions opts1, ProjectItemGroupElement pige) {
            Dictionary<string, string> tmp = new Dictionary<string, string>();
            WinDataProvider wdp = new WinDataProvider(WIN_NAME, opts1.projectNamespace, opts1.xamlType == XamlWindowType.RegularWindow);
            AppDataProvider apd = new AppDataProvider(wdp.fileName, opts1.projectNamespace);
            HomeDataProvider hdp = null;
            GeneralPage gp;
            string tmp2;

            XamlFileGenerator.generateFile(apd, opts1);
            XamlFileGenerator.generateFile(wdp, opts1);

            if (opts1.xamlType == XamlWindowType.NavigationWindow) {
                hdp = new HomeDataProvider(wdp.homePage, opts1.projectNamespace);
                XamlFileGenerator.generateFile(hdp, opts1);
                generatePageAndModel(pige, hdp);
            }

            if (!string.IsNullOrEmpty(tmp2 = wdp.viewModelName) && File.Exists(tmp2))
                generateCompile(pige, tmp2);

            generateApp(pige, apd);
            generatePage(pige, wdp);

            if (opts1.xamlPages.Count > 0) {
                foreach (string aPageName in opts1.xamlPages) {
                    gp = new GeneralPage(aPageName, opts1.projectNamespace);
                    XamlFileGenerator.generateFile(gp, opts1);
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
                tmp.Add("DependentUpon", Path.GetFileName(depName));
            tmp.Add("SubType", "Code");
            generateCompile(pige, fname, tmp);

        }

        static void generatePage(ProjectItemGroupElement pige, IXamlFileGenerationData ixfgd) {
            generateNode(pige, ixfgd.xamlName, "Designer", "Page");
            generateDependentCompile(pige, ixfgd.codeBehindName, ixfgd.xamlName);
        }

        static void generateNode(ProjectItemGroupElement pige, string fname, string genType, string itemType) {
            IDictionary<string, string> tmp = new Dictionary<string, string>();

            tmp.Add("Generator", "MSBuild:Compile");
            tmp.Add("SubType", genType);
            pige.AddItem(itemType, fname, tmp);

            Console.Error.WriteLine("[XXXX] adding: " + fname);
        }

        static void generateApp(ProjectItemGroupElement pige, IXamlFileGenerationData ixfgd) {
            generateNode(pige, ixfgd.xamlName, "Generator", "ApplicationDefinition");
            generateDependentCompile(pige, ixfgd.codeBehindName, ixfgd.xamlName);
        }

        static void generateCompile(ProjectItemGroupElement pige, string tmp2) {
            generateCompile(pige, tmp2, null);
        }

        static void generateCompile(ProjectItemGroupElement pige, string tmp2, IDictionary<string, string> tmp) {
            pige.AddItem("Compile", tmp2, tmp);
            Console.Error.WriteLine("[XXXX] adding: " + tmp2);
        }
    }
}