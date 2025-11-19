using System;
using System.Windows.Forms;

namespace PivotViewer
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new PivotViewerForm());
        }
    }
}

