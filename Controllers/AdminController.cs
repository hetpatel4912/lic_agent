using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace LIC.Controllers
{
    [Route("[controller]")]
    public class AdminController : Controller
    {
        private readonly ILogger<AdminController> _logger;
        private readonly NpgsqlConnection _conn;
        // private static readonly List<PolicyCard> Policies = new()
        // {
        //     new PolicyCard(1, "LIC-45892136", "8490943582", "Amit Patel", new DateTime(2026, 7, 15), 12500),
        //     new PolicyCard(2, "LIC-78541209", "9825012345", "Rina Shah", new DateTime(2026, 8, 3), 8400),
        //     new PolicyCard(3, "LIC-33218745", "7600011223", "Mahesh Desai", new DateTime(2026, 8, 21), 15600)
        // };

        public AdminController(ILogger<AdminController> logger, NpgsqlConnection conn)
        {
            _logger = logger;
            _conn = conn;
        }

        [HttpGet("Index")]
        public IActionResult Index()
        {
            if(HttpContext.Session.GetString("username") != "amit")
            {
                return RedirectToAction("Index","Home");
            }
            return View();
        }

        [HttpGet("Login")]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost("Login")]
        public IActionResult Login([FromForm] AdminLoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { success = false, message = "Username and password are required." });
            }

            if(request.Username == "amit" && request.Password == "licagent")
            {

                HttpContext.Session.SetString("username", "amit");
                return Json(new { success = true, message = "Login successful." });
            }

            return BadRequest();

        }

        [HttpGet("Policies")]
        public IActionResult PoliciesList()
        {
            string query = "select * from t_policies;";
            int status = 0;
            List<PolicyCard> policies = new List<PolicyCard>();
            try
            {
                _conn.Open();
                NpgsqlCommand com = new NpgsqlCommand(query, _conn);
                var reader = com.ExecuteReader();
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        PolicyCard policy = new PolicyCard(
                            Convert.ToInt32(reader["c_id"]),
                            reader["c_policyno"].ToString(),
                            reader["c_mobile"].ToString(),
                            reader["c_name"].ToString(),
                            (DateOnly)reader["c_premiumdate"],
                            Convert.ToDecimal(reader["c_premiumamount"])
                        );
                        
                        policies.Add(policy);
                    }
                }
                _conn.Close();
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error in message :- " + ex);
            }
            finally
            {
                _conn.Close();
            }

            // if(status != 0)
            // {
            //     return Json(new { success = true, message = "Policy card added successfully." });
            // }
            return Json(policies);

            // return Json(Policies.OrderBy(policy => policy.PremiumDate));
        }

        [HttpPost("Policies")]
        public async Task<IActionResult> AddPolicy([FromForm] PolicyCardRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.PolicyNumber) ||
                string.IsNullOrWhiteSpace(request.MobileNumber) ||
                string.IsNullOrWhiteSpace(request.PolicyHolderName) ||
                request.PremiumDate == default ||
                request.PremiumAmount <= 0)
            {
                return BadRequest(new { success = false, message = "Please enter all policy details." });
            }

            // var policy = new PolicyCard(
            //     Policies.Count == 0 ? 1 : Policies.Max(item => item.Id) + 1,
            //     request.PolicyNumber,
            //     request.MobileNumber,
            //     request.PolicyHolderName,
            //     request.PremiumDate,
            //     request.PremiumAmount);

            // Policies.Add(policy);

            string query = "insert into t_policies(c_policyno,c_name,c_mobile,c_premiumdate,c_premiumamount)values(@policyno,@name,@mobile,@date,@amount)";
            int status = 0;
            try
            {
                _conn.Open();
                NpgsqlCommand com = new NpgsqlCommand(query, _conn);
                com.Parameters.AddWithValue("policyno", request.PolicyNumber);
                com.Parameters.AddWithValue("name", request.PolicyHolderName);
                com.Parameters.AddWithValue("mobile", request.MobileNumber);
                com.Parameters.AddWithValue("date", request.PremiumDate);
                com.Parameters.AddWithValue("amount", request.PremiumAmount);
                status = com.ExecuteNonQuery();
                _conn.Close();
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error in message :- " + ex);
            }
            finally
            {
                _conn.Close();
            }

            if(status != 0)
            {
                return Json(new { success = true, message = "Policy card added successfully." });
            }
            return BadRequest();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View("Error!");
        }

        public sealed record AdminLoginRequest(string Username, string Password);

        public sealed record PolicyCard(
            int Id,
            string PolicyNumber,
            string MobileNumber,
            string PolicyHolderName,
            DateOnly PremiumDate,
            decimal PremiumAmount);

        public sealed record PolicyCardRequest(
            string PolicyNumber,
            string MobileNumber,
            string PolicyHolderName,
            DateTime PremiumDate,
            decimal PremiumAmount);
    }
}
