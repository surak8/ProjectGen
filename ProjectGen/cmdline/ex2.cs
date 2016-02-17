// SimpleOptionParser.cs
// Jim Mischel, 2009/03/09
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Step1 {
    public class SimpleOptionParser {
        // option types:
        // flags that turn something on (i.e. /x)
        // flags that toggle (i.e. /x, /x+, /x-)
        // options with required arguments (i.e. /a filename)
        // options with optional arguments (i.e. /b [filename])
        // unbound arguments
        enum OptionType {
            Flag,
            ToggleFlag,
            RequiredArg,
            OptionalArg
        };

        // Internal option definition.
        class OptionDef {
            public string Option { get; private set; }
            public string Arg { get; private set; }
            public OptionType Kind { get; private set; }

            public OptionDef(string opt,string argName,OptionType ot) {
                Option = opt;
                Arg = argName;
                Kind = ot;
            }
        }

        /// <summary>
        /// Descriptions of accepted options.
        /// </summary>
        private List<OptionDef> optionDefs;

        /// <summary>
        /// The command line arguments to be parsed.
        /// </summary>

        private IList<string> args;

        /// <summary>
        /// Current position in the command line arguments list.
        /// </summary>
        private int iarg;

        public const char DefaultOptionChar = '/';

        /// <summary>
        /// The character used to identify options.
        /// </summary>

        public char OptionChar { get; private set; }

        /// <summary>
        /// The most recent option parsed by GetOption.
        /// </summary>
        public string Option { get; private set; }

        /// <summary>
        /// The Argument (in any) associated with the most recent option.
        /// </summary>
        public string Argument { get; private set; }

        /// <summary>
        /// Error message generated if GetOpt returns false.
        /// </summary>
        public string Error { get; private set; }

        /// <summary>
        /// End of file flag set when all options are exhausted
        /// </summary>
        public bool Eof { get; private set; }

        public SimpleOptionParser(IList<string> options)
            : this(options,DefaultOptionChar) {
        }

        public SimpleOptionParser(IList<string> options,char optChar) {
            OptionChar = optChar;
            optionDefs = ParseOptions(options);
        }

        public void SetArgs(IList<string> args,int istart) {
            if (args == null) {
                throw new ArgumentException("Arguments array must not be null.");
            }
            this.args = args;
            Reset(istart);
        }

        public void Reset(int iStart) {
            if (args == null) {
                throw new ApplicationException("No arguments array.");
            }
            if (iStart < 0 || iStart > args.Count) {
                throw new ArgumentException("Starting index is outside the bounds of the arguments array.");
            }
            Option = string.Empty;
            Argument = string.Empty;
            Error = string.Empty;
            iarg = iStart;
            Eof = false;
        }

        private bool SetError(string msg) {
            Error = msg;
            return false;
        }

        private bool SetError(string msg,params object[] args) {
            return SetError(string.Format(msg,args));
        }

        /// <summary>
        /// Get the next option/argument pair from the command line.
        /// </summary>
        /// <returns>
        /// Returns True if an argument was successfully parsed.
        /// If True is returned, then the Option property contains the parsed option,
        /// and the Argument property contains the parsed option.
        /// Returns False on error or end of options.
        /// If end of options is reached, Eof is set to True.
        /// If Eof is False, then the Error property contains the error.
        /// </returns>
        public bool GetOption() {
            Option = string.Empty;
            Argument = string.Empty;
            Error = string.Empty;

            if (Eof) {
                return SetError("End of options");
            }

            if (iarg >= args.Count) {
                Eof = true;
                return false;
            }

            string opt = args[iarg];
            string arg = string.Empty;
            char toggle = (char) 0;

            if (opt[0] == OptionChar) {
                // for toggles, strip the switch if it's there, and modify the option
                if (opt[opt.Length - 1] == '+' || opt[opt.Length - 1] == '-') {
                    toggle = opt[opt.Length - 1];
                    opt = opt.Substring(0,opt.Length - 1);
                }
                // find this option in the list of optionDefs
                OptionDef def = optionDefs.Find((o) => o.Option == opt);
                if (def == null) {
                    return SetError("Unrecognized Option: {0}",opt);
                }
                if (toggle != (char) 0 && def.Kind != OptionType.ToggleFlag) {
                    return SetError("{0} option is not a toggle flag.",opt);
                }
                switch (def.Kind) {
                    case OptionType.ToggleFlag:
                        arg = new string(toggle,1);
                        break;
                    case OptionType.Flag:
                        break;
                    case OptionType.RequiredArg:
                        ++iarg;
                        if (iarg < args.Count) {
                            arg = args[iarg];
                            if (arg[0] == OptionChar) {
                                // it's an option. Sorry, that's an error
                                return SetError("Expected {0} argument for {1} option",def.Arg,opt);
                            }
                        } else {
                            return SetError("Expected {0} argument for {1} option",def.Arg,opt);
                        }
                        break;
                    case OptionType.OptionalArg:
                        if (iarg < args.Count - 1 && args[iarg + 1][0] != OptionChar) {
                            // optional argument was specified
                            ++iarg;
                            arg = args[iarg];
                        }
                        break;
                    default:
                        throw new ApplicationException("This can't happen!");
                }
            } else {
                // It's not an option. Must be a free argument.
                arg = opt;
                opt = string.Empty;
            }
            ++iarg;
            Option = opt;
            Argument = arg;

            return true;
        }

        /// <summary>
        /// Parses the array of options definitions to create internal list of OptionDef objects.
        /// </summary>
        /// <param name="options">An IList of options definitions.</param>
        /// <returns>
        /// A list of OptionDef structures that are used by the GetOption method.
        /// </returns>

        private List<OptionDef> ParseOptions(IList<string> options) {
            List<OptionDef> defs = new List<OptionDef>(options.Count);
            string pattern = string.Format(@"^(?<opt>{0}[^\s]+)(?:\s+(?<arg>[^\s]+))?$",Regex.Escape(OptionChar.ToString()));
            Regex reOpt = new Regex(pattern);
            foreach (string opt in options) {
                Match m = reOpt.Match(opt);
                if (!m.Success) {
                    throw new ArgumentException(string.Format("Bad option string: '{0}'",opt));
                }
                string sopt = m.Groups["opt"].Value;
                string sarg = m.Groups["arg"].Value;
                OptionType k;
                if (string.IsNullOrEmpty(sarg)) {
                    k = OptionType.Flag;
                } else if (sarg == "+-") {
                    k = OptionType.ToggleFlag;
                } else if (sarg[0] == '[') {
                    if (sarg[sarg.Length - 1] == ']') {
                        k = OptionType.OptionalArg;
                    } else {
                        throw new ArgumentException(string.Format("Bad option string: '{0}'",opt));
                    }
                } else {
                    k = OptionType.RequiredArg;
                }
                defs.Add(new OptionDef(sopt,sarg,k));
            }
            return defs;
        }
    }

    class driver {
        //As you can see here, the syntax to describe the options is very brief:
        static readonly string[] ProgramOptions = {
            "/c [filename]",    // optional argument
            "/n number",        // required number
            "/x +-",            // toggle flag
            "/?"                // flag
        };

        [STAThread]
        public static void Main(string[] args) {
            var avar = new SimpleOptionParser(ProgramOptions);
            avar.SetArgs(args,0);
            while (avar.GetOption()) {
                System.Diagnostics.Debug.Print("here");
            }
        }
    }
}