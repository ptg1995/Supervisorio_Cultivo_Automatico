using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO.Ports;
using System.Threading;

namespace ESTUFA_AUTOMATIZADA_REV1
{
    public class MsgDadosArduino
    {
        
        /// <summary>
        /// Variáveis privadas.
        /// </summary>
        /// 
        private Int32 m_IntensidadeLuz;
        private Int32 m_Umidade;
        private Double m_Temperatura;
        private string m_Msg;

        /// <summary>
        /// Propriedade para o campo tempo minimo de luz
        /// </summary>
        public Int32 IntensidadeLuz
        {
            get { return m_IntensidadeLuz; }
            set { m_IntensidadeLuz = value; }
        }
        /// <summary>
        /// Propriedade para o campo dth_estampagem
        /// </summary>
        public Int32 Umidade
        {
            get { return m_Umidade; }
            set { m_Umidade = value; }
        }
        /// <summary>
        /// Propriedade para o Temperatura máxima
        /// </summary>
        public Double Temperatura
        {
            get { return m_Temperatura; }
            set { m_Temperatura = value; }
        }
        /// <summary>
        /// Propriedade para o Temperatura máxima
        /// </summary>
        public string Msg
        {
            get { return m_Msg; }
            set { m_Msg = value; }
        }
    }
}
