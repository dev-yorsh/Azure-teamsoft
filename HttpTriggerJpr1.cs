using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;
using Company.Function.Jpr.Models;

namespace Company.Function.Jpr
{
    public class HttpTriggerJpr1
    {
        private readonly ILogger<HttpTriggerJpr1> _logger;
        private const string BaseUrl = "productos";

        public HttpTriggerJpr1(ILogger<HttpTriggerJpr1> logger)
        {
            _logger = logger;
        }

        [Function("listar-productos")]
        public async Task<HttpResponseData> GetProductos([HttpTrigger(AuthorizationLevel.Function, "get", Route = BaseUrl + "/listar-productos")] HttpRequestData req)
        {
            _logger.LogInformation("Ejecutando la función para listar productos.");

            var productos = new List<object>();

            using (SqlConnection conn = new SqlConnection(Constants.ConnectionString))
            {
                conn.Open();
                var text = "SELECT * FROM Producto";

                using (SqlCommand cmd = new SqlCommand(text, conn))
                {
                    var reader = await cmd.ExecuteReaderAsync();
                    while (await reader.ReadAsync())
                    {
                        productos.Add(new
                        {
                            IdProducto = reader["IdProducto"],
                            Nombre = reader["Nombre"],
                            Descripcion = reader["Descripcion"],
                            Precio = reader["Precio"],
                            Stock = reader["Stock"],
                            FechaCreacion = reader["FechaCreacion"],
                            Categoria = reader["Categoria"],
                            Marca = reader["Marca"],
                            Activo = reader["Activo"]
                        });
                    }
                }
            }

            var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
            await response.WriteAsJsonAsync(productos);
            return response;
        }

        [Function("insertar-producto")]
        public async Task<HttpResponseData> InsertProducto([HttpTrigger(AuthorizationLevel.Function, "post", Route = BaseUrl + "/insertar-productos")] HttpRequestData req)
        {
            _logger.LogInformation("Ejecutando la función para insertar un producto.");
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            var input = JsonConvert.DeserializeObject<Producto>(requestBody);

            using (SqlConnection conn = new SqlConnection(Constants.ConnectionString))
            {
                conn.Open();
                var text = "INSERT INTO Producto (Nombre, Descripcion, Precio, Stock, Categoria, Marca) VALUES (@Nombre, @Descripcion, @Precio, @Stock, @Categoria, @Marca)";

                using (SqlCommand cmd = new SqlCommand(text, conn))
                {
                    cmd.Parameters.AddWithValue("@Nombre", input.Nombre ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Descripcion", input.Descripcion ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Precio", input.Precio);
                    cmd.Parameters.AddWithValue("@Stock", input.Stock);
                    cmd.Parameters.AddWithValue("@Categoria", input.Categoria ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Marca", input.Marca ?? (object)DBNull.Value);

                    await cmd.ExecuteNonQueryAsync();
                }
            }

            var response = req.CreateResponse(System.Net.HttpStatusCode.OK);
            await response.WriteStringAsync("Producto insertado exitosamente.");
            return response;
        }
    }
}