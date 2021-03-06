using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

// need reference: Microsoft.VisualC.VSCodeProvider
// need reference: MCppCodeProvider


// -f WpfApplication -tx -g -xn -xf Page0 -xf Page1
// -g -f MyApp -tx   -xn -xf Page1
// -tx -g -xf dummy

/*
 * args: -f ScannerBeep -tx -Vy  -xw -g
 * Path: C:\Users\rtcousens\source\DevOps\ScannerBeep
*/

    /*
     * */
namespace NSprojectgen {

    class driver {
        /// <summary>entry-point</summary>
        /// <remarks>remarks</remarks>
        static void Main(string[] args) {
            int exitCode = 0;
            PGOptions opts = new PGOptions();
            string anArg;
            int nargs;
            int len;
            string atype;
            bool fixNS = false;
            bool showHelp = false;
#if TRACE
            Trace.Listeners.Add(new TextWriterTraceListener(Console.Out, PGOptions.LISTENER_NAME_2));
#endif
            if ((nargs = args.Length) > 0)
                for (int i = 0; i < nargs; i++) {
                    anArg = args[i];
                    if ((len = anArg.Length) >= 2) {
                        if (anArg[0] == '-' || anArg[0] == '/') {
                            switch (anArg[1]) {
                                case 'f':
                                    if (len > 2) opts.assemblyName = anArg.Substring(2).Trim();
                                    else { opts.assemblyName = args[i + 1]; i++; }
                                    opts.calculateNamespace();
                                    break;
                                case 'N':
                                    if (len > 2) opts.setNamespace(anArg.Substring(2).Trim());
                                    else { opts.setNamespace(args[i + 1]); i++; }
                                    fixNS = false;
                                    break;
                                case 'v':
                                    if (len > 2) opts.assemblyVersion = anArg.Substring(2).Trim();
                                    else { opts.assemblyVersion = args[i + 1]; i++; }
                                    break;
                                case 't':
                                    if (len > 2) atype = anArg.Substring(2).Trim();
                                    else { atype = args[i + 1]; i++; }
                                    switch (atype) {
                                        case "c": opts.projectType = ProjectType.ConsoleApp; break;
                                        case "d": opts.projectType = ProjectType.ClassLibrary; break;
                                        case "w": opts.projectType = ProjectType.WindowsForm; break;
                                        case "x": opts.projectType = ProjectType.XamlApp; if (opts.xamlType == XamlWindowType.NONE) opts.xamlType = XamlWindowType.RegularWindow; break;
                                        default: Console.Error.WriteLine("unknown project-projectType '" + atype + "'!"); break;
                                    }
                                    break;
                                case 'x':
                                    if (len > 2) atype = anArg.Substring(2).Trim();
                                    else { atype = args[i + 1]; i++; }
                                    switch (atype) {
                                        case "f":
                                            if (len > 3) { atype = anArg.Substring(2).Trim(); opts.addXmlPage(atype); } else { opts.addXmlPage(atype = args[i + 1]); i++; }
                                            break;
                                        case "n": opts.xamlType = XamlWindowType.NavigationWindow; break;
                                        case "w": opts.xamlType = XamlWindowType.RegularWindow; break;
                                        default: Console.Error.WriteLine("unknown xaml-type '" + atype + "'!"); opts.xamlType = XamlWindowType.RegularWindow; break;
                                    }
                                    break;
                                case 'b': opts.isVB = true; break;
                                case 'C': opts.isCPPProject = true; break;
                                case 'D': opts.doDevExpress = true; break;
                                case 'g': opts.generateCode = true; break;
                                case 'n': opts.forceNo = true; break;
                                //case 'p': opts.usePhibroStyle = true; break;
                                case 's': opts.simplyProject = true; break;
                                case 'V': opts.verbose = true;break;
                                case 'y': opts.forceYes = true; break;
                                case 'h': showHelp = true; break;
                                case '?': showHelp = true; break;
                            }
                        }
                    }
                }
            // -C C++ project
            // -D devexpress
            // -g generate-code
            // -p phibro-style
            // -s simplyProject
            //if (opts.simplyProject)
            //    opts.usePhibroStyle = false;
            if (showHelp)
                showUserHelp(Console.Error, Assembly.GetEntryAssembly());
            else {
                opts.projectFileName = opts.assemblyName + (opts.isCPPProject ? ".vcxproj" : (opts.isVB ? ".vbproj" : ".csproj"));
                if (fixNS)
                    opts.calculateNamespace();
                try {
                    opts.createProvider();
                    if (opts.isCPPProject)
                        CProjectGenerator.generate(opts);
                    else
                        DefaultProjectGenerator.generate(opts, true);
                } catch (Exception ex) {
                    Console.Error.WriteLine("[ERROR] " + ex.Message);
                    Trace.WriteLine("[TRACE] " + ex.Message);
                    Console.Error.Write("awaiting <ENTER>:");
                    Console.ReadLine();
                    exitCode = 1;
                }
            }
#if TRACE
            Trace.Flush();
            Trace.Listeners.Remove(PGOptions.LISTENER_NAME_2);
#endif
            Environment.Exit(exitCode);
        }

        static void showUserHelp(TextWriter tw, Assembly a) {
            AssemblyName an = a.GetName();

            tw.WriteLine(an.Name + " (v " + an.Version + ")");
            tw.WriteLine("usage:");
            tw.WriteLine("\t" + Path.GetFileNameWithoutExtension(a.Location) +
                ": -[f projectFileName] -[N namespace] -[v assemblyVersion] -[t c/d/x/w] [-bDgpsV] [-x [n/w]] [-xf page ...]\n");
            tw.WriteLine("-b\tgenerate VB.");
            tw.WriteLine("-C\tgenerate C++ project.");
            tw.WriteLine("-D\tgenerate DevExpress project.");
            tw.WriteLine("-g\tgenerate code.");
            //tw.WriteLine("-p\tgenerate phibro-style project.");
            tw.WriteLine("-n\tforce 'NO' to file-overwrite.");
            tw.WriteLine("-s\tgenerate simply project.");
            tw.WriteLine("-V\tverbose processing.");
            tw.WriteLine("-y\tforce 'YES' to file-overwrite.");
            tw.WriteLine("-tc\tgenerate console application.");
            tw.WriteLine("-td\tgenerate dll.");
            tw.WriteLine("-tx\tgenerate XAML application.");
            tw.WriteLine("-tw\tgenerate Windows application.");
            tw.WriteLine("-xn\tXAML navigation-window.");
            tw.WriteLine("-xw\tXAML normal window.");
        }
    }
}