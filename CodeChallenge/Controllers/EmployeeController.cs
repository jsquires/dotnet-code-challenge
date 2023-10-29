using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using CodeChallenge.Services;
using CodeChallenge.Models;

namespace CodeChallenge.Controllers
{
    [ApiController]
    [Route("api/employee")]
    public class EmployeeController : ControllerBase
    {
        private readonly ILogger _logger;
        private readonly IEmployeeService _employeeService;

        public EmployeeController(ILogger<EmployeeController> logger, IEmployeeService employeeService)
        {
            _logger = logger;
            _employeeService = employeeService;
        }

        [HttpPost]
        public IActionResult CreateEmployee([FromBody] Employee employee)
        {
            _logger.LogDebug($"Received employee create request for '{employee.FirstName} {employee.LastName}'");

            _employeeService.Create(employee);

            return CreatedAtRoute("getEmployeeById", new { id = employee.EmployeeId }, employee);
        }

        [HttpGet("{id}", Name = "getEmployeeById")]
        public IActionResult GetEmployeeById(String id)
        {
            _logger.LogDebug($"Received employee get request for '{id}'");

            var employee = _employeeService.GetById(id);

            if (employee == null)
                return NotFound();

            return Ok(employee);
        }

        [HttpPut("{id}")]
        public IActionResult ReplaceEmployee(String id, [FromBody] Employee newEmployee)
        {
            _logger.LogDebug($"Recieved employee update request for '{id}'");

            var existingEmployee = _employeeService.GetById(id);
            if (existingEmployee == null)
                return NotFound();

            _employeeService.Replace(existingEmployee, newEmployee);

            return Ok(newEmployee);
        }

        [HttpGet("{id}/reporting", Name = "getReportingStructureByEmployeeId")]
        public IActionResult GetReportingStructureByEmployeeId(String id)
        {
            _logger.LogDebug($"Received reporting structure get request for employee '{id}'");

            var employee = _employeeService.GetById(id);

            if (employee == null)
                return NotFound();

            int numReports = 0;
            var reports = new Queue<Employee>(employee.DirectReports);
            while (reports.Count > 0)
            {
                var emp = reports.Dequeue();
                numReports++;
                if (emp.DirectReports != null)
                {
                    emp.DirectReports.ForEach(report => reports.Enqueue(report));
                }
            }

            var reportingStructure = new ReportingStructure { Employee = employee, NumberOfReports = numReports };
            return Ok(reportingStructure);
        }

        [HttpGet("{id}/compensation", Name = "getCompensationByEmployeeId")]
        public IActionResult GetCompensationByEmployeeId(String id)
        {
            _logger.LogDebug($"Received compensation get request for employee '{id}'");

            var compensation = _employeeService.GetCompensationByEmployeeId(id);
            if (compensation == null)
                return NotFound();

            return Ok(compensation);
        }

        [HttpPost("{id}/compensation")]
        public IActionResult CreateCompensation(String id, [FromBody] Compensation compensation)
        {
            _logger.LogDebug($"Received compensation create request for employee '{id}: {compensation.Salary}, {compensation.EffectiveDate}'");

            var employee = _employeeService.GetById(id);
            if (employee == null)
                return NotFound();

            // Only allow one compensation per employee. In a production app, we might implement a PUT endpoint for updating it.
            var existingCompensation = _employeeService.GetCompensationByEmployeeId(id);
            if (existingCompensation != null)
                return BadRequest();

            compensation.Employee = employee;

            _employeeService.CreateCompensation(compensation);
            return CreatedAtRoute("getCompensationByEmployeeId", new { id }, compensation);
        }

    }
}
