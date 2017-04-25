using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Windows;

namespace RegimenCondominio.C
{
    public class SQL_Connector : IDisposable
    {

        /// <summary>
        /// The connection object
        /// </summary>
        public SqlConnection Connection;
        /// <summary>
        /// The connection status
        /// </summary>
        public ConnectionState ConnectionStatus;
        /// <summary>
        /// The last excecuted error
        /// </summary>
        public String Error;

        /// <summary>
        /// Initializes a new instance of the <see cref="SQL_Connector"/> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        public SQL_Connector(String connectionString)
        {
            try
            {
                this.Connection = new SqlConnection(connectionString);
                this.Connection.Open();
                this.ConnectionStatus = this.Connection.State;
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.ToString());
                //Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage("\n{0}", exc.Message);
            }
        }
        /// <summary>
        /// Selects the specified query.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="needle">El caracter que separa las columnas de las filas</param>
        /// <param name="result">Como parámetro de salida el resultado del query</param>
        public Boolean Select(String query, out List<String> result, char needle)
        {
            result = new List<string>();
            try
            {
                SqlDataAdapter sqlAdapter = new SqlDataAdapter(query, this.Connection);
                DataSet ds = new DataSet();
                sqlAdapter.Fill(ds);
                String row;
                for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                {
                    row = String.Empty;
                    for (int j = 0; j < ds.Tables[0].Rows[i].ItemArray.Length; j++)
                    {
                        row += ds.Tables[0].Rows[i].ItemArray[j].ToString();
                        if (j != ds.Tables[0].Rows[i].ItemArray.Length - 1)
                            row += needle;
                    }
                    result.Add(row);
                }
                return true;
            }
            catch (Exception exc)
            {
                Error = exc.Message;
            }
            return false;
        }


        /// <summary>
        /// Selects the specified query.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <param name="needle">El caracter que separa las columnas de las filas</param>
        /// <param name="result">Como parámetro de salida el resultado del query</param>
        public DataSet SelectTables(string query)
        {
            //Inicializo
            DataSet dtSet = new DataSet();
            try
            {
                SqlDataAdapter sqlAdapter = new SqlDataAdapter(query, this.Connection);
                sqlAdapter.Fill(dtSet);
            }
            catch (Exception exc)
            {
                Error = exc.Message;
            }
            return dtSet;
        }

        public Boolean SelectOne(String query, out string result)
        {
            result = string.Empty;
            try
            {
                SqlDataAdapter sqlAdapter = new SqlDataAdapter(query, this.Connection);
                DataSet ds = new DataSet();
                sqlAdapter.Fill(ds);
                result = ds.Tables[0].Rows[0].ItemArray[0].ToString();
                return true;
            }
            catch (Exception exc)
            {
                Error = exc.Message;
            }
            return false;
        }



        /// <summary>
        /// Runs the specified query.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>Verdadero si funciona el comando</returns>
        public bool Run(string query)
        {
            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = this.Connection;
                cmd.CommandText = query;
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception exc)
            {
                Error = exc.Message;
                Error.ToEditor();
            }
            return false;
        }

        /// <summary>
        /// Runs the specified query.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>Verdadero si funciona el comando</returns>
        public bool Run(string query, List<M.Bloques> bloques)
        {
            try
            {
                SqlCommand cmd = new SqlCommand();
                cmd.Connection = this.Connection;
                cmd.CommandText = query;
                foreach (M.Bloques bloque in bloques)
                {
                    cmd.Parameters.AddWithValue("@IdBloque", bloque.IdBloque);//0 ID_BLOQUE
                    cmd.Parameters.AddWithValue("@Descripcion", bloque.Descripcion); //1 DESCRIPCION_CALC @Descripcion
                    cmd.Parameters.AddWithValue("@Orden", bloque.Orden);//2 ORDEN 
                    cmd.Parameters.AddWithValue("@Usuario", M.Constant.Usuario); //USUARIO_CREACION Y USUARIO_MOD
                    cmd.Parameters.AddWithValue("@TiempoActual", DateTime.Now); //FECHA_CREACION Y FECHA_MOD
                    cmd.ExecuteNonQuery();
                    cmd.Parameters.Clear();
                }
                
                return true;
            }
            catch (Exception exc)
            {
                Error = exc.Message;
                exc.Message.ToEditor();              
            }
            return false;
        }


        /// <summary>
        /// Runs the specified query.
        /// </summary>
        /// <param name="query">The query.</param>
        /// <returns>Verdadero si funciona el comando</returns>
        public Boolean Run(SqlCommand cmd)
        {
            try
            {
                cmd.Connection = this.Connection;
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception exc)
            {
                Error = exc.Message;
            }
            return false;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Connection.Close();
            this.Connection.Dispose();
        }
        /// <summary>
        /// Formats the specified value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static string Format(String value)
        {
            return value.Replace("'", "''");
        }

    }
}
