using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Diagnostics;

namespace ESTUFA_AUTOMATIZADA_REV1
{
    class EventTracer
    {
        // Membros
        private static Mutex m_TraceMutex = new Mutex();
        private static Mutex m_LoadFileInfoMutex = new Mutex();
        private static Mutex m_EmergencyTraceMutex = new Mutex();
        private static EventType m_LogLevel = EventType.Full;
        private static EventType m_EventLevel = EventType.Info;
        private static string m_LogFileName = "TraceLog";
        private static int m_LogDays = 5;
        private static System.Timers.Timer m_DeleteOldFilesTimer = null;
        private static bool m_FirstLogDone = false;
        private static bool m_CurrInfoLoaded = false;
        private static string m_CurrLogFullFileName = "";
        private static DateTime m_CurrLogDate = DateTime.MinValue;
        private static int m_CurrLogFileNumber = 1;
        private static string m_CurrLogStatus = "";
        private static DateTime m_LastLengthVerification = DateTime.MinValue;

        // Enums
        /// <summary>
        /// NÃ­vel de eventos de log
        /// </summary>
        public enum EventType
        {
            /// <summary>
            ///  NÃ­vel de log para exceções do sistema
            /// </summary>
            Exception = 0x0001,
            /// <summary>
            ///  NÃ­vel de log para erros do sistema
            /// </summary>
            Error = 0x0002,
            /// <summary>
            ///  NÃ­vel de log de atenssão do sistema
            /// </summary>
            Warning = 0x0004,
            /// <summary>
            /// NÃ­vel de log de informações gerais do sistema
            /// </summary>
            Info = 0x0008,
            /// <summary>
            /// NÃ­vel de log para dados importantes do sistema
            /// </summary>
            Data = 0x0010,
            /// <summary>
            /// NÃ­vel de log para informações mais completas do sistema
            /// </summary>
            Full = 0x0020
        }

        // Constantes
        private static string FILE_EXT = "txt";

        // Delegates
        public delegate void DlgOnTraceMsg(DateTime date, EventType type, string message);

        // Eventos
        /// <summary>
        /// Gerado, condicionado ao EventLevel atual, sempre que acontece um trace
        /// </summary>
        public static event DlgOnTraceMsg OnTraceMsg;

        // Construtor static
        static EventTracer()
        {
            // ----- Habilita timer para delessão de arquivos de log antigos
            m_DeleteOldFilesTimer = new System.Timers.Timer();
            m_DeleteOldFilesTimer.Interval = 3600000;        // uma hora
            m_DeleteOldFilesTimer.Elapsed += new System.Timers.ElapsedEventHandler(m_DeleteOldFilesTimer_Elapsed);
            m_DeleteOldFilesTimer.AutoReset = true;
            m_DeleteOldFilesTimer.Enabled = true;
        }

        /// <summary>
        /// NÃ­vel de log para arquivo
        /// </summary>
        public static EventType LogLevel
        {
            get { return m_LogLevel; }
            set { m_LogLevel = value; }
        }

        /// <summary>
        /// NÃ­vel de log para gerassão do evento de trace
        /// Combinassão dos nÃ­veis que devem gerar evento
        /// </summary>
        public static EventType EventLevel
        {
            get { return m_EventLevel; }
            set { m_EventLevel = value; }
        }

        /// <summary>
        /// Nome do arquivo de log
        /// </summary>
        public static string LogFileName
        {
            get
            {
                return m_LogFileName;
            }
            set
            {
                if (m_LogFileName != value)
                {
                    m_CurrInfoLoaded = false;
                    m_LogFileName = value;
                }
            }
        }

        /// <summary>
        /// Quantidade de dias para manutenssão do log
        /// </summary>
        public static int LogDays
        {
            get { return m_LogDays; }
            set { m_LogDays = value; }
        }

