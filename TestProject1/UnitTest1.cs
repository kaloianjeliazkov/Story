using NUnit.Framework;
using RestSharp;
using RestSharp.Authenticators;
using System.Net;
using System.Text.Json;


namespace StorySpoiler
{
    [TestFixture]
    public class StoryTests
    {
        private RestClient client;
        private static string createdStoryId;
        private const string BaseUrl = "https://d3s5nxhwblsjbi.cloudfront.net";

        [OneTimeSetUp]
        public void Setup()
        {
            string token = GetJwtToken("KolBoi", "KolKol1");

            var options = new RestClientOptions(BaseUrl)
            {
                Authenticator = new JwtAuthenticator(token)
            };

            client = new RestClient(options);
        }

        private string GetJwtToken(string username, string password)
        {
            var loginClient = new RestClient(BaseUrl);
            var request = new RestRequest("/api/User/Authentication", Method.Post);

            request.AddJsonBody(new { username, password });

            var response = loginClient.Execute(request);

            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);

            return json.GetProperty("accessToken").GetString() ?? string.Empty;

        }

        [Test, Order(1)]

        public void CreateStory_ShouldReturnCreated()
        {
            var story = new
            {
                Title = "New story",
                Description = "Malko skuchno",
                Url = "http://123.png"
            };
            var request = new RestRequest("/api/Story/Create", Method.Post);
            request.AddJsonBody(story);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));

            var json = JsonSerializer.Deserialize<JsonElement>(response.Content);

            createdStoryId = json.GetProperty("storyId").GetString() ?? string.Empty;

            Assert.That(response.Content, Does.Contain("Successfully created!"));

            Assert.That(createdStoryId, Is.Not.Null.And.Not.Empty, "Story Id should not be empty");

        }

        [Test, Order(2)]
        public void EditStory_ShouldReturnOk_AndSuccessMessage()
        {
            Assert.That(createdStoryId, Is.Not.Null.And.Not.Empty, "Story Id should not be empty from creation step.");

            var changes = new
            {
                // API expects these names: title, description (url optional)
                title = "Updated story title",
                description = "New desc",
                url = ""
            };

            var request = new RestRequest($"/api/Story/Edit/{createdStoryId}", Method.Put);
            request.AddJsonBody(changes);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Content, Does.Contain("Successfully edited"));
        }

        [Test, Order(3)]

        public void GetAllStorySpoilers_ShouldReturnList()
         {
            var request = new RestRequest("/api/Story/All", Method.Get);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var spoilers = JsonSerializer.Deserialize<List<object>>(response.Content);

            Assert.That(spoilers, Is.Not.Empty);
         }

        [Test, Order(4)]

        public void DeleteSpoiler_ShouldReturnOk()
         {
            var request = new RestRequest($"/api/Story/Delete/{createdStoryId}", Method.Delete);
            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(response.Content, Does.Contain("Deleted successfully!"));
         }

        [Test, Order(5)]

        public void CreateSpoilerWithoutRequiredFields_ShouldReturnBadRequest()
         {
            var story = new
             {
              Title = "",
              Description = ""
             };

            var request = new RestRequest("/api/Story/Create", Method.Post);
            request.AddJsonBody(story);
            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
         }

        [Test, Order(6)]

        public void EditSpoilerThatDoesNotExist_ShouldReturnNotFound()
         {
            string fakeId = "123";
            var changes = new
             {               
              title = "new title",
              description = "new desc",
              url = ""
             };

            var request = new RestRequest($"/api/Story/Edit/{fakeId}", Method.Put);
            request.AddJsonBody(changes);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));

            Assert.That(response.Content, Does.Contain("No spoilers..."));
         }

        [Test, Order(7)]

        public void DeleteStoryThatDoesNotExist_ShouldReturnBadRequest()
         {
            string fakeId = "123";

            var request = new RestRequest($"/api/Story/Delete/{fakeId}", Method.Delete);
            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(response.Content, Does.Contain("Unable to delete this story spoiler!"));
         }
        
        [OneTimeTearDown]
        public void CleanUp()
        {
            client?.Dispose();
        }
    }
}