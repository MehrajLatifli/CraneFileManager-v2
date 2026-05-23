using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CraneFileManager.Domain.Entities.AuthModels;
using CraneFileManager.Application.Mapper.DTO.AuthDTO;
using Amazon.Runtime.Internal;
using CraneFileManager.Application.Mapper.DTO.NotificationDTO;

namespace CraneFileManager.Monitoring.ConsoleApp
{
    public static class ConnectionAPI
    {
        public static async Task<(GetUserDTOModel UserDTO, string Token, object Data)> LogiIn( HttpClient httpClient,string username, string password, string connectionurl, CancellationTokenSource cts)
        {
            cts.CancelAfter(TimeSpan.FromMinutes(2));
            var login = new Login
            {
                Username = username,
                Password = password
            };

            string token = string.Empty;
            GetUserDTOModel userDTO = null; // Initialize the return object
            object data = null; // Initialize data to avoid NullReferenceException

            try
            {
                if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
                {
                    string apiuser_jsondata = JsonConvert.SerializeObject(login);
                    var httpContent = new StringContent(apiuser_jsondata, Encoding.UTF8, "application/json");
                    var response = await httpClient.PostAsync(connectionurl, httpContent, cts.Token);

                    if (response.IsSuccessStatusCode)
                    {
                        token = await response.Content.ReadAsStringAsync();
                        JObject jObject = JObject.Parse(token);
                        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", jObject["token"]?.ToString());

                        // Get the user profile after logging in
                        var userProfileResponse = await httpClient.GetStringAsync("https://localhost:7272/api/v1/Auth/profile");
                        userDTO = JsonConvert.DeserializeObject<GetUserDTOModel>(userProfileResponse);
                    }
                    else
                    {
                        JObject jObject2 = JObject.Parse(await response.Content.ReadAsStringAsync());
                        data = new
                        {
                            Status = jObject2["status"]?.ToString(),
                            Title = jObject2["title"]?.ToString(),
                            User = jObject2["user"]?.ToString(),
                            Date = jObject2["date"]?.ToString(),
                            Machine = jObject2["machine"]?.ToString(),
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                // Optionally log or handle the exception
                Console.WriteLine($"\n Error: {ex.Message}");
            }

            return (userDTO, token, data); // Return the tuple including user data and error data
        }

        public static async Task<(string Token, NotificationDTOforGetandGetAll[] Messages, object Data)> Notification(
        HttpClient httpClient,
        string username,
        string password,
        string connectionUrl,
        CancellationTokenSource cts)
        {
            var login = new Login { Username = username, Password = password };
            string token = string.Empty;
            var messages = new List<NotificationDTOforGetandGetAll>();
            object data = null;
            cts.CancelAfter(TimeSpan.FromMinutes(2));
            try
            {
                // Log in to get the token
                string jsonData = JsonConvert.SerializeObject(login);
                var httpContent = new StringContent(jsonData, Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(connectionUrl, httpContent, cts.Token);

                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    var jObject = JObject.Parse(responseBody);
                    token = jObject["token"]?.ToString();

                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }

                // If we have a valid token, start streaming messages
                if (!string.IsNullOrEmpty(token))
                {
                    var messageResponse = await httpClient.GetAsync("https://localhost:7171/api/v1/Notification/messages", HttpCompletionOption.ResponseHeadersRead, cts.Token);

                    if (messageResponse.IsSuccessStatusCode)
                    {
                        using var stream = await messageResponse.Content.ReadAsStreamAsync(cts.Token);
                        using var reader = new StreamReader(stream);

                        Console.WriteLine("\n Streaming messages... \n");

                        while (!reader.EndOfStream)
                        {
                            var line = await reader.ReadLineAsync();

                            if (!string.IsNullOrEmpty(line))
                            {
                                // The line should be in the format: data: {"Id":"...","Title":"...","Description":"...","NotificationDate":"..."}
                                if (line.StartsWith("data: "))
                                {
                                    var jsonString = line.Substring("data: ".Length);
                                    var notification = JsonConvert.DeserializeObject<NotificationDTOforGetandGetAll>(jsonString);

                                    if (notification != null)
                                    {
                                        messages.Add(notification); 


                                        Console.WriteLine($" Message received --> {notification.Description}");
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        var errorResponse = await messageResponse.Content.ReadAsStringAsync();
                        data = JsonConvert.DeserializeObject<object>(errorResponse);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred while fetching messages: {ex.Message}");
            }

            return (token, messages.ToArray(), data); // Return messages as an array
        }

    }
}
