using System;
using System.Collections.Generic;
using System.Linq;
using Npgsql;
using System.Text;
using System.Data;
using System.Threading.Tasks;

namespace ESTUFA_AUTOMATIZADA_REV1
{
    public class ResEstufaDAC
    {
        private string connString = "Server=localhost; Port=5432; User Id=postgres; Password=postgres; Database=PROJETO_ESTUFA_DB";
        public NpgsqlConnection conn;
        public NpgsqlCommand cmd;
        public string sq;

        /// Preenche classe de registros da tabela ResEstufaDAC baseado no DataReader
        /// </summary>
        /// <param name="reader">DataReader posicionado no registro que se deseja ler</param>
        /// <returns>Classe de registro preenchida com dados do DataReader</returns>
        private static ResEstufaDAO GetDAO(IDataReader reader)
        {
            // Cria instância com valores padrões
            ResEstufaDAO dao = new ResEstufaDAO();
            // Preenche dados considerando ordem dos campos no select
            if (!reader.IsDBNull(0)) dao.Temperatura = reader.GetInt16(0);
            if (!reader.IsDBNull(1)) dao.Umidade = reader.GetInt16(1);
            if (!reader.IsDBNull(2)) dao.IntensidadeLuz = reader.GetInt16(2);
            if (!reader.IsDBNull(3)) dao.DthCriacaoReg = reader.GetDateTime(3);
            // Retorna instância criada
            return dao;
        }
        public List<ResEstufaDAO> SelectAll()
        {
            try
            {
                this.conn = new NpgsqlConnection(this.connString);
                this.conn.Open();
                // Monta sql
                StringBuilder sql = new StringBuilder("select");
                sql.Append(" \"TEMPERATURA\",");
                sql.Append(" \"UMIDADE\",");
                sql.Append(" \"INTENSIDADE_LUZ\",");
                sql.Append(" \"DTH_CRIACAO_REG\"");
                sql.Append(" from public.res_estufa");
                sq = sql.ToString();
                // Cria comando
                cmd = new NpgsqlCommand(sq, conn);
                using (IDataReader reader = cmd.ExecuteReader())
                {
                    List<ResEstufaDAO> list = new List<ResEstufaDAO>();
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
        public void Insert(Double temperatura, Int32 umidade, Int32 intensidade_luz)
        {
            try
            {
                this.conn = new NpgsqlConnection(this.connString);
                // Abre a conexão
                this.conn.Open();
                // Monta sql
                StringBuilder sql = new StringBuilder("insert into public.res_estufa(");
                sql.Append(" \"temperatura\",");
                sql.Append(" \"umidade\",");
                sql.Append(" \"intensidade_luz\",");
                sql.Append(" \"dth_criacao_reg\"");
                sql.Append(") values (");
                sql.Append(" :temperatura,");
                sql.Append(" :umidade,");
                sql.Append(" :intensidade_luz,");
                sql.Append(" :dth_criacao_reg");
                sql.Append(")");
                string sq = sql.ToString();
                // Cria comando
                cmd = new NpgsqlCommand(sq, conn);
                // Adiciona parâmetros (na ordem em que aparecem na query)
                cmd.Parameters.Add("@temperatura", NpgsqlTypes.NpgsqlDbType.Double, 12).Value = temperatura;
                cmd.Parameters.Add("@umidade", NpgsqlTypes.NpgsqlDbType.Integer, 3).Value = umidade;
                cmd.Parameters.Add("@intensidade_luz", NpgsqlTypes.NpgsqlDbType.Integer, 3).Value = intensidade_luz;
                cmd.Parameters.Add("@dth_criacao_reg", NpgsqlTypes.NpgsqlDbType.Timestamp, 32).Value = DateTime.Now;
                // Executa o comando e verifica retorno
                if (cmd.ExecuteNonQuery() == 0)
                    throw new Exception("Erro na inserção de registro na tabela reg_estufa.");

                this.conn.Close();
                EventTracer.Trace(EventTracer.EventType.Info, "Dados de regulagem inseridos ao banco !");
            }
            catch (Exception ex)
            {
                EventTracer.Trace(ex, "Sem conexão com o banco ou valores nulos");
            }

        }
        public void Delete(Int32 temperatura, Int32 umidade, Int32 intensidade_luz, DateTime dth_criacao_reg)
        {
            try
            {
                this.conn = new NpgsqlConnection(this.connString);
                // Abre a conexão
                this.conn.Open();
                // Monta sql
                StringBuilder sqlDel = new StringBuilder("delete from public.res_estufa");
                sqlDel.Append(" WHERE");
                sqlDel.Append(" \"TEMPERATURA\",");
                sqlDel.Append(" \"UMIDADE\",");
                sqlDel.Append(" \"INTENSIDADE_LUZ\",");
                sqlDel.Append(" \"DTH_CRIACAO_REG\"");
                string sq = sqlDel.ToString();
                // Cria comando
                cmd = new NpgsqlCommand(sq, conn);
                // Adiciona parâmetros (na ordem em que aparecem na query)
                cmd.Parameters.Add("@temperatura", NpgsqlTypes.NpgsqlDbType.Varchar, 12).Value = temperatura;
                cmd.Parameters.Add("@umidade", NpgsqlTypes.NpgsqlDbType.Integer, 3).Value = umidade;
                cmd.Parameters.Add("@intensidade_luz", NpgsqlTypes.NpgsqlDbType.Integer, 3).Value = intensidade_luz;
                cmd.Parameters.Add("@dth_criacao_reg", NpgsqlTypes.NpgsqlDbType.Timestamp, 3).Value = dth_criacao_reg;

                // Executa o comando e verifica retorno
                if (cmd.ExecuteNonQuery() == 0)
                    throw new Exception("Erro na exclusão de registro na tabela reg_estufa.");
                this.conn.Close();
                EventTracer.Trace(EventTracer.EventType.Info, "Planta foi deletada do banco de dados!");
            }
            catch (Exception ex)
            {
                EventTracer.Trace(ex, "Sem conexão com o banco ou erro na querry");
            }
        }
        /// <summary>
        /// Retorna dados para relatório de umidade x hora
        /// </summary>
        /// <returns></returns>
        public List<ResEstufaDAO> RelatorioUmidadeDiaria(DateTime dthini)
        {
            try
            {
                this.conn = new NpgsqlConnection(this.connString);
                this.conn.Open();
                // Monta sql
                StringBuilder sql = new StringBuilder("select");
                sql.Append(" \"temperatura\",");
                sql.Append(" \"umidade\",");
                sql.Append(" \"intensidade_luz\",");
                sql.Append(" \"dth_criacao_reg\"");
                sql.Append(" from public.res_estufa");
                sql.Append(" where  \"dth_criacao_reg\" between :dthini and :dthfim ");
                sq = sql.ToString();
                // Cria comando
                cmd = new NpgsqlCommand(sq, conn);
                cmd.Parameters.Add("@dthini", NpgsqlTypes.NpgsqlDbType.Timestamp, 32).Value = dthini.Date;
                cmd.Parameters.Add("@dthfim", NpgsqlTypes.NpgsqlDbType.Timestamp, 32).Value = dthini.AddDays(1);
                using (IDataReader reader = cmd.ExecuteReader())
                {
                    List<ResEstufaDAO> list = new List<ResEstufaDAO>();
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
    }
}

