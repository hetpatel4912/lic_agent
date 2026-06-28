using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using LIC.Models;
using Npgsql;

namespace LIC.Controllers;

public class HomeController : Controller
{

    private readonly NpgsqlConnection _conn;
        
    public HomeController(NpgsqlConnection conn)
    {
        _conn = conn;
    }

    public IActionResult Index()
    {
        return View();
    }



    [HttpGet("getmypolicies")]
    public IActionResult getmypolicies()
    {
        return View();
    }

    [HttpGet("getmypolicies/search")]
    public IActionResult SearchPolicies([FromQuery] string mobile)
    {
        if (string.IsNullOrWhiteSpace(mobile))
        {
            return BadRequest(new { success = false, message = "Please enter your mobile number." });
        }

        string query = "select * from t_policies where c_mobile = @mobile;";
            List<PolicyCard> policies = new List<PolicyCard>();
            try
            {
                _conn.Open();
                NpgsqlCommand com = new NpgsqlCommand(query, _conn);
                com.Parameters.AddWithValue("mobile", mobile.Trim());
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

            return Json(policies);
    }

    public sealed record PolicyCard(
            int Id,
            string PolicyNumber,
            string MobileNumber,
            string PolicyHolderName,
            DateOnly PremiumDate,
            decimal PremiumAmount);




    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
