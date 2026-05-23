using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using CraneFileManager.Application.Mapper.DTO.AuthDTO;

namespace CraneFileManager.Monitoring.ConsoleApp
{
    public class Program
    {
        private static async Task Main(string[] args)
        {
            bool exit = false;
            string username = string.Empty;
            string password = string.Empty;

            using var clientHandler = new HttpClientHandler();
            using var httpClient = new HttpClient(clientHandler);
            var cts = new CancellationTokenSource();
            var cts2 = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromMinutes(2));
            cts2.CancelAfter(TimeSpan.FromMinutes(2));

            while (!exit)
            {
                Console.Clear();
                Console.WriteLine("\n Menu of Monitoring: \n");
                Console.WriteLine(" 1. Log In");
                Console.WriteLine(" 2. Get Notifications");
                Console.WriteLine(" 3. Exit");
                Console.Write("\n Select an option: ");

                string input = Console.ReadLine();

                switch (input)
                {
                    case "1":
                        Console.Write("\n Enter Username: ");
                        username = Console.ReadLine();
                        Console.Write(" Enter Password: ");
                        password = Console.ReadLine();

                        var loginResponse = await ConnectionAPI.LogiIn(httpClient, username, password, "https://localhost:7272/api/v1/Auth/login", cts);

                        if (loginResponse.UserDTO != null)
                        {
                            Console.WriteLine("\n Welcome: " + loginResponse.UserDTO.Username + "\n");
                        }
                        else if (loginResponse.Data != null)
                        {
                            HandleErrorResponse(loginResponse.Data);
                        }
                        else
                        {
                            Console.WriteLine("\n Login successful, but user data is not available.\n");
                        }
                        break;

                    case "2":
                        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                        {
                            Console.WriteLine("\n You need to log in first.");
                        }
                        else
                        {
                            var result = await ConnectionAPI.Notification(httpClient, username, password, "https://localhost:7171/api/v1/Notification/login", cts);
                            var messages = result.Messages;

                            if (messages == null)
                            {
                                Console.WriteLine("No messages received or there was an error.");
                            }

                        }
                        break;

                    case "3":
                        exit = true;
                        break;

                    default:
                        Console.WriteLine("\n Invalid option. Please try again.");
                        break;
                }

                Console.WriteLine("\n Press any key to continue...");
                Console.ReadKey();
            }
        }

        private static void HandleErrorResponse(object data)
        {
            var errorData = data as dynamic;
            if (errorData != null)
            {
                Console.WriteLine("\n Login failed.");
                Console.WriteLine($"   |-> Status: {errorData.Status ?? "N/A"}");
                Console.WriteLine($"   |-> Title: {errorData.Title ?? "N/A"}");
                Console.WriteLine($"   |-> User: {errorData.User ?? "N/A"}");
                Console.WriteLine($"   |-> Date: {errorData.Date ?? "N/A"}");
                Console.WriteLine($"   |-> Machine: {errorData.Machine ?? "N/A"}");
                Console.WriteLine("\n\n");
            }
        }
    }
}
