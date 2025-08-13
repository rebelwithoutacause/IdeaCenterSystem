using System;
using System.Net;
using IdeaAPI.Models;
using NUnit.Framework;
using RestSharp;
using RestSharp.Authenticators;
using System.Net;
using System.Text.Json;

namespace IdeaAPI
{
    [TestFixture]
    public class IdeaAPITests
    {
        private RestClient _client;
        private static string? lastCreatedIdeaId;

        private const string BaseUrl = "http://softuni-qa-loadbalancer-2137572849.eu-north-1.elb.amazonaws.com:84";

        // Your static token
        private const string StaticToken =
            "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJKd3RTZXJ2aWNlQWNjZXNzVG9rZW4iLCJqdGkiOiIzZTk5NzA0Mi0yM2Q5LTQwNjQtOGJkNS02NDQ4NWFhYmE3NmUiLCJpYXQiOiIwOC8xMi8yMDI1IDE2OjA5OjUyIiwiVXNlcklkIjoiOGUwOTdiMWMtMjU4OC00ZDIwLWQyODUtMDhkZGQ0ZTA4YmQ4IiwiRW1haWwiOiJ0ZWRvQGFidi5iZyIsIlVzZXJOYW1lIjoidGVkbyIsImV4cCI6MTc1NTAzNjU5MiwiaXNzIjoiSWRlYUNlbnRlcl9BcHBfU29mdFVuaSIsImF1ZCI6IklkZWFDZW50ZXJfV2ViQVBJX1NvZnRVbmkifQ.EkIOxDBuQ7esGQSAgQ4lNLGHEARDun_ktWxc81MdBhI";

        // Credentials for fallback login
        private const string LoginEmail = "tedo@abv.bg";
        private const string Password = "123456";

        [OneTimeSetUp]
        public void Setup()
        {
            string jwtToken;

            if (!string.IsNullOrWhiteSpace(StaticToken))
            {
                jwtToken = StaticToken;
            }
            else
            {
                jwtToken = GetJwtToken(LoginEmail, Password);
            }

            var options = new RestClientOptions(BaseUrl)
            {
                Authenticator = new JwtAuthenticator(jwtToken)
            };

            this._client = new RestClient(options);
        }

      
    
        private string GetJwtToken(string email, string password)
        {
            var tempClient = new RestClient(BaseUrl);
            var request = new RestRequest("/api/User/Authentication", Method.Post);
            request.AddJsonBody(new
            {
                email, password
            });
            var response = tempClient.Execute(request);

            if (response.StatusCode == HttpStatusCode.OK) 
            {
              var content = JsonSerializer.Deserialize<JsonElement>(response.Content);
              var token = content.GetProperty("accessToken").GetString();

                if (string.IsNullOrWhiteSpace(token)) 
                {
                    throw new InvalidOperationException("Failed to retrieve JWT token from the response");
                }
                return token;
            }
            else 
            {
              throw new InvalidOperationException();
            }
            
        }

        [Order(1)]
        [Test]
        public void CreateNewIdea_WithCorrectData_ShouldSucceed()
        {
            var ideaRequest = new IdeaDTO
            {
                Title = "New Idea",
                Description = "Description",
                url = ""

            };

            var request = new RestRequest("/api/Idea/Create", Method.Post);
            request.AddJsonBody(ideaRequest);
            var response = this._client.Execute(request);
            var createResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(createResponse.Msg, Is.EqualTo("Successfully created!"));
        }

        [Order(2)]
        [Test]

        public void GetAllIdeas_ShouldReturnListOfIdeas() 
        {
            var request = new RestRequest("/api/Idea/All", Method.Get);
            var response = this._client.Execute(request);
            
            var responseItems = JsonSerializer.Deserialize<List<ApiResponseDTO>>(response.Content);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(responseItems, Is.Not.Null);
            Assert.That(responseItems, Is.Not.Empty);

            lastCreatedIdeaId = responseItems.LastOrDefault()?.Id;


        }
        [Order(3)]
        [Test]

        public void EditExistingIdea_ShouldReturnSuccess() 
        {

            var editedIdeaRequest = new IdeaDTO
            {
                
                Title = "New Edited Idea",
                url = "",
                Description = "New Edited Description",
            };
            
            
            var request = new RestRequest($"/api/Idea/Edit", Method.Put);
            request.AddQueryParameter("IdeaId", lastCreatedIdeaId);
            request.AddJsonBody(editedIdeaRequest);
            var response = this._client.Execute(request);
            var editResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);
            
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(editResponse.Msg, Is.EqualTo("Edited successfully"));


        }

        [Order(4)]
        [Test]

        public void DeleteExistingIdea_ShouldReturnSuccess() 
        {
            var request = new RestRequest($"/api/Idea/Delete", Method.Delete);
            request.AddQueryParameter("ideaId", lastCreatedIdeaId);
            var response = this._client.Execute(request);
            

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Content, Does.Contain("The idea is deleted!"));

        }

        [Order(5)]
        [Test]


        public void CreateIdea_WithoutRequiredFields_ShouldReturnsBadRequest() 
        {

            var ideaRequest = new IdeaDTO() 
            {
              Title = "",
              Description = "",
            
            };

            var request = new RestRequest("/api/Idea/Create", Method.Post);
            request.AddJsonBody(ideaRequest);
            var response = this._client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        
        }

        [Order(6)]
        [Test]

        public void EditNonExistingIdea_ShouldReturnNotFound() 
        {
            string nonExistingIdeaId = "666";

            var editRequest = new IdeaDTO
            {
                Title = "Edited Non-Existing Idea",
                Description = "Updated Non-Existing Idea",
                url = ""

            };

            var request = new RestRequest("/api/Idea/Edit", Method.Put);
            request.AddQueryParameter("ideaId", nonExistingIdeaId);
            request.AddJsonBody(editRequest);
            var response = this._client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(response.Content, Does.Contain("There is no such idea!"));

        }

        [Order(7)]
        [Test]

        public void DeleteNonExistingIdea_ShouldReturnBadRequest() 
        {  
            string nonExistingIdeaId = "123456789";

            var request = new RestRequest("/api/Idea/Delete", Method.Delete);
            request.AddQueryParameter("ideaId", nonExistingIdeaId);
            var response = this._client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(response.Content, Does.Contain("There is no such idea!"));
        
        }
        [OneTimeTearDown]
        public void Teardown() 
        {
         this._client.Dispose();
        }

        public class LoginResponse
        {
            public string Token { get; set; }
        }
    }
}
