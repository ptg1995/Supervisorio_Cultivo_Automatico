using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESTUFA_AUTOMATIZADA_REV1
{
    class RegEstufaDAO
    {
        private string m_Planta;
        private Int32 m_TmoMinLuz;
        private Int32 m_UmiMin;
        private Int32 m_TempMax;
        private Int32 m_TempMin;
        public void GuardarRegulagens(string Planta, Int32 UmiMin, Int32 TempMax, Int32 TempMin, Int32 TmoMinLuz)
        {
            m_Planta = Planta;
            m_TmoMinLuz = TmoMinLuz;
            m_UmiMin = UmiMin;
            m_TempMax = TempMax;
            m_TempMin = TempMin;
        }
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
        /// Propriedade para o campo Umidade Minima
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
        /// Propriedade para o Temperatura mínima
        /// </summary>
        public Int32 TempMin
        {
            get { return m_TempMin; }
            set { m_TempMin = value; }
        }
    }
}
