using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESTUFA_AUTOMATIZADA_REV1
{
    class EnviaMsgArduino
    {
        /// <summary>
        /// Variáveis privadas.
        /// </summary>
        private string m_Planta;
        private Int32 m_TmoMinLuz;
        private Int32 m_UmiMax;
        private Int32 m_UmiMin;
        private Int32 m_TempMax;
        private Int32 m_TempMin;

        /// <summary>
        /// Propriedade para o campo planta
        /// </summary>
        public string Planta
        {
            get { return m_Planta; }
            set { m_Planta = value; }
        }
        /// <summary>
        /// Propriedade para o campo tempo minimo de luz
        /// </summary>
        public Int32 TmoMinLuz
        {
            get { return m_TmoMinLuz; }
            set { m_TmoMinLuz = value; }
        }
        /// <summary>
        /// Propriedade para o campo comprimento
        /// </summary>
        public Int32 UmiMax
        {
            get { return m_UmiMax; }
            set { m_UmiMax = value; }
        }
        /// <summary>
        /// Propriedade para o campo dth_estampagem
        /// </summary>
        public Int32 UmiMin
        {
            get { return m_UmiMin; }
            set { m_UmiMin = value; }
        }
        /// <summary>
        /// Propriedade para o Temperatura máxima
        /// </summary>
        public Int32 TempMax
        {
            get { return m_TempMax; }
            set { m_TempMax = value; }
        }
        /// <summary>
        /// Propriedade para o Temperatura máxima
        /// </summary>
        public Int32 TempMin
        {
            get { return m_TempMin; }
            set { m_TempMin = value; }
        }


        public void GuardaRegulagemAtualEnvioArduino(string Planta, Int32 UmiMax, Int32 UmiMin, Int32 TempMax, Int32 TempMin, Int32 TmoMinLuz)
        {
            m_Planta = Planta;
            m_TmoMinLuz = TmoMinLuz;
            m_UmiMax = UmiMax;
            m_UmiMin = UmiMin;
            m_TempMax = TempMax;
            m_TempMin = TempMin;
        }
    }
}

