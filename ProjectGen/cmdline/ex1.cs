// http://www.informit.com/guides/content.aspx?g=dotnet&seqNum=735
using System;

class driver {
    [STAThread]
    public static void Main(string[] args) {
        ParseCommandLine(args,0);
    }

    // the cruncher program is fairly typical. 
    // Program options set by ParseCommandLine
    static bool cflag = false;
    static bool xFlag = false;
    static string cFilename = null;
    static int numRecs = 0; // default of 0 means all
    static string inputFilename = null;
    static string outputFilename = null;
    // Parses the command line options and sets program options.
    //
    // If the command line is valid, this method returns True,
    // and program options are set accordingly.
    //
    // If the command line is badly formed, this method displays an error message,
    // and returns False.
    static bool ParseCommandLine(string[] args,int istart) {
        int iarg;
        string option;

        if (istart >= args.Length)
            throw new ArgumentException("Starting index is beyond end of argument array.");

        iarg = istart;
        while (iarg < args.Length) {
            option = args[iarg];
            ++iarg;
            if (option[0] == '/') {
                switch (option) {
                    case "/c":
                        // /c option allows an optional filename
                        cflag = true;
                        if (iarg < args.Length) {
                            if (args[iarg][0] == '/') {
                                cFilename = args[iarg];
                                ++iarg;
                            }
                        }
                        break;
                    case "/n":
                        // /n option expects a numeric argument
                        if (iarg < args.Length) {
                            string snum = args[iarg];
                            ++iarg;
                            if (!int.TryParse(snum,out numRecs)) {
                                Console.WriteLine("ERROR: non-numeric argument supplied for /n option.");
                                return false;
                            }
                        } else {
                            Console.WriteLine("ERROR: Expected argument for /n option.");
                            return false;
                        }
                        break;
                    case "/x":
                    case "/x+":
                        // /x is a flag. /x and /x+ turn it on. /x- turns it off
                        xFlag = true;
                        break;
                    case "/x-":
                        xFlag = false;
                        break;
                    case "/?":
                        // /? just displays help message.
                        return false;
                    default:
                        Console.WriteLine("ERROR: Unknown option '{0}'",option);
                        return false;
                }
            } else {
                // unbound arguments
                if (inputFilename == null) {
                    inputFilename = option;
                } else if (outputFilename == null) {
                    outputFilename = option;
                } else {
                    Console.WriteLine("ERROR: excess arguments");
                    return false;
                }
            }
        }
        // Do any validation here, checking for required options, etc.
        if (inputFilename == null) {
            Console.WriteLine("ERROR: Expected input filename.");
            return false;
        }
        if (outputFilename == null) {
            Console.WriteLine("ERROR: Expected output filename.");
            return false;
        }
        return true;
    }
}