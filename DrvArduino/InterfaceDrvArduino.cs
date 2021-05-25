using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.IO.Ports;

namespace ESTUFA_AUTOMATIZADA_REV1
{
    class InterfaceDrvArduino
    {
        public StringBuilder msg;
        int count;
        int hora;
        int diaAtual = DateTime.Now.Day;
        int diaAnterior = DateTime.Now.Day;
        int horaAtual = DateTime.Now.Hour;
        int horaAnterior = DateTime.Now.Hour;
        public InterfaceDrvArduino()
        {
            Program.SerialPort.DataReceived += SerialPort_DataReceived;
        }
        public void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                hora = DateTime.Now.Minute;
                diaAnterior = DateTime.Now.Day;
                ResEstufaDAC dac = new ResEstufaDAC();
                string msgDataReceived = Program.SerialPort.ReadExisting();
                if (msgDataReceived.Length > 5)
                {
                    string[] msgAux = msgDataReceived.Split(';');
                    MsgDadosArduino msgDadosArduino = new MsgDadosArduino
                    {
                        Temperatura = double.Parse(msgAux[0])/100,
                        Umidade = int.Parse(msgAux[1]),
                        IntensidadeLuz = int.Parse(msgAux[2])

                    };
                    PublicaDados(msgDadosArduino);
                    if (diaAtual != diaAnterior || horaAtual != horaAnterior )
                    {
                        hora = 0; 
                        count = 0; 
                        diaAtual = DateTime.Now.Day;
                    }
                    if (hora - count >= 1)
                    {
                        dac.Insert(msgDadosArduino.Temperatura, msgDadosArduino.Umidade, msgDadosArduino.IntensidadeLuz);
                        count = DateTime.Now.Minute;
                        diaAtual = DateTime.Now.Day;
                    }
                }
                else if (msgDataReceived.StartsWith("B"))
                {
                    MsgDadosArduino msg = new MsgDadosArduino();
                    msg.Msg = msgDataReceived.Substring(0);
                    PublicaDados(msg);
                }
                else if (msgDataReceived.StartsWith("I"))
                {
                    MsgDadosArduino msg = new MsgDadosArduino();
                    msg.Msg = msgDataReceived.Substring(0);
                    PublicaDados(msg);
                }
                else if (msgDataReceived.StartsWith("V"))
                {
                    MsgDadosArduino msg = new MsgDadosArduino();
                    msg.Msg = msgDataReceived.Substring(0);
                    PublicaDados(msg);
                }
                else if (msgDataReceived.StartsWith("A"))
                {
                    MsgDadosArduino msg = new MsgDadosArduino();
                    msg.Msg = msgDataReceived.Substring(0);
                    PublicaDados(msg);
                }
            }
            catch (Exception ex)
            {
                EventTracer.Trace(ex, "InterfaceDrvArduino.DefineDadosArduino();");
            }
        }
        public string MontaMsgEnvioArduino(Int32 UmiMin, Int32 TempMax, Int32 TempMin, Int32 TmoMinLuz)
        {

            msg = new StringBuilder(4) ;
            msg.AppendFormat("{0}|", UmiMin);
            msg.AppendFormat("{0}|", TempMax);
            msg.AppendFormat("{0}|", TempMin);
            msg.AppendFormat("{0}|", TmoMinLuz);
            string msgAux = msg.ToString();
            return msgAux;
        }
        public void PublicaDados(MsgDadosArduino msg)
        {   
            if(msg.Umidade > 0 | msg.Temperatura > 0)
                Program.FrmEstufa.Atualizar(msg);
            else if (msg.Msg.StartsWith("B"))
            {
                Program.FrmEstufa.AtualizarBomba(msg.Msg);
            }
            else if (msg.Msg.StartsWith("I"))
            {
                Program.FrmEstufa.AtualizarLED(msg.Msg);
            }
            else if (msg.Msg.StartsWith("V"))
            {
                Program.FrmEstufa.AtualizarVentilador(msg.Msg);
            }
            else if (msg.Msg.StartsWith("A"))
            {
                Program.FrmEstufa.AtualizarAquecimento(msg.Msg);
            }
        }
    }
}
 