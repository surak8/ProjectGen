using System;
using System.Diagnostics;
using System.Reflection;

namespace NSprojectgen {
    /// <summary>logger class.</summary>
    public static class Logger {
        /// <summary>Indicate execution of a method.</summary>
        /// <param name="mb"></param>
        public static void log(MethodBase mb) {
            log(makeSig(mb));
        }

        /// <summary>create a description from a <seealso cref="MethodBase"/> instance.</summary>
        /// <param name="mb"></param>
        /// <returns></returns>
        public static string makeSig(MethodBase mb) {
            return mb.ReflectedType.Name + "." + mb.Name;
        }

        /// <summary>log a message.</summary>
        /// <param name="msg"></param>

        public static void log(string msg) {
#if DEBUG
			Debug.WriteLine("[DEBUG] " + msg);
#else
#if TRACE
			Trace.WriteLine("[TRACE] " + msg);
#endif
#endif
        }

		internal static void log(MethodBase methodBase, string v) {
			log(makeSig(methodBase) + ":" + v);
		}
	}
}