using System;
using System.Collections.Generic;
using System.Linq;
using Npgsql;
using System.Text;
using System.Data;
using System.Threading.Tasks;

namespace ESTUFA_AUTOMATIZADA_REV1
{
    class RegEstufaDAC
    {
        private string connString = "Server=localhost; Port=5432; User Id=postgres; Password=postgres; Database=PROJETO_ESTUFA_DB";
        public NpgsqlConnection conn;
        public StringBuilder sql;
        public NpgsqlCommand cmd;
        public string sq;

        /// Preenche classe de registros da tabela CAD_TUBO_PADRAO baseado no DataReader
        /// </summary>
        /// <param name="reader">DataReader posicionado no registro que se deseja ler</param>
        /// <returns>Classe de registro preenchida com dados do DataReader</returns>
        private static RegEstufaDAO GetDAO(IDataReader reader)
        {
            // Cria instância com valores padrões
            RegEstufaDAO dao = new RegEstufaDAO();
            // Preenche dados considerando ordem dos campos no select
            if (!reader.IsDBNull(0)) dao.Planta = reader.GetString(0);
            if (!reader.IsDBNull(1)) dao.TempMax = reader.GetInt16(1);
            if (!reader.IsDBNull(2)) dao.TempMin = reader.GetInt16(2);
            if (!reader.IsDBNull(3)) dao.UmiMin = reader.GetInt16(3);
            if (!reader.IsDBNull(4)) dao.TmoMinLuz = reader.GetInt16(4);
            // Retorna instância criada
            return dao;
        }
        public List<RegEstufaDAO> SelectAll()
        {
            try
            {
                this.conn = new NpgsqlConnection(this.connString);
                this.conn.Open();
                // Monta sql
                StringBuilder sql = new StringBuilder("select");
                sql.Append(" \"PLANTA\",");
                sql.Append(" \"TMP_MAX\",");
                sql.Append(" \"TMP_MIN\",");
                sql.Append(" \"UMI_MIN\",");
                sql.Append(" \"TMO_MIN_LUZ\"");
                sql.Append(" from public.reg_estufa");
                sq = sql.ToString();
                // Cria comando
                cmd = new NpgsqlCommand(sq, conn);
                using (IDataReader reader = cmd.ExecuteReader())
                {
                    List<RegEstufaDAO> list = new List<RegEstufaDAO>();
                    while (reader.Read())
                        list.Add(GetDAO(reader));
                    if (list.Count > 0)
                        return list;
                    else
                        return null;
                }
            }
            catch (Exception exc)
            {
                EventTracer.Trace(exc);
                return null;
            }
        }
        /// <summary>
        /// Insere registro na tabela res_barra
        /// </summary>
        /// <param name="reg">Registro a ser inserido</param>
        /// <param name="timeout">Timeout, em segundos, para execução do comando</param>
        public void Insert(string planta, Int32 tmpMax, Int32 tmpMin, Int32 umiMin, Int32 tmoMinLuz)
        {
            try
            {
                this.conn = new NpgsqlConnection(this.connString);

                // Abre a conexão
                this.conn.Open();
                // Monta sql
                StringBuilder sql = new StringBuilder("insert into public.reg_estufa(");
                sql.Append(" \"PLANTA\",");
                sql.Append(" \"TMP_MAX\",");
                sql.Append(" \"TMP_MIN\",");
                sql.Append(" \"UMI_MIN\",");
                sql.Append(" \"TMO_MIN_LUZ\"");
                sql.Append(") values (");
                sql.Append(" :planta,");
                sql.Append(" :tmp_max,");
                sql.Append(" :tmp_min,");
                sql.Append(" :umi_min,");
                sql.Append(" :tmo_min_luz");
                sql.Append(")");
                string sq = sql.ToString();
                // Cria comando
                cmd = new NpgsqlCommand(sq, conn);
                // Adiciona parâmetros (na ordem em que aparecem na query)
                cmd.Parameters.Add("@planta", NpgsqlTypes.NpgsqlDbType.Varchar, 12).Value = planta;
                cmd.Parameters.Add("@TMP_MAX", NpgsqlTypes.NpgsqlDbType.Integer, 3).Value = tmpMax;
                cmd.Parameters.Add("@TMP_MIN", NpgsqlTypes.NpgsqlDbType.Integer, 3).Value = tmpMin;
                cmd.Parameters.Add("@UMI_MIN", NpgsqlTypes.NpgsqlDbType.Integer, 3).Value = umiMin;
                cmd.Parameters.Add("@tmo_min_luz", NpgsqlTypes.NpgsqlDbType.Integer, 3).Value = tmoMinLuz;


                // Executa o comando e verifica retorno
                if (cmd.ExecuteNonQuery() == 0)
                    throw new Exception("Erro na inserção de registro na tabela res_barra.");
            }
            catch (Exception ex)
            {
                EventTracer.Trace(ex, "Sem conexão com o banco ou valores nulos");
            }

        }
        public void Delete(string planta)
        {
            try
            {
                this.conn = new NpgsqlConnection(this.connString);
                // Abre a conexão
                this.conn.Open();
                // Monta sql
                StringBuilder sqlDel = new StringBuilder("delete from public.reg_estufa");
                sqlDel.Append(" WHERE");
                sqlDel.Append(" \"PLANTA\" = :planta");
                string sq = sqlDel.ToString();
                // Cria comando
                cmd = new NpgsqlCommand(sq, conn);
                // Adiciona parâmetros (na ordem em que aparecem na query)
                cmd.Parameters.Add("@planta", NpgsqlTypes.NpgsqlDbType.Varchar, 12).Value = planta;
                // Executa o comando e verifica retorno
                if (cmd.ExecuteNonQuery() == 0)
                    throw new Exception("Erro na exclusão de registro na tabela reg_estufa.");
            }
            catch (Exception ex)
            {
                EventTracer.Trace(ex);
            }
        }
    }
}

