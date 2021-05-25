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
    public partial class frmEstufa : Form
    {
        // Variáveis Privadas
        private string[] m_PortName;
        // Variaveis publicas
        RegEstufaDAO dao = new RegEstufaDAO();
        RegEstufaDAC dacReg = new RegEstufaDAC();
        MsgDadosArduino msgArduino = new MsgDadosArduino();
        public string[] NomePorta
        {
            get { return m_PortName; }
            set { m_PortName = value; }
        }
        public frmEstufa()
        {
            InitializeComponent();
            PreencheListaPortas();
        }
        public enum Relatorio
        {
            RelUmidade = 0x0001,

            RelTemperatura = 0x0002,

            RelIluminacao = 0x0004,

            //reserva = 0x0008
        }
        public void Atualizar(MsgDadosArduino msg)
        {
            MethodInvoker mth = (MethodInvoker)delegate ()
            {
                try
                {
                    if (msg.ToString().Length > 4)
                    {
                        txtTempAtualEstuf.Text = msg.Temperatura.ToString()+" °C";
                        lblFuncLED.Text = msg.IntensidadeLuz.ToString() == "1" ? "ON" : "OFF";
                        lblFuncLED.ForeColor = msg.IntensidadeLuz.ToString() == "1" ? Color.Green : Color.Red;
                        txtUmidadeAtualEstuf.Text = msg.Umidade.ToString()+" %";
                    }
                    
                }
                catch (Exception exc)
                {
                    EventTracer.Trace(exc);
                }
            };
            if (!this.IsHandleCreated)
                mth.Invoke();
            else
                this.BeginInvoke(mth);
        }
        public void AtualizarBomba(string msg)
        {
            MethodInvoker mth = (MethodInvoker)delegate ()
            {
                try
                {
                    lblEstadoBomba.Text = msg == "BON" ? "ON" : "OFF";
                    lblEstadoBomba.ForeColor = msg == "BOFF" ? Color.Red : Color.Green;
                    btnAtivarIrrig.Text = msg == "BOFF" ? "Ativar Irrigação" : "Desativar Irrigação";
                }
                catch (Exception exc)
                {
                    EventTracer.Trace(exc);
                }
            };
            if (!this.IsHandleCreated)
                mth.Invoke();
            else
                this.BeginInvoke(mth);
        }
        public void AtualizarLED(string msg)
        {
            MethodInvoker mth = (MethodInvoker)delegate ()
            {
                try
                {
                    lblFuncLED.Text = msg == "ION" ? "ON" : "OFF";
                    lblFuncLED.ForeColor = msg == "IOFF" ? Color.Red : Color.Green;
                    btnAtivarIlu.Text = msg == "IOFF" ? "Ativar Iluminação" : "Desativar Iluminação";
                }
                catch (Exception exc)
                {
                    EventTracer.Trace(exc);
                }
            };
            if (!this.IsHandleCreated)
                mth.Invoke();
            else
                this.BeginInvoke(mth);
        }
        public void AtualizarVentilador(string msg)
        {
            MethodInvoker mth = (MethodInvoker)delegate ()
            {
                try
                {
                    lblFuncVentilador.Text = msg == "VON" ? "ON" : "OFF";
                    lblFuncVentilador.ForeColor = msg == "VOFF" ? Color.Red : Color.Green;
                    btnAtivarVentilador.Text = msg == "VOFF" ? "Ativar Ventilador" : "Desativar Ventilador";
                }
                catch (Exception exc)
                {
                    EventTracer.Trace(exc);
                }
            };
            if (!this.IsHandleCreated)
                mth.Invoke();
            else
                this.BeginInvoke(mth);
        }
        public void AtualizarAquecimento(string msg)
        {
            MethodInvoker mth = (MethodInvoker)delegate ()
            {
                try
                {
                    lblFuncAquecimento.Text = msg == "AON" ? "ON" : "OFF";
                    lblFuncAquecimento.ForeColor = msg == "AOFF" ? Color.Red : Color.Green;
                    btnAtivarAquecimento.Text = msg == "AOFF" ? "Ativar Aquecimento" : "Desativar Aquecimento";
                }
                catch (Exception exc)
                {
                    EventTracer.Trace(exc);
                }
            };
            if (!this.IsHandleCreated)
                mth.Invoke();
            else
                this.BeginInvoke(mth);
        }
        private void cboPortas_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(Program.SerialPort.PortName))
            {
                Program.SerialPort.PortName = cboPortas.Text;
                Program.SerialPort.Open();
            }
            else if (!String.IsNullOrEmpty(Program.SerialPort.PortName) &&
                Program.SerialPort.PortName != cboPortas.Text)
            {
                Program.SerialPort.Close();
                Program.SerialPort.PortName = cboPortas.Text;
                Program.SerialPort.Open();
            }
            else
            {
                Program.SerialPort.PortName = cboPortas.Text;
                Program.SerialPort.Open();
            }
                
        }
        public void PreencheListaPortas()
        {
            foreach (string s in SerialPort.GetPortNames()) cboPortas.Items.Add(s);
        }
        public void LimpaFormularioReg()
        {
            txtPlanta.Text = "";
            txtTempMax.Text = "";
            txtTempMin.Text = "";
            txtTmpLuzMin.Text = "";
            txtUmiMin.Text = "";
        }
        public void PreencheRegulagensTela()
        {
            txtPlantaCad.Text = dao.Planta;
            txtTempMaxReg.Text = dao.TempMax.ToString();
            txtTempMinReg.Text = dao.TempMin.ToString();
            txtTmpLuzMinReg.Text = dao.TmoMinLuz.ToString();
            txtUmiMinReg.Text = dao.UmiMin.ToString();
            txtPlantaInicio.Text = dao.Planta;
            if (dao != null) { lblFunc.Text = "ON"; lblFunc.ForeColor = Color.Green; }
            txtLuminosAtualEstuf.Text = dao.TmoMinLuz.ToString() + " h";
                
        }
        private void btnEnviarReg_Click_1(object sender, EventArgs e)
        {
            try
            {
                if (cboPortas.SelectedIndex > -1)
                {
                    if (txtPlanta.Text != "" & Int32.Parse(txtUmiMin.Text) > 0 & Int32.Parse(txtTempMax.Text) > 0 & Int32.Parse(txtTempMin.Text) > 0 & Int32.Parse(txtTmpLuzMin.Text) > 0)
                    {
                        string aux;
                        dao.GuardarRegulagens(txtPlanta.Text, Int32.Parse(txtUmiMin.Text), Int32.Parse(txtTempMax.Text), Int32.Parse(txtTempMin.Text), Int16.Parse(txtTmpLuzMin.Text));  //envia o texto presente nos txts de regulagem
                        PreencheRegulagensTela();
                        aux = Program.InterfaceDrvArduino.MontaMsgEnvioArduino(Int32.Parse(txtUmiMin.Text), Int32.Parse(txtTempMax.Text), Int32.Parse(txtTempMin.Text), Int16.Parse(txtTmpLuzMin.Text));
                        if (Program.SerialPort.IsOpen)
                        {
                            Program.SerialPort.Write(aux);
                            txtMsg.Text = ("Regulagem enviada com sucesso!");
                        }
                        else
                        {
                            txtMsg.Text = "Selecione a porta COM!";
                        }
                    }
                    else
                    {
                        MessageBox.Show("Todos os campos devem ser preenchidos");
                    }
                }
                else
                {
                    txtMsg.Text = ("Confira se o dispositivo está conectado ao computador.");
                    MessageBox.Show("Porta serial não encontrada");
                    EventTracer.Trace(EventTracer.EventType.Error, "Porta serial não encontrada");
                }

            }
            catch (Exception exc)
            {
                EventTracer.Trace(exc);
            }
        }
        private void btnCadastrar_Click(object sender, EventArgs e)
        {
            try
            {
                if (txtPlanta.Text != "" & Int32.Parse(txtUmiMin.Text) > 0 & Int32.Parse(txtTempMax.Text) > 0 & Int32.Parse(txtTempMin.Text) > 0 & Int32.Parse(txtTmpLuzMin.Text) > 0)
                {
                    dacReg.Insert(txtPlanta.Text, Int32.Parse(txtTempMax.Text), Int32.Parse(txtTempMin.Text), Int32.Parse(txtUmiMin.Text), Int32.Parse(txtTmpLuzMin.Text));
                    this.txtMsg.Text = "Sucesso na inserção de registro ao banco!";
                    ThreadLerCadastro();
                }
                else
                {
                    this.txtMsg.Text = "Erro na inserção de registro ao banco!";
                }
            }
            catch (Exception ex)
            {
                EventTracer.Trace(ex);
            }
        }
        private void btnDeletar_Click(object sender, EventArgs e)
        {
            RegEstufaDAC dac = new RegEstufaDAC();
            try
            {
                var selected = lsvRegulagens.SelectedItems[0];
                if (selected != null)
                {
                    dac.Delete(selected.SubItems[0].Text);
                    this.txtMsg.Text = "Dados excluídos!";
                    ThreadLerCadastro();
                }
            }
            catch (Exception exc)
            {
                EventTracer.Trace(exc);
            }
        }
        /// <summary>
        /// Ler os registros salvos no banco de dados
        /// </summary>
        private void ThreadLerCadastro()
        {
            try
            {
                RegEstufaDAC dac = new RegEstufaDAC();
                List<RegEstufaDAO> cadastro = new List<RegEstufaDAO>();
                ThreadPool.QueueUserWorkItem((_) =>
                {
                    try
                    {
                        cadastro = dac.SelectAll();
                        if (cadastro != null)
                        {
                            // Atualiza ordens selecionadas
                            MethodInvoker mth = (MethodInvoker)delegate ()
                            {
                                AtualizaListaCadastros(cadastro);
                            };
                            if (!this.IsHandleCreated)
                                mth.Invoke();
                            else
                                this.BeginInvoke(mth);
                        }
                        else
                        {
                            // Atualiza ordens selecionadas
                            MethodInvoker mth = (MethodInvoker)delegate ()
                            {
                                try
                                {
                                    this.lsvRegulagens.BeginUpdate();
                                    this.lsvRegulagens.Items.Clear();
                                    this.lsvRegulagens.EndUpdate();
                                }
                                catch (Exception e)
                                {
                                    EventTracer.Trace(e);
                                }
                            };
                            if (!this.IsHandleCreated)
                                mth.Invoke();
                            else
                                this.BeginInvoke(mth);
                        }
                    }
                    catch (Exception exc)
                    {
                        EventTracer.Trace(EventTracer.EventType.Error, String.Format("frmCadTuboPadrao - ThreadLerCadastro - {0}", exc.Message));
                    }
                });
            }
            catch (Exception exp)
            {
                EventTracer.Trace(exp);
            }
        }
        /// <summary>
        /// Ler os registros salvos no banco de dados
        /// </summary>
        private void ThreadRelatorio(Enum msgRel)
        {
            try
            {

                ResEstufaDAC dac = new ResEstufaDAC();
                List<ResEstufaDAO> cadastro = new List<ResEstufaDAO>();
                ThreadPool.QueueUserWorkItem((_) =>
                {
                    try
                    {
                        cadastro = dac.RelatorioUmidadeDiaria(dtpRelUmidade.Value);
                        // Atualiza ordens selecionadas
                        MethodInvoker mth = (MethodInvoker)delegate ()
                        {
                            AtualizaGraficoUmidade(cadastro, msgRel);
                        };
                        if (!this.IsHandleCreated)
                            mth.Invoke();
                        else
                            this.BeginInvoke(mth);
                    }
                    catch (Exception exc)
                    {
                        EventTracer.Trace(EventTracer.EventType.Error, String.Format("frmCadTuboPadrao - ThreadLerCadastro - {0}", exc.Message));
                    }
                });
            }
            catch (Exception exp)
            {
                EventTracer.Trace(exp);
            }
        }
        /// <summary>
        /// Ler os registros salvos no banco de dados 
        /// </summary>
        private void ThreadAtualizaComboBox()
        {
            try
            {
                ThreadPool.QueueUserWorkItem((_) =>
                {
                    try
                    {
                        // Atualiza combobox de regulagens.
                        MethodInvoker mth = (MethodInvoker)delegate ()
                        {
                            PreencheListaRegulagem();
                        };
                        if (!this.IsHandleCreated)
                            mth.Invoke();
                        else
                            this.BeginInvoke(mth);
                    }
                    catch (Exception exc)
                    {
                        EventTracer.Trace(EventTracer.EventType.Error, String.Format("frmCadTuboPadrao - ThreadLerCadastro - {0}", exc.Message));
                    }
                });
            }
            catch (Exception exp)
            {
                EventTracer.Trace(exp);
            }
        }
        /// <summary>
        /// Autaliza Lista de Cadastros
        /// </summary>
        /// <param name="cadastro">Lista de cadastros</param>
        private void AtualizaListaCadastros(List<RegEstufaDAO> cadastro)
        {
            try
            {
                this.lsvRegulagens.Items.Clear();
                this.lsvRegulagens.BeginUpdate();
                if (cadastro.Count > 0)
                {
                    for (int i = 0; i < cadastro.Count; i++)
                    {
                        ListViewItem lvi = new ListViewItem(cadastro[i].Planta.ToString());
                        lvi.SubItems.Add(cadastro[i].TempMax.ToString());
                        lvi.SubItems.Add(cadastro[i].TempMin.ToString());
                        lvi.SubItems.Add(cadastro[i].UmiMin.ToString()); ;
                        lvi.SubItems.Add(cadastro[i].TmoMinLuz.ToString());

                        this.lsvRegulagens.Items.Add(lvi);
                    }
                }
                this.lsvRegulagens.EndUpdate();
            }
            catch (Exception exp)
            {
                EventTracer.Trace(exp);
            }
        }
        /// <summary>
        /// Autaliza Lista de Cadastros
        /// </summary>
        /// <param name="cadastro">Lista de cadastros</param>
        private void AtualizaGraficoUmidade(List<ResEstufaDAO> cadastro, Enum enumTbpRel)
        {
            try
            {
                switch (enumTbpRel)
                {
                    case Relatorio.RelUmidade:
                        gfcUmidadeSolo.Series["UmidadexHora"].Points.Clear();
                        if (cadastro != null)
                        {
                            for (int i = 0; i < cadastro.Count; i++)
                            {
                                gfcUmidadeSolo.Series["UmidadexHora"].Points.AddXY(cadastro[i].DthCriacaoReg, cadastro[i].Umidade);
                            }
                            txtMsg.Text = "Gráfico de Umidade x Hora gerado com sucesso!";
                        }
                        else
                        {
                            gfcUmidadeSolo.Series["UmidadexHora"].Points.Clear();
                            txtMsg.Text = "Não existem dados para este dia!!";
                        }
                        break;
                    case Relatorio.RelTemperatura:
                        gfcTemperatura.Series["TemperaturaxHora"].Points.Clear();
                        if (cadastro != null)
                        {
                            for (int i = 0; i < cadastro.Count; i++)
                            {
                                gfcTemperatura.Series["TemperaturaxHora"].Points.AddXY(cadastro[i].DthCriacaoReg, cadastro[i].Temperatura);
                            }
                            txtMsg.Text = "Gráfico de Temperatura x Hora gerado com sucesso!";
                        }
                        else
                        {
                            gfcTemperatura.Series["TemperaturaxHora"].Points.Clear();
                            txtMsg.Text = "Não existem dados para este dia!!";
                        }
                        break;
                    case Relatorio.RelIluminacao:
                        gfcIluminacao.Series["IluminacaoxHora"].Points.Clear();
                        if (cadastro != null)
                        {
                            for (int i = 0; i < cadastro.Count; i++)
                            {
                                gfcIluminacao.Series["IluminacaoxHora"].Points.AddXY(cadastro[i].DthCriacaoReg, cadastro[i].IntensidadeLuz);
                            }
                            txtMsg.Text = "Gráfico de Iluminação x Hora gerado com sucesso!";
                        }
                        else
                        {
                            gfcIluminacao.Series["IluminacaoxHora"].Points.Clear();
                            txtMsg.Text = "Não existem dados para este dia!!";
                        }
                        break;
                }
               
            }
            catch (Exception exp)
            {
                EventTracer.Trace(exp);
            }
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            // TODO: esta linha de código carrega dados na tabela 'pROJETO_ESTUFA_DBDataSet1.res_estufa'. Você pode movê-la ou removê-la conforme necessário.
            this.res_estufaTableAdapter.Fill(this.pROJETO_ESTUFA_DBDataSet1.res_estufa);
            ThreadLerCadastro();
        }
        private void btnLoguin_Click(object sender, EventArgs e)
        {

        }
        private void PreencheListaRegulagem()
        {
            txtPlanta.Text = lsvRegulagens.SelectedItems[0].SubItems[0].Text.ToString();
            txtTempMax.Text = lsvRegulagens.SelectedItems[0].SubItems[1].Text.ToString();
            txtTempMin.Text = lsvRegulagens.SelectedItems[0].SubItems[2].Text.ToString();
            txtUmiMin.Text = lsvRegulagens.SelectedItems[0].SubItems[3].Text.ToString();
            txtTmpLuzMin.Text = lsvRegulagens.SelectedItems[0].SubItems[4].Text.ToString();
        }
        private void lsvRegulagens_SelectedIndexChanged(object sender, EventArgs e)
        {
            ThreadAtualizaComboBox();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            switch (tbpGGraph.SelectedTab.Name)
            {
                case "tbpGUmidade":
                    ThreadRelatorio(Relatorio.RelUmidade);
                    break;
                case "tbpGTemperatura":
                    ThreadRelatorio(Relatorio.RelTemperatura);
                    break;
                case "tbpIluminacao":
                    ThreadRelatorio(Relatorio.RelIluminacao);
                    break;
            }
            
        }

        private void lblMsgFuncEstuf_Click(object sender, EventArgs e)
        {

        }

        private void button2_Click(object sender, EventArgs e)
         {
            try
            {
                if (cboPortas.SelectedItem != null)
                {
                    if (!Program.SerialPort.IsOpen)
                    {
                        Program.SerialPort.Open();
                        Program.SerialPort.Write("B");
                    }
                    else
                        Program.SerialPort.Write("B");
                }
                else
                    txtMsg.Text = "Selecione a porta COM";
            }
            catch (Exception ex)
            {
                EventTracer.Trace(ex);
            }
        }

        private void btnAtivarIlu_Click(object sender, EventArgs e)
        {
            try
            {
                if (cboPortas.SelectedItem != null)
                {
                    if (!Program.SerialPort.IsOpen)
                    {
                        Program.SerialPort.Open();
                        Program.SerialPort.Write("I");
                    }
                    else
                        Program.SerialPort.Write("I");
                }
                else
                    txtMsg.Text = "Selecione a porta COM";
            }
            catch (Exception ex)
            {
                EventTracer.Trace(ex);
            }
        }

        private void btnAtivarVentilador_Click(object sender, EventArgs e)
        {
            try
            {
                if (cboPortas.SelectedItem != null)
                {
                    if (!Program.SerialPort.IsOpen)
                    {
                        Program.SerialPort.Open();
                        Program.SerialPort.Write("V");
                    }
                    else
                        Program.SerialPort.Write("V");
                }
                else
                    txtMsg.Text = "Selecione a porta COM";
            }
            catch (Exception ex)
            {
                EventTracer.Trace(ex);
            }
        }

        private void btnAtivarAquecimento_Click(object sender, EventArgs e)
        {
            try
            {
                if (cboPortas.SelectedItem != null)
                {
                    if (!Program.SerialPort.IsOpen)
                    {
                        Program.SerialPort.Open();
                        Program.SerialPort.Write("A");
                    }
                    else
                        Program.SerialPort.Write("A");
                }
                else
                    txtMsg.Text = "Selecione a porta COM";
            }
            catch (Exception ex)
            {
                EventTracer.Trace(ex);
            }
        }

        private void btn_Manual_Click(object sender, EventArgs e)
        {
            try
            {
                if (cboPortas.SelectedItem != null)
                {
                    if (!Program.SerialPort.IsOpen)
                    {
                        Program.SerialPort.Open();
                        Program.SerialPort.Write("Man");
                        btn_Manual.Enabled = false;
                        btn_Automatico.Enabled = true;
                    }
                    else
                        Program.SerialPort.Write("Man");
                        btn_Manual.Enabled = false;
                        btn_Automatico.Enabled = true;
                }
                else
                    txtMsg.Text = "Selecione a porta COM";
            }
            catch (Exception ex)
            {
                EventTracer.Trace(ex);
            }
        }

        private void btn_Automatico_Click(object sender, EventArgs e)
        {
            try
            {
                if (cboPortas.SelectedItem != null)
                {
                    if (!Program.SerialPort.IsOpen)
                    {
                        Program.SerialPort.Open();
                        Program.SerialPort.Write("Aut");
                        btn_Manual.Enabled = true;
                        btn_Automatico.Enabled = false;
                    }
                    else
                        Program.SerialPort.Write("Aut");
                    btn_Manual.Enabled = true;
                    btn_Automatico.Enabled = false;
                }
                else
                    txtMsg.Text = "Selecione a porta COM";
            }
            catch (Exception ex)
            {
                EventTracer.Trace(ex);
            }
        }
    }
}
