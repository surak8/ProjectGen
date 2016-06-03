using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace NSprojectgen {

	public class driver {
		/// <summary>entry-point</summary>
		/// <remarks>remarks</remarks>
		static void Main(string[] args) {
			int exitCode = 0;
			const string LISTENER_NAME = "dummy";
			ProjectType type = ProjectType.ConsoleApp;
			string ns, filename, version, asmName, anArg, atype;
			bool phibroStyle = false, showHelp = false, doDevExpress = false, simple = false, generateCode = false, cproject = false, fixNS = true;
			int nargs, len;

			asmName = LISTENER_NAME;
			filename = asmName + ".csproj";
			version = "1.0.0.0";
			ns = "NS" + asmName.Substring(0, 1).ToUpper() + asmName.Substring(1);

			Debug.Listeners.Add(new TextWriterTraceListener(Console.Out, LISTENER_NAME));

			if ((nargs = args.Length) > 0)

				for (int i = 0; i < nargs; i++) {
					anArg = args[i];
					if ((len = anArg.Length) >= 2) {
						if (anArg[0] == '-' || anArg[0] == '/') {
							switch (anArg[1]) {
								case 'f':
									if (len > 2) asmName = anArg.Substring(2).Trim();
									else { asmName = args[i + 1]; i++; }
									ns = "NS" + asmName.Substring(0, 1).ToUpper() + asmName.Substring(1);
									break;
								case 'n':
									if (len > 2) ns = anArg.Substring(2).Trim();
									else { ns = args[i + 1]; i++; }
									fixNS = false;
									break;
								case 'v':
									if (len > 2) version = anArg.Substring(2).Trim();
									else { version = args[i + 1]; i++; }
									break;
								case 't':
									if (len > 2) atype = anArg.Substring(2).Trim();
									else { atype = args[i + 1]; i++; }
									switch (atype) {
										case "c": type = ProjectType.ConsoleApp; break;
										case "d": type = ProjectType.ClassLibrary; break;
										case "w": type = ProjectType.WindowsForm; break;
										case "x": type = ProjectType.XamlApp; break;
										default: Console.Error.WriteLine("unknown project-type '" + atype + "'!"); break;
									}
									break;
								case 'C': cproject = true; break;
								case 'D': doDevExpress = true; break;
								case 'g': generateCode = true; break;
								case 'p': phibroStyle = true; break;
								case 's': simple = true; break;
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
			// -s simple
			if (simple)
				phibroStyle = false;
			if (showHelp)
				showUserHelp(Console.Error, Assembly.GetEntryAssembly());
			else {
				filename = asmName + (cproject ? ".vcxproj" : ".csproj");
				if (fixNS)
					ns = "NS" + asmName.Substring(0, 1).ToUpper() + asmName.Substring(1);

				if (cproject)
					CProjectGenerator.generate(filename, version, asmName, ns, type);
				else
					DefaultProjectGenerator.generate(filename, version, asmName, ns, type, true, phibroStyle, doDevExpress, simple, generateCode);
			}
			Debug.Listeners.Remove(LISTENER_NAME);
			Environment.Exit(exitCode);
		}

		static void showUserHelp(TextWriter tw, Assembly a) {
			tw.WriteLine("usage:");
			tw.WriteLine("\t" + Path.GetFileNameWithoutExtension(a.Location) +
				": -[f filename] -[n namespace] -[v version] -[t c/d/w] [-Dgps]\n");
			tw.WriteLine("-C\tgenerate C++ project.");
			tw.WriteLine("-D\tgenerate DevExpress project.");
			tw.WriteLine("-g\tgenerate code.");
			tw.WriteLine("-p\tgenerate phibro-style project.");
			tw.WriteLine("-a\tgenerate simple project.");
		}
	}
}