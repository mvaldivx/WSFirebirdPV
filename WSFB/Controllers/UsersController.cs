using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using FirebirdSql.Data.FirebirdClient;

namespace WSFB.Controllers
{
    public class UsersController : ApiController
    {
        Conexion con = new Conexion();


        [HttpGet]
        public IHttpActionResult ProbarConexion() {
            try
            {
                FbConnection Conexion = new FbConnection(con.connectionString);
                Conexion.Open();
                FbTransaction myTransaction = Conexion.BeginTransaction();
                FbCommand myCommand = new FbCommand();

                myCommand.Connection = Conexion;
                myCommand.Transaction = myTransaction;
                myCommand.Transaction.Dispose();
                Conexion.Close();
                return Ok("Conectado Exitosamente");
            }
            catch (Exception ex) {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public IHttpActionResult ValidaUsuario([FromUri] string usuario, [FromUri] string pass) {
            FbConnection Conexion = new FbConnection(con.connectionString);
            List<Object> Usuario = new List<Object>();
            try
            {
                Conexion.Open();
                FbTransaction myTransaction = Conexion.BeginTransaction();

                FbCommand myCommand = new FbCommand();

                myCommand.CommandText = "select * from USUARIOS WHERE USUARIO ='"+usuario+"' and CLAVE = '"+pass+"'";
                myCommand.Connection = Conexion;
                myCommand.Transaction = myTransaction;

                FbDataReader dr = myCommand.ExecuteReader();
                int c = 0;
                while (dr.Read())
                {
                    c += 1;
                    Usuario.Add(new
                    {
                        id = dr["ID"],
                        Nombre = dr["NOMBRE_COMPLETO"],
                        Usuario = dr["USUARIO"],
                        Permisos = dr["PERMISOS"],
                        Activo = dr["ACTIVO"]
                    });
                }

                myCommand.Transaction.Dispose();
                Conexion.Close();
                if (c == 0)
                    return BadRequest("Usuario y/o Contraseña incorrectos");
                else
                    return Ok(Usuario);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        /*[HttpGet]
        public IHttpActionResult GetUsers() {

            FbConnection Conexion = new FbConnection(con.connectionString);

            List<Object> Usuarios = new List<Object>();
            try
            {
                Conexion.Open();

                FbTransaction myTransaction = Conexion.BeginTransaction();

                FbCommand myCommand = new FbCommand();

                myCommand.CommandText = "SELECT * FROM USERS";
                myCommand.Connection = Conexion;
                myCommand.Transaction = myTransaction;

                
                 FbDataReader dr = myCommand.ExecuteReader();
                 while (dr.Read()) {
                    Usuarios.Add(new { usr= dr["usr"], pass = dr["pass"] , level = dr["level"]});
                 }

                myCommand.Transaction.Dispose();

                
            }
            catch (Exception e) {
                return BadRequest(e.Message);
            }
            Conexion.Close();
            return Ok(Usuarios);
        }
        */
        [HttpPost]
        public IHttpActionResult NuevoUsuario([FromUri]string user, [FromUri]string pass, [FromUri]int level) {
            FbConnection Conexion = new FbConnection(con.connectionString);
            try
            {
                
                Conexion.Open();
                int idnuevo = 0;
                FbTransaction myTransaction = Conexion.BeginTransaction();

                FbCommand myCommand = new FbCommand();
                myCommand.CommandText = "SELECT MAX(id) FROM USERS";
                myCommand.Connection = Conexion;
                myCommand.Transaction = myTransaction;
                FbDataReader dr = myCommand.ExecuteReader();
                while (dr.Read())
                {
                    idnuevo= Convert.ToInt32(dr.GetValue(0));
                }
                myCommand.Transaction.Dispose();

                FbTransaction myTransactionI = Conexion.BeginTransaction();
                myCommand.CommandText = "INSERT INTO USERS (id, usr, pass, level) VALUES (@id, @usr, @pass, @level)";
                myCommand.Connection = Conexion;
                myCommand.Transaction = myTransactionI;

                myCommand.Parameters.Add("@id", FbDbType.Integer).Value = idnuevo +1;
                myCommand.Parameters.Add("@usr", FbDbType.Text).Value = user;
                myCommand.Parameters.Add("@pass", FbDbType.Text).Value = pass;
                myCommand.Parameters.Add("@level", FbDbType.Integer).Value = level;
                myCommand.ExecuteNonQuery();
                myTransactionI.Commit();
                myTransactionI.Dispose();
            }
            catch (Exception e) {
                return BadRequest(e.Message);
            }
            Conexion.Close();
            return Ok("Registrado con exito");
        }

    }
}
