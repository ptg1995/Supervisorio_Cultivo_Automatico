using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Ports;

namespace ESTUFA_AUTOMATIZADA_REV1
{
    static class Program
    {
        public static InterfaceDrvArduino InterfaceDrvArduino;
        public static SerialPort SerialPort;
        public static frmEstufa FrmEstufa;
        /// <summary>
        /// Ponto de entrada principal para o aplicativo.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            OnStart();
            Application.Run(FrmEstufa);
        }
        static void OnStart()
        {
            SerialPort = new SerialPort();
            FrmEstufa = new frmEstufa();
            InterfaceDrvArduino = new InterfaceDrvArduino();
        }
    }
}
