using FirebirdSql.Data.FirebirdClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;


namespace WSFB.Controllers
{
    public class ProductosController : ApiController
    {
        Conexion con = new Conexion();
        [HttpGet]
        public IHttpActionResult GetProductos()
        {
            
            FbConnection Conexion = new FbConnection(con.connectionString);

            List<Object> Productos = new List<Object>();
            try
            {
                Conexion.Open();

                FbTransaction myTransaction = Conexion.BeginTransaction();

                FbCommand myCommand = new FbCommand();

                myCommand.CommandText = "SELECT * FROM Productos";
                myCommand.Connection = Conexion;
                myCommand.Transaction = myTransaction;


                FbDataReader dr = myCommand.ExecuteReader();
                while (dr.Read())
                {
                    Productos.Add(new {
                        id = dr["id"],
                        codigo = dr["codigo"],
                        Descripcion = dr["Descripcion"],
                        Existencia = dr["Existencia"],
                        Costo = dr["Costo"]
                    });
                }

                myCommand.Transaction.Dispose();


            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
            Conexion.Close();
            return Ok(Productos);
        }

        [HttpPost]
        public IHttpActionResult NuevoProducto([FromUri]string codigo, [FromUri]string descripcion, [FromUri]int existencia, [FromUri]float costo)
        {
            FbConnection Conexion = new FbConnection(con.connectionString);
            try
            {
                Conexion.Open();

                FbTransaction myTransaction = Conexion.BeginTransaction();
                FbCommand myCommand = new FbCommand();
                
                myCommand.CommandText = "INSERT INTO Productos ( Codigo, Descripcion, Existencia, Costo) VALUES (@Codigo, @Descripcion, @Existencia, @Costo)";
                myCommand.Connection = Conexion;
                myCommand.Transaction = myTransaction;

                myCommand.Parameters.Add("@Codigo", FbDbType.Text).Value = codigo;
                myCommand.Parameters.Add("@Descripcion", FbDbType.Text).Value = descripcion;
                myCommand.Parameters.Add("@Existencia", FbDbType.Integer).Value = existencia;
                myCommand.Parameters.Add("@Costo", FbDbType.Float).Value = costo;
                myCommand.ExecuteNonQuery();
                myTransaction.Commit();
                myTransaction.Dispose();
            }
            catch (Exception e)
            {
                return BadRequest(e.Message);
            }
            Conexion.Close();
            return Ok("Producto registrado con exito");
        }
    }
}
