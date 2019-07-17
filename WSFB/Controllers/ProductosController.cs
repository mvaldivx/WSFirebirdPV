using FirebirdSql.Data.FirebirdClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using MySql.Data;
using MySql.Data.MySqlClient;
using System.Globalization;

namespace WSFB.Controllers
{
    public class ProductosController : ApiController
    {
        Conexion con = new Conexion();

        [HttpGet]
        public IHttpActionResult GetProductosFiltrados([FromUri] string tipo, [FromUri] string cadena) {
            FbConnection Conexion = new FbConnection(con.connectionString);
            List<Object> Productos = new List<Object>();
            try
            {
                Conexion.Open();
                FbTransaction myTransaction = Conexion.BeginTransaction();

                FbCommand myCommand = new FbCommand();

                myCommand.CommandText = "select P.ID,P.CODIGO,P.DESCRIPCION,CANTIDAD_ACTUAL,PCOSTO,PVENTA,UMEDIDA from PRODUCTOS P LEFT JOIN INVENTARIO_BALANCES I ON P.ID = I.PRODUCTO_ID WHERE UPPER(P.CODIGO) = '"+cadena+"'";
                myCommand.Connection = Conexion;
                myCommand.Transaction = myTransaction;

                FbDataReader dr = myCommand.ExecuteReader();
                while (dr.Read())
                {
                    Productos.Add(new
                    {
                        id = dr["ID"],
                        codigo = dr["CODIGO"],
                        Descripcion = dr["DESCRIPCION"],
                        Existencia = dr["CANTIDAD_ACTUAL"],
                        Costo = dr["PCOSTO"],
                        Venta = dr["PVENTA"],
                        UMedida = dr["UMEDIDA"]
                    });
                }

                myCommand.Transaction.Dispose();
                Conexion.Close();

                return Ok(Productos);
            }
            catch (Exception ex) {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public IHttpActionResult ObtieneProductos() {
            FbConnection Conexion = new FbConnection(con.connectionString);
            List<Object> Productos = new List<Object>();
            try
            {
                Conexion.Open();
                FbTransaction myTransaction = Conexion.BeginTransaction();

                FbCommand myCommand = new FbCommand();

                myCommand.CommandText = "SELECT * FROM PRODUCTOS P LEFT JOIN INVENTARIO_BALANCES I ON P.ID = I.PRODUCTO_ID";
                myCommand.Connection = Conexion;
                myCommand.Transaction = myTransaction;

                FbDataReader dr = myCommand.ExecuteReader();
                while (dr.Read())
                {
                    Productos.Add(new
                    {
                        id = dr["ID"],
                        codigo = dr["CODIGO"],
                        Descripcion = dr["DESCRIPCION"],
                        Existencia = dr["CANTIDAD_ACTUAL"],
                        Costo = dr["PCOSTO"],
                        Venta = dr["PVENTA"],
                        UMedida = dr["UMEDIDA"]
                    });
                }

                myCommand.Transaction.Dispose();
                Conexion.Close();
                return Ok(Productos);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public IHttpActionResult ObtieneMovimientos([FromUri] int dia, [FromUri] int mes , [FromUri] int anio) {
            var date = anio + "-" + mes + "-" + dia + " 00:00";
            DateTime fecha = Convert.ToDateTime(date);
            DateTime sigFecha = Convert.ToDateTime(date).AddDays(1);
            List<Object> data = new List<Object>();
            FbConnection Conexion = new FbConnection(con.connectionString);
            try
            {
                Conexion.Open();
                FbTransaction myTransaction = Conexion.BeginTransaction();

                FbCommand myCommand = new FbCommand();

                myCommand.CommandText = "SELECT CUANDO_FUE, P.DESCRIPCION, I.DESCRIPCION AS MOVIMIENTO, CANTIDAD_ANTERIOR,CANTIDAD,USUARIO" +
                                            "FROM INVENTARIO_HISTORIAL I LEFT JOIN PRODUCTOS P ON P.ID = I.PRODUCTO_ID"+
                                            "LEFT JOIN USUARIOS U ON I.USUARIO_ID = U.ID WHERE CUANDO_FUE BETWEEN '"+fecha+"' AND '"+sigFecha+"'";
                myCommand.Connection = Conexion;
                myCommand.Transaction = myTransaction;

                FbDataReader dr = myCommand.ExecuteReader();
                while (dr.Read())
                {
                    data.Add(new
                    {
                        Fecha = dr["CUANDO_FUE"],
                        Descripcion = dr["DESCRIPCION"],
                        Movimiento = dr["MOVIMIENTO"],
                        Habia = dr["CANTIDAD_ANTERIOR"],
                        Tipo = 1,
                        Cantidad = dr["CANTIDAD"],
                        Cajero = dr["USUARIO"]
                    });
                }

                myCommand.Transaction.Dispose();
                Conexion.Close();
                return Ok(data);
            }
            catch (Exception ex) {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public IHttpActionResult ActualizaInventario([FromUri] float venta, [FromUri] int IdUsuario, [FromUri] int id, [FromUri] string codigo, [FromUri]int cant) {
            try
            {

                FbConnection Conexion = new FbConnection(con.connectionString);
                Conexion.Open();
                FbTransaction myTransaction = Conexion.BeginTransaction();

                FbCommand myCommand = new FbCommand();

                myCommand.CommandText = "SELECT CANTIDAD_ACTUAL FROM INVENTARIO_BALANCES WHERE PRODUCTO_ID = " + id ;
                myCommand.Connection = Conexion;
                myCommand.Transaction = myTransaction;

                FbDataReader dr = myCommand.ExecuteReader();
                var nr = 0;
                var ActCant = 0;
                while (dr.Read())
                {
                    nr += 1;
                    ActCant = Convert.ToInt16(dr["CANTIDAD_ACTUAL"]);
                }
                myCommand.Transaction.Dispose();
                Conexion.Close();
                if (nr > 0)
                {
                    if (ActCant > 0)
                        cant += ActCant;
                    Conexion.Open();
                    FbTransaction Transactionupdate = Conexion.BeginTransaction();

                    FbCommand commandUpdate = new FbCommand();
                    commandUpdate.CommandText = "UPDATE INVENTARIO_BALANCES SET CANTIDAD_ACTUAL  = " + cant + " WHERE PRODUCTO_ID =" + id;
                    commandUpdate.Connection = Conexion;
                    commandUpdate.Transaction = Transactionupdate;

                    commandUpdate.ExecuteNonQuery();
                    Transactionupdate.Commit();
                    Transactionupdate.Dispose();
                    Conexion.Close();
                }
                else {
                    Conexion.Open();
                    FbTransaction Transactioninsert = Conexion.BeginTransaction();

                    FbCommand commandinsert = new FbCommand();
                    commandinsert.CommandText = "INSERT INTO INVENTARIO_BALANCES (PRODUCTO_ID, CANTIDAD_ACTUAL,ALMACEN_ID) VALUES ("+id+","+ cant+"," +1+")";
                    commandinsert.Connection = Conexion;
                    commandinsert.Transaction = Transactioninsert;

                    commandinsert.ExecuteNonQuery();
                    Transactioninsert.Commit();
                    Transactioninsert.Dispose();
                    Conexion.Close();
                }

                Conexion.Open();
                FbTransaction TransactionQuery = Conexion.BeginTransaction();

                FbCommand commandQuery = new FbCommand();
                commandQuery.CommandText = "SELECT MAX(ID) FROM INVENTARIO_RECIBOS";
                commandQuery.Connection = Conexion;
                commandQuery.Transaction = TransactionQuery;

                FbDataReader qr = commandQuery.ExecuteReader();
                var ID_Inventario=0;
                
                while (qr.Read())
                {
                    ID_Inventario = Convert.ToInt16(qr["MAX"]);
                }
                ID_Inventario += 1;
                commandQuery.Transaction.Dispose();
                Conexion.Close();

                //Inserta en Inventario_Recibos
                Conexion.Open();
                FbTransaction TransactioninvR = Conexion.BeginTransaction();

                FbCommand commandinvR = new FbCommand();
                commandinvR.CommandText = "INSERT INTO INVENTARIO_RECIBOS (FOLIO,RECIBIDO_EN, ORDEN_DE_COMPRA_ID, CAJA_ID, USUARIO_ID, ALMACEN_ID) VALUES "+
                            "(@IdInventario,@Date,@ordenCompra,"+ 1 + "," + IdUsuario + ","+ 1+")";
                commandinvR.Parameters.AddWithValue("@IdInventario", ID_Inventario);
                commandinvR.Parameters.AddWithValue("@Date", DateTime.Now);
                commandinvR.Parameters.AddWithValue("@ordenCompra", null);
                commandinvR.Connection = Conexion;
                commandinvR.Transaction = TransactioninvR;

                commandinvR.ExecuteNonQuery();
                TransactioninvR.Commit();
                TransactioninvR.Dispose();
                Conexion.Close();

                //Inserta en INVENTARIO_RECIBOS_DETALLE
                Conexion.Open();
                FbTransaction TransactioninvRD = Conexion.BeginTransaction();

                FbCommand commandinvRD = new FbCommand();
                commandinvRD.CommandText = "INSERT INTO INVENTARIO_RECIBOS_DETALLE (INVENTARIO_RECIBO_ID, SECUENCIA, PRODUCTO_ID, CANTIDAD_RECIBIDA, COSTO_UNITARIO) VALUES "+
                            "("+ID_Inventario+","+ 1+","+ id +"," + cant +","+ venta +")";
                commandinvRD.Connection = Conexion;
                commandinvRD.Transaction = TransactioninvRD;

                commandinvRD.ExecuteNonQuery();
                TransactioninvRD.Commit();
                TransactioninvRD.Dispose();
                Conexion.Close();

                //Inserta en INVENTARIO_HISTORIAL
                Conexion.Open();
                FbTransaction TransactioninvH = Conexion.BeginTransaction();

                FbCommand commandinvH = new FbCommand();
                commandinvH.CommandText = "INSERT INTO INVENTARIO_HISTORIAL (PRODUCTO_ID, CUANDO_FUE, CANTIDAD_ANTERIOR, CANTIDAD, DESCRIPCION, COSTO_UNITARIO, COSTO_DESPUES, AJUSTE_ID, RECIBO_INVENTARIO_ID, VENTA_ID, TRANSFERENCIA_ID, CAJA_ID, VENTA_POR_KIT, USUARIO_ID, ALMACEN_ID ) VALUES "+
                    "(@id,@Date,"+ ActCant+",@CantAnt, @Concepto," +venta+","+venta+",@nul,"+ID_Inventario+",@nul,@nul,"+ 1+","+ 0+","+ IdUsuario+"," +1+")";
                commandinvH.Parameters.AddWithValue("@id", id);
                commandinvH.Parameters.AddWithValue("@Date",DateTime.Now);
                commandinvH.Parameters.AddWithValue("@CantAnt", (cant - ActCant));
                commandinvH.Parameters.AddWithValue("@nul", null);
                commandinvH.Parameters.AddWithValue("@Concepto", "Recepcion de inventario #" + ID_Inventario );
                commandinvH.Connection = Conexion;
                commandinvH.Transaction = TransactioninvH;

                commandinvH.ExecuteNonQuery();
                TransactioninvH.Commit();
                TransactioninvH.Dispose();
                Conexion.Close();

                return Ok("Actualizado Correctamente");
            }
            catch (Exception ex) {
                return BadRequest(ex.Message);
            }
        }

        /*[HttpGet]
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

        [HttpGet]
        public IHttpActionResult ReplicaProductos([FromUri] string a)
        {
            FbConnection Conexion = new FbConnection(con.connectionString);
            MySqlConnection conn = new MySqlConnection(con.ConnectionStringMysql);

            List<Object> Productos = new List<Object>();
            try
            {
                Conexion.Open();

                FbTransaction myTransaction = Conexion.BeginTransaction();

                FbCommand myCommand = new FbCommand();

                myCommand.CommandText = "SELECT Descripcion FROM Productos";
                myCommand.Connection = Conexion;
                myCommand.Transaction = myTransaction;


                FbDataReader dr = myCommand.ExecuteReader();
                while (dr.Read())
                {
                    Productos.Add(new
                    {
                        idCategoria = 1,
                        VBuscada = 1,
                        Palabra = dr["Descripcion"]
                    });
                }

                myCommand.Transaction.Dispose();
                Conexion.Close();


                conn.Open();
                foreach (dynamic p in Productos)
                {
                    string query = "INSERT INTO diccionariobusqueda (Palabra,IdCategoria,VBuscada) Values ('" + p.Palabra + "',1,1)";
                    MySqlCommand cmd = new MySqlCommand(query, conn);
                    cmd.ExecuteNonQuery();
                }

                conn.Close();
            }
            catch (Exception e)
            {
                conn.Close();
                Conexion.Close();
                return BadRequest(e.Message);
            }



            return Ok("Replicado Correctamente");
        }
        */

    }
}
