//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.18444
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel;
using System.Windows.Forms;


namespace NSdummy {
    
    public partial class dummy2Form {
        public dummy2Form() {
            InitializeComponent();
        }
        void exitClick(object sender, System.EventArgs ea) {
            CancelEventArgs cea = new CancelEventArgs();

            Application.Exit(cea);
            if (cea.Cancel) {
                return;
            }
            Application.Exit();
        }
        [STAThread()]
        public static void Main(string[] args) {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new dummy2Form());
        }
    }
}
