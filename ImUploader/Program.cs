using Microsoft.AspNetCore.Mvc;
using System.Net;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace ImgUploader
{
    public class Program
    {
        const string C_TEST = "/test";
        const string C_IMGUPLOADER = "/imgUploader";

        static string _savePath;
        public static void Main(string[] args)
        {
            int httpPort = Convert.ToInt32(args[0]);
            int httpsPort = Convert.ToInt32(args[1]);
            _savePath = args[2];

            Console.WriteLine("Service start");
            Console.WriteLine("http port " + httpPort);
            Console.WriteLine("https port " + httpsPort);
            Console.WriteLine("save path: " + _savePath);
            Console.WriteLine("GET: " + C_TEST);
            Console.WriteLine("POST: " + C_IMGUPLOADER);

            var builder = WebApplication.CreateBuilder(args);
            builder.WebHost
            //.UseSetting(WebHostDefaults.SuppressStatusMessagesKey, "True")
            .ConfigureKestrel((context, serverOptions) =>
            {
                
                    serverOptions.Listen(IPAddress.Loopback, httpPort);
                    serverOptions.Listen(IPAddress.Loopback, httpsPort, listenOptions =>
                    {
                        listenOptions.UseHttps();//nada de certificados.... por ahora-

                    });
            });

            // Configuración de registro para limitar los mensajes de registro
            //builder.Logging.ClearProviders(); // Limpia todos los proveedores de registro existentes
            //builder.Logging.AddConsole(); // Agrega el proveedor de registro de consola
            //builder.Logging.SetMinimumLevel(LogLevel.Warning); // Establece el nivel de registro deseado (en este caso, Warning o superior)


            var app = builder.Build();
  
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet(C_TEST, context => context.Response.WriteAsync("ready online"));
                endpoints.MapPost(C_IMGUPLOADER, HandleImageUpload);
            });

            app.Run();

        }

        private static async Task HandleImageUpload(HttpContext context)
        {
            try
            {
                var request = context.Request;
                var form = await request.ReadFormAsync();

                var userId = form["UserId"];
                var imageFile = form.Files["Image"];

                if (!Directory.Exists(_savePath + "\\"+ userId))
                {
                    Directory.CreateDirectory(_savePath + "\\"+ userId);    
                }

                // Verifica si se envió un archivo
                if (imageFile != null && imageFile.Length > 0)
                {
                    // Construye la ruta completa del archivo en el directorio "C:\prueba"
                    string filePath = Path.Combine(_savePath + "\\" + userId, imageFile.FileName);

                    // Abre un stream de archivo para escribir el contenido del archivo recibido
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }

                    await context.Response.WriteAsync("ok");
                }
                else
                {
                    // Si no se envió ningún archivo, regresamos un error 400 (Bad Request)
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsync("400");
                }
            }
            catch (Exception ex)
            {
                // Si ocurre una excepción, regresamos un error 500 (Internal Server Error)
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsync(ex.Message);
            }
        }


    }

   


}