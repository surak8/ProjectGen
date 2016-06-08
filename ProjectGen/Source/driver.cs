using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

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

			Debug.Listeners.Add(new TextWriterTraceListener(Console.Out, PGOptions.LISTENER_NAME));
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
								case 'n':
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
										case "x": opts.projectType = ProjectType.XamlApp; opts.xamlType = XamlWindowType.RegularWindow; break;
										default: Console.Error.WriteLine("unknown project-projectType '" + atype + "'!"); break;
									}
									break;
								case 'x':
									if (len > 2) atype = anArg.Substring(2).Trim();
									else { atype = args[i + 1];i++; }
									switch (atype) {
										case "f":
											if (len > 3) { atype = anArg.Substring(2).Trim(); opts.addXmlPage(atype); } else { opts.addXmlPage(atype = args[i + 1]); i++; }
											break;
										case "n": opts.xamlType = XamlWindowType.NavigationWindow; break;
										case "w": opts.xamlType = XamlWindowType.RegularWindow; break;
										default: Console.Error.WriteLine("unknown xaml-type '" + atype + "'!"); opts.xamlType = XamlWindowType.RegularWindow; break;
									}
									break;
								case 'C': opts.isCPPProject = true; break;
								case 'D': opts.doDevExpress = true; break;
								case 'g': opts.generateCode = true; break;
								case 'p': opts.usePhibroStyle = true; break;
								case 's': opts.simplyProject = true; break;
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
			if (opts.simplyProject)
				opts.usePhibroStyle = false;
			if (showHelp)
				showUserHelp(Console.Error, Assembly.GetEntryAssembly());
			else {
				opts.projectFileName = opts.assemblyName + (opts.isCPPProject ? ".vcxproj" : ".csproj");
				if (fixNS)
					opts.calculateNamespace();
				try {
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
			Debug.Listeners.Remove(PGOptions.LISTENER_NAME);
			Environment.Exit(exitCode);
		}

		static void showUserHelp(TextWriter tw, Assembly a) {
			tw.WriteLine("usage:");
			tw.WriteLine("\t" + Path.GetFileNameWithoutExtension(a.Location) +
				": -[f projectFileName] -[n namespace] -[v assemblyVersion] -[t c/d/w] [-Dgps] [-x [n/w]] [-xf page ...]\n");
			tw.WriteLine("-C\tgenerate C++ project.");
			tw.WriteLine("-D\tgenerate DevExpress project.");
			tw.WriteLine("-g\tgenerate code.");
			tw.WriteLine("-p\tgenerate phibro-style project.");
			tw.WriteLine("-a\tgenerate simplyProject project.");
			tw.WriteLine("-xn\tXAML navigation-window.");
			tw.WriteLine("-xw\tXAML normal window.");
		}
	}
}