        /// <summary>
        /// Efetua o log de exceções do sistema
        /// </summary>
        /// <param name="exception">Excessão para log</param>
        public static void Trace(Exception exception)
        {
            // Delega para funssão mais completa
            Trace(exception, "");
        }
        /// <summary>
        /// Efetua o log de exceções do sistema, com uma mensagem adicional
        /// </summary>
        /// <param name="exception">Excessão para log</param>
        /// <param name="additionalMessage">Mensagem adicional para excessão</param>
        public static void Trace(Exception exception, string additionalMessage)
        {
            try
            {
                // Loga erro relacionado Ã  excessão
                if ((additionalMessage != null) && (additionalMessage.Length > 0))
                    Trace(EventType.Error, String.Format("{0} - Excessão não esperada", additionalMessage));

                // Loga dados detalhados da excessão
                StringBuilder msgDetails = new StringBuilder();
                if ((exception != null) && (exception.Message != null))
                {
                    msgDetails.AppendFormat("{0}", exception.Message);
                    if ((exception.StackTrace != null) && (exception.StackTrace.Length > 0))
                        msgDetails.AppendFormat("- {0}", exception.StackTrace.Replace("\r\n", " "));
                }
                else
                    msgDetails.Append("Exception == null || Exception.Message == null");
                Trace(EventType.Exception, msgDetails.ToString());
            }
            catch (Exception exc)
            {
                EmergencyTrace("TraceException", String.Format("Excessão na chamada da funssão Trace: {0}", exc.Message));
            }
        }

        /// <summary>
        /// Efetua o log de eventos do sistema
        /// </summary>
        /// <param name="type">Tipo do evento</param>
        /// <param name="message">Mensagem de log</param>
        /// <param name="fileName">Nome do arquivo de log</param>
        public static void Trace(EventType type, string message)
        {
            try
            {
                // Acesso exclusivo
                m_TraceMutex.WaitOne();

                // Carrega informações no nome do arquivos gravados
                if (!m_CurrInfoLoaded) LoadFileNameInfo();

                // Salva variáveis para trace
                DateTime traceDate = DateTime.Now;

                // Verifica se possui o nÃ­vel definido para gerassão do evento
                if ((type & m_EventLevel) == type)
                    if (OnTraceMsg != null)
                        try
                        {
                            OnTraceMsg(traceDate, type, message);
                        }
                        catch (Exception e)
                        {
                            EmergencyTrace("Falha ao disparar evento de log 'OnTraceMsg'", type.ToString() + message + " (" + e.Message + ")");
                        }

                // Verifica se à maior que o nÃ­vel de log atual, se for sai fora
                if (type > m_LogLevel) return;

                // Se o arquivo foi apagado, limpa status
                if (m_CurrLogFullFileName != "" && !File.Exists(m_CurrLogFullFileName))
                    m_CurrLogStatus = "";

                // Verifica se trocou de dia
                if (DateTime.Now.Date > m_CurrLogDate.Date)
                {
                    m_CurrLogFileNumber = 1;
                    m_CurrLogFullFileName = "";
                    m_CurrLogStatus = "";
                    m_CurrLogDate = DateTime.Now;
                }

                // Novo status
                string newStatus = GetNewStatus(type);

                // Nome do arquivo a ser gravado
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("{0}_{1:yyyyMMdd} # {2:D3} [{3}].{4}", m_LogFileName, m_CurrLogDate, m_CurrLogFileNumber, newStatus, FILE_EXT);
                string fileName = sb.ToString();

                // Verifica se trocou de status
                if (m_CurrLogStatus.PadRight(newStatus.Length, '_') != newStatus)
                    if (m_CurrLogFullFileName != null)
                        if (File.Exists(m_CurrLogFullFileName))
                            try
                            {
                                // Renomeia arquivo
                                FileInfo fi = new FileInfo(m_CurrLogFullFileName);
                                fi.MoveTo(fileName);
                            }
                            catch (Exception e)
                            {
                                EmergencyTrace("Falha ao renomear arq. de log", string.Format("{0} -> {1} ({2})", m_CurrLogFullFileName, fileName, e.Message));
                            }

                // Status corrente
                m_CurrLogStatus = newStatus;

                // Nome completo do arq. corrente
                m_CurrLogFullFileName = fileName;

                // Escreve no arq.
                if (!m_FirstLogDone)
                {
                    m_FirstLogDone = true;
                    WriteLogFile(m_CurrLogFullFileName, FormataMensagemLog(traceDate, EventType.Info, "Primeiro log do sistema após a partida do processo"));
                }
                WriteLogFile(m_CurrLogFullFileName, FormataMensagemLog(traceDate, type, message));

                // Verifica tamanho do arq.
                if (DateTime.Now.Subtract(m_LastLengthVerification).TotalSeconds > 60)
                {
                    try
                    {
                        m_LastLengthVerification = DateTime.Now;
                        FileInfo fi = new FileInfo(m_CurrLogFullFileName);
                        if (fi.Length > (10 * 1024 * 1024))
                        {
                            m_CurrLogFileNumber++;
                            m_CurrLogFullFileName = "";
                            m_CurrLogStatus = "";
                        }
                    }
                    catch (Exception e)
                    {
                        EmergencyTrace("Falha na verificassão do tamanho do arq. de log", m_CurrLogFullFileName + " (" + e.Message + ")");
                    }
                }
            }
            catch (Exception exc)
            {
                EmergencyTrace("TraceEventType", String.Format("Excessão na chamada da funssão Trace: {0}", exc.Message));
            }
            finally
            {
                m_TraceMutex.ReleaseMutex();
            }
        }

