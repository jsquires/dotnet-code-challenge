
using System;
using System.Net;
using System.Net.Http;
using System.Text;
using CodeChallenge.Data;
using CodeChallenge.Models;

using CodeCodeChallenge.Tests.Integration.Extensions;
using CodeCodeChallenge.Tests.Integration.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CodeCodeChallenge.Tests.Integration
{
    [TestClass]
    public class EmployeeControllerTests
    {
        private static HttpClient _httpClient;
        private static TestServer _testServer;

        [ClassInitialize]
        // Attribute ClassInitialize requires this signature
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "<Pending>")]
        public static void InitializeClass(TestContext context)
        {
            _testServer = new TestServer();
            _httpClient = _testServer.NewClient();
        }

        [ClassCleanup]
        public static void CleanUpTest()
        {
            _httpClient.Dispose();
            _testServer.Dispose();
        }

        private static void InsertCompensation(Compensation compensation)
        {
            var serviceScopeFactory = (IServiceScopeFactory)_testServer.ApplicationFactory.Services.GetService(typeof(IServiceScopeFactory));
            using (var scope = serviceScopeFactory.CreateScope())
            {
                var services = scope.ServiceProvider;
                var dbContext = services.GetRequiredService<EmployeeContext>();
                dbContext.Compensation.Add(compensation);
                // Don't attempt to save the employee, since they already exist
                dbContext.Entry(compensation.Employee).State = EntityState.Unchanged;
                dbContext.SaveChanges();
            }
        }

        [TestMethod]
        public void CreateEmployee_Returns_Created()
        {
            // Arrange
            var employee = new Employee()
            {
                Department = "Complaints",
                FirstName = "Debbie",
                LastName = "Downer",
                Position = "Receiver",
            };

            var requestContent = new JsonSerialization().ToJson(employee);

            // Execute
            var postRequestTask = _httpClient.PostAsync("api/employee",
               new StringContent(requestContent, Encoding.UTF8, "application/json"));
            var response = postRequestTask.Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

            var newEmployee = response.DeserializeContent<Employee>();
            Assert.IsNotNull(newEmployee.EmployeeId);
            Assert.AreEqual(employee.FirstName, newEmployee.FirstName);
            Assert.AreEqual(employee.LastName, newEmployee.LastName);
            Assert.AreEqual(employee.Department, newEmployee.Department);
            Assert.AreEqual(employee.Position, newEmployee.Position);
        }

        [TestMethod]
        public void GetEmployeeById_Returns_Ok()
        {
            // Arrange
            var employeeId = "16a596ae-edd3-4847-99fe-c4518e82c86f";
            var expectedFirstName = "John";
            var expectedLastName = "Lennon";

            // Execute
            var getRequestTask = _httpClient.GetAsync($"api/employee/{employeeId}");
            var response = getRequestTask.Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var employee = response.DeserializeContent<Employee>();
            Assert.AreEqual(expectedFirstName, employee.FirstName);
            Assert.AreEqual(expectedLastName, employee.LastName);
        }

        [TestMethod]
        public void UpdateEmployee_Returns_Ok()
        {
            // Arrange
            var employee = new Employee()
            {
                EmployeeId = "03aa1462-ffa9-4978-901b-7c001562cf6f",
                Department = "Engineering",
                FirstName = "Pete",
                LastName = "Best",
                Position = "Developer VI",
            };
            var requestContent = new JsonSerialization().ToJson(employee);

            // Execute
            var putRequestTask = _httpClient.PutAsync($"api/employee/{employee.EmployeeId}",
               new StringContent(requestContent, Encoding.UTF8, "application/json"));
            var putResponse = putRequestTask.Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, putResponse.StatusCode);
            var newEmployee = putResponse.DeserializeContent<Employee>();

            Assert.AreEqual(employee.FirstName, newEmployee.FirstName);
            Assert.AreEqual(employee.LastName, newEmployee.LastName);
        }

        [TestMethod]
        public void UpdateEmployee_Returns_NotFound()
        {
            // Arrange
            var employee = new Employee()
            {
                EmployeeId = "Invalid_Id",
                Department = "Music",
                FirstName = "Sunny",
                LastName = "Bono",
                Position = "Singer/Song Writer",
            };
            var requestContent = new JsonSerialization().ToJson(employee);

            // Execute
            var postRequestTask = _httpClient.PutAsync($"api/employee/{employee.EmployeeId}",
               new StringContent(requestContent, Encoding.UTF8, "application/json"));
            var response = postRequestTask.Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public void GetReportingStructureByEmployeeId_WithReports_Returns_Ok()
        {
            // Arrange
            var lennonEmployeeId = "16a596ae-edd3-4847-99fe-c4518e82c86f";
            int lennonExpectedReports = 4;

            // Execute
            var getRequestTask = _httpClient.GetAsync($"api/employee/{lennonEmployeeId}/reporting");
            var response = getRequestTask.Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var reporting = response.DeserializeContent<ReportingStructure>();
            Assert.AreEqual(lennonExpectedReports, reporting.NumberOfReports);
            Assert.AreEqual(lennonEmployeeId, reporting.Employee.EmployeeId);
        }


        [TestMethod]
        public void GetReportingStructureByEmployeeId_WithoutReports_Returns_Ok()
        {
            // Arrange
            var harrisonEmployeeId = "c0c2293d-16bd-4603-8e08-638a9d18b22c";
            int harrisonExpectedReports = 0;

            // Execute
            var getRequestTask = _httpClient.GetAsync($"api/employee/{harrisonEmployeeId}/reporting");
            var response = getRequestTask.Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var reporting = response.DeserializeContent<ReportingStructure>();
            Assert.AreEqual(harrisonExpectedReports, reporting.NumberOfReports);
            Assert.AreEqual(harrisonEmployeeId, reporting.Employee.EmployeeId);
        }

        [TestMethod]
        public void GetCompensationByEmployeeId_Returns_Ok()
        {
            // Arrange
            var lennonEmployeeId = "16a596ae-edd3-4847-99fe-c4518e82c86f";            
            var compensation = new Compensation
            {
                Id = "1",
                Salary = 80000,
                EffectiveDate = DateTime.Now,
                Employee = new Employee
                {
                    EmployeeId = lennonEmployeeId,
                }
            };
            InsertCompensation(compensation);

            // Execute
            var getRequestTask = _httpClient.GetAsync($"api/employee/{lennonEmployeeId}/compensation");
            var response = getRequestTask.Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var newCompensation = response.DeserializeContent<Compensation>();
            Assert.AreEqual(compensation.Salary, newCompensation.Salary);
            Assert.AreEqual(compensation.EffectiveDate, newCompensation.EffectiveDate);
            Assert.AreEqual(lennonEmployeeId, newCompensation.Employee.EmployeeId);
        }

        [TestMethod]
        public void CreateCompensation_Returns_Created()
        {
            // Arrange
            var harrisonEmployeeId = "c0c2293d-16bd-4603-8e08-638a9d18b22c";

            var compensation = new Compensation()
            {
                Salary = 100000,
                EffectiveDate = DateTime.Now,
            };

            var requestContent = new JsonSerialization().ToJson(compensation);

            // Execute
            var postRequestTask = _httpClient.PostAsync($"api/employee/{harrisonEmployeeId}/compensation",
               new StringContent(requestContent, Encoding.UTF8, "application/json"));
            var response = postRequestTask.Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.Created, response.StatusCode);

            var newCompensation = response.DeserializeContent<Compensation>();
            Assert.IsNotNull(newCompensation.Id);
            Assert.AreEqual(compensation.Salary, newCompensation.Salary);
            Assert.AreEqual(compensation.EffectiveDate, newCompensation.EffectiveDate);
            Assert.AreEqual(harrisonEmployeeId, newCompensation.Employee.EmployeeId);
        }

        [TestMethod]
        public void CreateCompensation_Returns_NotFound()
        {
            // Arrange
            var invalidEmployeeId = "Invalid_Id";

            var compensation = new Compensation()
            {
                Salary = 100000,
                EffectiveDate = DateTime.Now,
            };

            var requestContent = new JsonSerialization().ToJson(compensation);

            // Execute
            var postRequestTask = _httpClient.PostAsync($"api/employee/{invalidEmployeeId}/compensation",
               new StringContent(requestContent, Encoding.UTF8, "application/json"));
            var response = postRequestTask.Result;

            // Assert
            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }


    }
}
