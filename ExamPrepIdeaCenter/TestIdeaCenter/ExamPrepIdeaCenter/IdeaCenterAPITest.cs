using System;
using System.Net;
using System.Text.Json;
using RestSharp;
using RestSharp.Authenticators;
using ExamPrepIdeaCenter.Models;


namespace ExamPrepIdeaCenter
{
    [TestFixture]
    public class Tests
    {
        private RestClient client;

        private const string BaseUrl = "http://144.91.123.158:82";
        private const string Statictoken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJKd3RTZXJ2aWNlQWNjZXNzVG9rZW4iLCJqdGkiOiI5ZGM4NDY4OC1iYmQ5LTRlODAtYjYxMy05ZGUyZjkwZWZkMDEiLCJpYXQiOiIwNC8xNS8yMDI2IDE4OjE0OjQ4IiwiVXNlcklkIjoiZTQ3ZTBmMmMtNDRkNy00Y2YyLTUzYTctMDhkZTc2YTJkM2VjIiwiRW1haWwiOiJwbGFtaWlpaUBzb2Z0dW5pLmJnIiwiVXNlck5hbWUiOiJQbGFtaWkiLCJleHAiOjE3NzYyOTg0ODgsImlzcyI6IklkZWFDZW50ZXJfQXBwX1NvZnRVbmkiLCJhdWQiOiJJZGVhQ2VudGVyX1dlYkFQSV9Tb2Z0VW5pIn0.e-q20iZ3zhipPuaM12tkjhtMaD5furwYO67P6RSVSgw";
        private static string lastCreateadIdeaId;

        private const string LogInEmail = "plamiiii@softuni.bg";
        private const string LogInPassword = "Password";

        public object Method { get; private set; }

        [OneTimeSetUp]
        public void Setup()
        {
            string jwtToken;

            if (!string.IsNullOrWhiteSpace(Statictoken))
            {
                jwtToken = Statictoken;
            }
            else
            {
                jwtToken = GetJwtToken(LogInEmail, LogInPassword);
            }
            var options = new RestClientOptions(BaseUrl)
            {
                Authenticator = new JwtAuthenticator(jwtToken)
            };

            this.client = new RestClient(options);

        }
        private string GetJwtToken(string email, string password)
        {
            var tempClient = new RestClient(BaseUrl);
            var request = new RestRequest("/api/User/Authentication", RestSharp.Method.Post);
            request.AddJsonBody(new { email, password });

            var response = tempClient.Execute(request);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var content = JsonSerializer.Deserialize<JsonElement>(response.Content);
                var token = content.GetProperty("token").GetString();

                if (string.IsNullOrWhiteSpace(token))
                {
                    throw new InvalidOperationException("Token not found in the response.");
                }
                return token;
            }
            else
            {
                throw new InvalidOperationException($"Failed to authenticate. Staus code: {response.StatusCode}, Response: {response.Content}");

            }


        }

        [Order(1)]
        [Test]
        public void CreateIdea_WithRequiredFIelds_ShouldReturnSuccess()
        {
            var ideaData = new IdeaDTO
            {
                Title = "Test Idea",
                Description = "This is a test idea description.",
                Url = ""
            };

            var request = new RestRequest("/api/Idea/Create", RestSharp.Method.Post);
            request.AddJsonBody(ideaData);

            var response = this.client.Execute(request);

            var createResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected status code 200 OK.");
            Assert.That(createResponse.Msg, Is.EqualTo("Successfully created!"));

        }
        [Order(2)]
        [Test]
        public void GetAllIdeas_ShouldReturnSuccess()
        {
            var request = new RestRequest("/api/Idea/All", RestSharp.Method.Get);
            var response = this.client.Execute(request);

            var responseItems = JsonSerializer.Deserialize<List<ApiResponseDTO>>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected status code 200 OK.");
            Assert.That(responseItems, Is.Not.Empty);
            Assert.That(responseItems, Is.Not.Null);

            lastCreateadIdeaId = responseItems.LastOrDefault()?.Id;

        }

        [Order(3)]
        [Test]

        public void EditExistingIdea_ShouldReturnSuccess()
        {
            var editRequestData = new IdeaDTO
            {
                Title = "Edited Idea",
                Description = "This is a edited idea description.",
                Url = ""
            };


            var request = new RestRequest("/api/Idea/Edit", RestSharp.Method.Put);

            request.AddQueryParameter("ideaId", lastCreateadIdeaId);
            request.AddJsonBody(editRequestData);

            var response = this.client.Execute(request);

            var editResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected status code 200 OK.");
            Assert.That(editResponse.Msg, Is.EqualTo("Edited successfully"));
        }


        [Order(4)]
        [Test]

        public void DeleteIdea_ShouldReturnSuccess()
        {
            var request = new RestRequest("/api/Idea/Delete", RestSharp.Method.Delete);
            request.AddQueryParameter("ideaId", lastCreateadIdeaId);
            var response = this.client.Execute(request);


            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected status code 200 OK.");
            Assert.That(response.Content, Is.EqualTo("\"The idea is deleted!\""));
        }

        [Order(5)]
        [Test]
        public void CreateIdea_WithMissingRequiredFields_ShouldReturnBadRequest()
        {
            var ideaData = new IdeaDTO
            {
                Title = "",
                Description = "This is a test idea description.",
                Url = ""
            };
            var request = new RestRequest("/api/Idea/Create", RestSharp.Method.Post);
            request.AddJsonBody(ideaData);

            var response = this.client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), "Expected status code 400 Bad Request.");
        }

        [Order(6)]
        [Test]

        public void EditNonExistingIdea_ShouldReturnNotFound()
        {
            string nonExistingIdeaId = "9999999";
            var editRequestData = new IdeaDTO
            {
                Title = "Edited Idea",
                Description = "This is a edited idea description.",
                Url = ""
            };
            var request = new RestRequest("/api/Idea/Edit", RestSharp.Method.Put);
            request.AddQueryParameter("ideaId", nonExistingIdeaId);
            request.AddJsonBody(editRequestData);

            var response = this.client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), "Expected status code 400 Bad Request.");
        }

        [Order(7)]
        [Test]

        public void DeleteNonExistingIdea_ShouldReturnNotFound()
        {
            string nonExistingIdeaId = "9999999";

            var request = new RestRequest("/api/Idea/Delete", RestSharp.Method.Delete);
            request.AddQueryParameter("ideaId", nonExistingIdeaId);
            var response = this.client.Execute(request);


            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest), "Expected status code 400 Bad Request.");
            Assert.That(response.Content, Is.EqualTo("\"There is no such idea!\""));
        }

 

        [OneTimeTearDown]
        public void TearDown()
        {
            this.client?.Dispose();
        }
    }
}

   