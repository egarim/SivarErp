namespace Tests
{
    public class TestWeatherController: MultiServerBaseTest
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public async Task Test1()
        {
            HttpClient httpClient = this.GetTestClientFactory().CreateClient("TestClient");
         await   httpClient.GetAsync("/WeatherForecast").ContinueWith(responseTask =>
            {
                var response = responseTask.Result;
                Assert.That(response.IsSuccessStatusCode, Is.True);
                Assert.That(response.Content.ReadAsStringAsync().Result, Is.Not.Null.Or.Empty);
            });
        }
    }
}
