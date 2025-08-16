using System.Net;
using System.Text.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using RestSharp;
using RestSharp.Authenticators;
using Story.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;


namespace Story
{
    [TestFixture]
    public class StoryTests
    {
        private RestClient client;
        private static string createdStoryId;
       
        private const string baseUrl = "https://d3s5nxhwblsjbi.cloudfront.net";

        [OneTimeSetUp]
        public void Setup()
        {
           
            string token = GetJwtToken("nikolay", "nikolay");

            var options = new RestClientOptions(baseUrl)
            {
               Authenticator = new JwtAuthenticator(token)
            };

            client = new RestClient(options);
        }

        private string GetJwtToken(string username, string password)
        {
            var loginClient = new RestClient(baseUrl);
            var request = new RestRequest("/api/User/Authentication", Method.Post);

            request.AddJsonBody(new { username, password });

            var response = loginClient.Execute(request);

            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);

            return json.GetProperty("accessToken").GetString() ?? string.Empty;

        }

        

        [Test, Order(1)]
        public void CreateStory_Created() 
        {
            var story = new
            {
                Title = "New Story",
                Description = "Test story description",
                Url = ""
            };

            var request = new RestRequest("/api/Story/Create", Method.Post);
            request.AddJsonBody(story);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
        


            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);
            Assert.That(json.GetProperty("storyId").GetString(), Is.Not.Null.And.Not.Empty);
            Assert.That(json.GetProperty("msg").GetString(), Is.EqualTo("Successfully created!"));
            createdStoryId = json.GetProperty("storyId").GetString();

        }

        [Test, Order(2)]

        public void EditStory_ShouldReturnOkAndMessage()
        {
            var updatedStory = new
            {
                Title = "Edited Story",
                Description = "Updated description",
                Url = ""
            };

            
            var request = new RestRequest($"/api/Story/Edit/{createdStoryId}", Method.Put);
            request.AddJsonBody(updatedStory);

            var response = client.Execute(request);

            
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            
            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);

            
            Assert.That(json.GetProperty("msg").GetString(), Is.EqualTo("Successfully edited"));

        }

        [Test, Order(3)]

        public void GetAllStorySpoilers_ShouldReturnNonEmptyArray()
        {
            var request = new RestRequest("/api/Story/All", Method.Get);

            
            var response = client.Execute(request);

            
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            
            var jsonArray = JArray.Parse(response.Content);
            Assert.That(jsonArray.Count, Is.GreaterThan(0), "Response array is empty");
        }
       

        [Test, Order(4)]

        public void DeleteStory_ShoudReturnOk()
        {
            var request = new RestRequest($"/api/Story/Delete/{createdStoryId}", Method.Delete);

           
            var response = client.Execute(request);

          
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

          
            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);
            Assert.That(json.GetProperty("msg").GetString(), Is.EqualTo("Deleted successfully!"));
        }

        [Test, Order(5)]

        public void CreateStory_WithoutRequiredFields_ShouldReturnBadRequest()
        {
            var incompleteStory = new
            {
                Url = ""
            };

            var request = new RestRequest("/api/Story/Create", Method.Post);
            request.AddJsonBody(incompleteStory);

           
            var response = client.Execute(request);

           
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }


        [Test, Order(6)]

        public void EditNonExistingStory_ShouldReturnNotFound()
        {

            string nonExistingStoryId = "00000000-0000-0000-0000-000000000000";

            var updatedStory = new
            {
                Title = "Non-existing Story",
                Description = "Trying to edit a story that does not exist",
                Url = ""
            };

            var request = new RestRequest($"/api/Story/Edit/{nonExistingStoryId}", Method.Put);
            request.AddJsonBody(updatedStory);

          
            var response = client.Execute(request);

           
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));

            
            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);
            Assert.That(json.GetProperty("msg").GetString(), Is.EqualTo("No spoilers..."));

        }

        [Test, Order(7)]

        public void DeleteNonExistingStory_ShouldReturnBadRequest()
        {
                string nonExistingStoryId = "00000000-0000-0000-0000-000000000000";

                var request = new RestRequest($"/api/Story/Delete/{nonExistingStoryId}", Method.Delete);

               
                var response = client.Execute(request);

                
                Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

                
                var json = JsonSerializer.Deserialize<JsonElement>(response.Content);
                Assert.That(json.GetProperty("msg").GetString(), Is.EqualTo("Unable to delete this story spoiler!"));
            }

        

        [OneTimeTearDown]
        public void Cleanup()
        {
            client?.Dispose();
        }
    }
}