        /// <summary>
        /// Formata mensagem de log
        /// </summary>
        private static string FormataMensagemLog(DateTime traceDate, EventType type, string message)
        {
            return string.Format("{0:HH:mm:ss,fff} - {1} - {2}", traceDate, type.ToString().PadRight(9), message);
        }

        /// <summary>
        /// Evento do timer de delessão dos arquivos antigos de trace
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void m_DeleteOldFilesTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                foreach (string fullFileName in Directory.GetFiles(Path.GetDirectoryName(m_LogFileName), string.Format("*.{0}", FILE_EXT)))
                {
                    string fileName = Path.GetFileName(fullFileName);
                    if (Path.GetFileName(fullFileName).StartsWith(Path.GetFileName(m_LogFileName)))
                    {
                        int dateDel = fileName.IndexOf('_');
                        int numDel = fileName.IndexOf('#');
                        int statusStartDel = fileName.IndexOf('[');
                        int statusEndDel = fileName.IndexOf(']');

                        if (statusEndDel > statusStartDel &&
                            statusStartDel > numDel &&
                            numDel > dateDel &&
                            numDel - dateDel == 10)
                        {
                            string fileDate = fileName.Substring(dateDel + 1, 8);
                            string limitDate = DateTime.Now.AddDays(-m_LogDays).ToString("yyyyMMdd");
                            if (int.Parse(fileDate) < int.Parse(limitDate))
                                File.Delete(fullFileName);
                        }
                    }
                }
            }
            catch (Exception exp)
            {
                EmergencyTrace("Falha na limpeza de arquivos de log", exp.Message);
            }
        }

        /// <summary>
        /// Primeiro log do sistema
        /// </summary>
        private static void FirstLog()
        {
            if (!m_FirstLogDone)
            {
                m_FirstLogDone = true;
                Trace(EventType.Info, "Primeiro log do sistema após a partida do processo");
            }
        }

        /// <summary>
        /// Escreve no arquivo de log
        /// </summary>
        private static void WriteLogFile(string fileName, string line)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(fileName, true))
                    sw.WriteLine(line);
            }
            catch (Exception e)
            {
                EmergencyTrace("Falha na escrita do arq. de log", "arq.: " + fileName + " line: " + line + " (" + e.Message + ")");
            }
        }

        /// <summary>
        /// Carrega informações no nome dos arquivos gravados
        /// </summary>
        private static void LoadFileNameInfo()
        {
            try
            {
                m_LoadFileInfoMutex.WaitOne();
                // Verifica se já rodou
                if (m_CurrInfoLoaded) return;
                // Pega dados no nome completo do arquivo de log
                m_CurrLogDate = DateTime.Now.Date;
                string dirName = Path.GetDirectoryName(m_LogFileName);
                string fName = Path.GetFileName(m_LogFileName);
                int fNum = 0;
                // Cria diretório se necessário
                if (dirName != "")
                {
                    if (!Directory.Exists(dirName))
                        Directory.CreateDirectory(dirName);
                }
                else
                {
                    dirName = Environment.CurrentDirectory;
                    m_LogFileName = Path.Combine(dirName, m_LogFileName);
                }
                // Varre todos os arquivos no diretório
                foreach (string fullFileName in Directory.GetFiles(dirName, string.Format("*.{0}", FILE_EXT)))
                {
                    // Verifica se o arquivo tem a extensãoo de arquivo de log
                    string fileName = Path.GetFileName(fullFileName);
                    // Verifica se o arquivo inicia com o padrãoo de um arq. de log
                    if (Path.GetFileName(fullFileName).StartsWith(Path.GetFileName(m_LogFileName)))
                    {
                        int dateDel = fileName.IndexOf('_');
                        int numDel = fileName.IndexOf('#');
                        int statusStartDel = fileName.IndexOf('[');
                        int statusEndDel = fileName.IndexOf(']');

                        // Verifica se o nome do arq. possui todas as informações de um verdadeiro arq. de log
                        if (statusEndDel > statusStartDel &&
                            statusStartDel > numDel &&
                            numDel > dateDel &&
                            numDel - dateDel == 10)
                        {
                            // Verifica se o arq. de log à de hoje (ou seja, à o corrente)
                            string date = fileName.Substring(dateDel + 1, 8);
                            if (date == m_CurrLogDate.ToString("yyyyMMdd"))
                            {
                                // Pega o arq. com o maior nÃºmero
                                int num = int.Parse(fileName.Substring(numDel + 1, statusStartDel - numDel - 1).Trim());
                                if (num > fNum)
                                {
                                    m_CurrLogFileNumber = num;
                                    m_CurrLogStatus = fileName.Substring(statusStartDel + 1, statusEndDel - statusStartDel - 1);
                                    m_CurrLogFullFileName = fullFileName;
                                    fNum = num;
                                }
                            }
                        }
                    }
                }
                m_CurrInfoLoaded = true;
            }
            catch (Exception e)
            {
                EmergencyTrace("Falha ao carregar informações dos arquivos de log", e.Message);
            }
            finally
            {
                m_LoadFileInfoMutex.ReleaseMutex();
            }
        }

        /// <summary>
        /// Monta status
        /// </summary>
        private static string GetNewStatus(EventType type)
        {
            string result = "";

            // Exception
            if ((m_CurrLogStatus.Length > 0 && m_CurrLogStatus[0] == 'X') || (int)type == 1)
                result += "X";
            else
                result += "_";

            // Error
            if ((m_CurrLogStatus.Length > 1 && m_CurrLogStatus[1] == 'E') || (int)type == 2)
                result += "E";
            else
                result += "_";

            // Warning
            if ((m_CurrLogStatus.Length > 2 && m_CurrLogStatus[2] == 'W') || (int)type == 4)
                result += "W";
            else
                result += "_";

            // Info
            if ((m_CurrLogStatus.Length > 3 && m_CurrLogStatus[3] == 'I') || (int)type == 8)
                result += "I";
            else
                result += "_";

            // Data
            if ((m_CurrLogStatus.Length > 4 && m_CurrLogStatus[4] == 'D') || (int)type == 16)
                result += "D";
            else
                result += "_";

            // Full
            if ((m_CurrLogStatus.Length > 5 && m_CurrLogStatus[5] == 'F') || (int)type == 32)
                result += "F";
            else
                result += "_";

            return result;
        }

        /// <summary>
        /// Log de emergencia caso de algum problema na utilizassão do log convencional
        /// </summary>
        private static void EmergencyTrace(string errorMsg, string logMsg)
        {
            string execName = Assembly.GetEntryAssembly().GetName().Name;
            try
            {
                m_EmergencyTraceMutex.WaitOne();
                StringBuilder line = new StringBuilder();
                line.AppendFormat("{0:dd/MM/yyyy HH:mm:ss,fff} - {1}: {2}", DateTime.Now, errorMsg, logMsg);
                using (StreamWriter sw = new StreamWriter(execName + "_logerror.txt", true)) sw.WriteLine(line.ToString());
            }
            catch (Exception exc)
            {
                System.Diagnostics.EventLog.WriteEntry(execName, string.Format("Falha na gravassão da entrada [{0}] no arquivo de log emergencial. Erro: {1}", logMsg, exc.Message), EventLogEntryType.Error);
            }
            finally
            {
                m_EmergencyTraceMutex.ReleaseMutex();
            }
        }
    }
}
