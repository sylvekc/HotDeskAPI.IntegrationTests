using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using HotDeskAPI.Entities;
using HotDeskAPI.Models;
using Microsoft.AspNetCore.Authorization.Policy;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Newtonsoft.Json;

namespace HotDeskAPI.IntegrationTests
{
    public class DeskControllerTests
    {
        private HttpClient _client;

        public DeskControllerTests()
        {
            _client = new WebApplicationFactory<Startup>().WithWebHostBuilder(builder =>
           {
               builder.ConfigureServices(services =>
               {
                   var dbContextOptions = services.SingleOrDefault(services =>
                       services.ServiceType == typeof(DbContextOptions<HotDeskDbContext>));
                   services.Remove(dbContextOptions);
                   services.AddSingleton<IPolicyEvaluator, FakePolicyEvaluator>();
                   services.AddMvc(option => option.Filters.Add(new FakeUserFilter()));

                   services.AddDbContext<HotDeskDbContext>(options => options.UseInMemoryDatabase("HotDeskDb"));
               });
           })
                .CreateClient();
        }


        [Theory]
        [InlineData("A12")]
        [InlineData("B311")]
        public async Task GetAllDesks_WithLocationParameter_ReturnsOkResult(string location)
        {

            var response = await _client.GetAsync("api/desk?" + location);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        }

        [Fact]
        public async Task AddDesk_WithValidModel_ReturnsCreatedStatus()
        {

            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("DeskNumber", "1"),
                new KeyValuePair<string, string>("Description", "Desk with ethernet cable"),
                new KeyValuePair<string, string>("LocationName", "A12")
            });

            
            var response = await _client.PostAsync("/api/desk", formContent);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Created);
        }


        [Fact]
        public async Task AddDesk_WithInvalidModel_ReturnsCreatedStatus()
        {

            var formContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("DeskNumber", "-3"),
                new KeyValuePair<string, string>("Description", " "),
                new KeyValuePair<string, string>("LocationName", "A12222")
            });


            var response = await _client.PostAsync("/api/desk", formContent);

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        }


    }
}
