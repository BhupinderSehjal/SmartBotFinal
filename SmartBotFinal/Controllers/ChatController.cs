using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartBotFinal.Models;
using System;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace SmartBotFinal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly HttpClient _httpClient;
        private readonly string _grokApiKey;

        public ChatController(ApplicationDbContext context, IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _context = context;
            _httpClient = httpClientFactory.CreateClient();
            _grokApiKey = configuration["GrokApiKey"];
        }

        public IActionResult Index()
        {
            return Ok(new { message = "Index endpoint is not implemented." });
            // Remove the duplicate definition of ChatRequest from this file
            // The class ChatRequest is already defined elsewhere in the namespace
            // No changes are needed to the rest of the file
        }
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ChatRequest request)
        {
            if (string.IsNullOrEmpty(request?.Message))
            {
                return BadRequest(new { error = "Message cannot be empty" });
            }

            // Detect intent using Grok API
            var intent = await DetectIntentAsync(request.Message);

            // Generate response based on intent
            string response = await GenerateResponseAsync(request.Message, intent, request.UserId);

            // Save the chat message to the database
            var chatbotMessage = new ChatbotMessage
            {
                UserId = request.UserId,
                MessageText = request.Message,
                Response = response,
                Timestamp = DateTime.UtcNow,
                Intent = intent
            };
            _context.ChatbotMessages.Add(chatbotMessage);
            await _context.SaveChangesAsync();

            return Ok(new { response, intent = intent.ToString() });
        }

        private async Task<ChatbotIntent> DetectIntentAsync(string message)
        {
            try
            {
                // Prepare Grok API request
                var requestBody = new
                {
                    prompt = $"Classify the intent of the following user query into one of these categories: Admissions, Courses, Fees, Events, ContactSupport, GeneralInquiry. Query: {message}",
                    model = "grok-3"
                };

                var requestContent = new StringContent(
                    JsonSerializer.Serialize(requestBody),
                    System.Text.Encoding.UTF8,
                    "application/json"
                );

                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _grokApiKey);

                // Call Grok API
                var response = await _httpClient.PostAsync("https://api.x.ai/grok", requestContent);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Grok API error: {response.StatusCode}");
                    return ChatbotIntent.GeneralInquiry;
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                var grokResponse = JsonSerializer.Deserialize<GrokResponse>(responseContent);

                // Map Grok response to ChatbotIntent
                return grokResponse.Intent?.ToLower() switch
                {
                    "admissions" => ChatbotIntent.Admissions,
                    "courses" => ChatbotIntent.Courses,
                    "fees" => ChatbotIntent.Fees,
                    "events" => ChatbotIntent.Events,
                    "contactsupport" => ChatbotIntent.ContactSupport,
                    _ => ChatbotIntent.GeneralInquiry
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calling Grok API: {ex.Message}");
                return ChatbotIntent.GeneralInquiry;
            }
        }

        private async Task<string> GenerateResponseAsync(string message, ChatbotIntent intent, string userId)
        {
            message = message.ToLower();

            switch (intent)
            {
                case ChatbotIntent.Admissions:
                    if (message.Contains("application status"))
                    {
                        return "Your application status is: Admitted. Congratulations! Your acceptance package was sent on August 15, 2025.";
                    }
                    if (message.Contains("admission requirements"))
                    {
                        var admission = await _context.Admissions.FirstOrDefaultAsync();
                        if (admission != null)
                        {
                            return $"Admission requirements for {admission.ProgramName}: {admission.Requirements}. Apply at: {admission.ApplicationLink}. Contact: {admission.ContactEmail}.";
                        }
                    }
                    if (message.Contains("application deadlines"))
                    {
                        var admission = await _context.Admissions
                            .OrderBy(a => a.ApplicationDeadline)
                            .FirstOrDefaultAsync();
                        if (admission != null)
                        {
                            return $"The next application deadline for {admission.ProgramName} is {admission.ApplicationDeadline:MMM dd, yyyy}.";
                        }
                    }
                    var faqAdmissions = await _context.FAQs
                        .Where(f => f.Category == FAQCategory.Admissions && f.Keywords.ToLower().Contains(message))
                        .FirstOrDefaultAsync();
                    return faqAdmissions?.Answer ?? "I couldn't find specific admission details. Please provide more details or contact admissions@university.edu.";

                case ChatbotIntent.Courses:
                    if (message.Contains("class schedule"))
                    {
                        var course = await _context.Courses.FirstOrDefaultAsync();
                        if (course != null)
                        {
                            return $"Sample class schedule: {course.CourseCode} - {course.Title} (Mon/Wed 9:00 AM). Check the student portal for your full schedule.";
                        }
                    }
                    if (message.Contains("view grades"))
                    {
                        return "Your current grades: CS101: A-, MATH202: B+. Check the student portal for a full breakdown.";
                    }
                    if (message.Contains("course registration"))
                    {
                        return "Course registration for Spring 2026 begins on November 15, 2025. Register via the student portal.";
                    }
                    if (message.Contains("exam schedule"))
                    {
                        return "Final exams for Fall 2025: CS101 (Dec 12, 9:00 AM, Room H201), MATH202 (Dec 14, 1:00 PM, Room S105). Check the portal for details.";
                    }
                    var courseMatch = await _context.Courses
                        .Where(c => c.Title.ToLower().Contains(message) || c.CourseCode.ToLower().Contains(message))
                        .FirstOrDefaultAsync();
                    if (courseMatch != null)
                    {
                        return $"Course: {courseMatch.CourseCode} - {courseMatch.Title}. Description: {courseMatch.Description}. Credits: {courseMatch.Credits}. Prerequisites: {courseMatch.Prerequisites ?? "None"}.";
                    }
                    return "I couldn't find that course. Try specifying the course code or title.";

                case ChatbotIntent.Fees:
                    if (message.Contains("financial aid"))
                    {
                        var faqFees = await _context.FAQs
                            .Where(f => f.Category == FAQCategory.FinancialAid)
                            .FirstOrDefaultAsync();
                        return faqFees?.Answer ?? "Financial aid details are available at the student portal. Submit your FAFSA by January 15.";
                    }
                    return "I couldn't find fee details. Please check the Financial Aid FAQ or contact financialaid@university.edu.";

                case ChatbotIntent.Events:
                    if (message.Contains("events"))
                    {
                        var upcomingEvent = await _context.Events
                            .Where(e => e.Date >= DateTime.Today)
                            .OrderBy(e => e.Date)
                            .FirstOrDefaultAsync();
                        if (upcomingEvent != null)
                        {
                            return $"Upcoming Event: {upcomingEvent.Title} on {upcomingEvent.Date:MMM dd, yyyy} at {upcomingEvent.Location}. Details: {upcomingEvent.Description}. Register: {upcomingEvent.RegistrationLink}.";
                        }
                    }
                    return "No upcoming events found. Check the events page for more info.";

                case ChatbotIntent.ContactSupport:
                    var faqSupport = await _context.FAQs
                        .Where(f => f.Category == FAQCategory.TechnicalSupport || f.Category == FAQCategory.GeneralInquiry)
                        .FirstOrDefaultAsync();
                    return faqSupport?.Answer ?? "For support, email support@university.edu or call (123) 456-7890.";

                case ChatbotIntent.GeneralInquiry:
                default:
                    if (message.Contains("campus map") || message.Contains("facilities"))
                    {
                        var location = await _context.CampusLocations
                            .Where(l => l.Description.ToLower().Contains("library") || l.BuildingName.ToLower().Contains(message))
                            .FirstOrDefaultAsync();
                        if (location != null)
                        {
                            return $"The {location.BuildingName} ({location.Description}) is located at {location.RoomNumber}. Map: {location.MapImageUrl}.";
                        }
                    }
                    if (message.Contains("housing"))
                    {
                        var faqHousing = await _context.FAQs
                            .Where(f => f.Category == FAQCategory.Housing)
                            .FirstOrDefaultAsync();
                        return faqHousing?.Answer ?? "Housing options include dormitories and apartments. Apply at housing.university.edu.";
                    }
                    if (message.Contains("feedback"))
                    {
                        var rating = message.Contains("5 stars") ? 5 : (message.Contains("4 stars") ? 4 : 3);
                        var feedback = new Feedback
                        {
                            Name = userId,
                            Message = message,
                            Rating = rating,
                            Timestamp = DateTime.UtcNow
                        };
                        _context.Feedbacks.Add(feedback);
                        await _context.SaveChangesAsync();
                        return "Thank you for your feedback!";
                    }
                    var faqGeneral = await _context.FAQs
                        .Where(f => f.Category == FAQCategory.GeneralInquiry && f.Keywords.ToLower().Contains(message))
                        .FirstOrDefaultAsync();
                    return faqGeneral?.Answer ?? "I'm not sure how to help with that. Could you clarify or try a quick action?";
            }
        }
    }

    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            if (request.Email.Contains("@") && request.Password.Length >= 4)
            {
                return Ok(new { name = request.Email.Split('@')[0] });
            }
            return Unauthorized(new { error = "Invalid credentials" });
        }

        [HttpPost("verify")]
        public IActionResult Verify([FromBody] VerifyRequest request)
        {
            bool isValid = request.Code.Length == 6 && request.Email != null;
            return Ok(new { isValid });
        }

        [HttpPost("resend-otp")]
        public IActionResult ResendOtp([FromBody] ResendOtpRequest request)
        {
            if (request.Email != null)
            {
                return Ok(new { message = "OTP resent" });
            }
            return BadRequest(new { error = "Invalid email" });
        }
    }

    

    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class VerifyRequest
    {
        public string Email { get; set; }
        public string Code { get; set; }
    }

    public class ResendOtpRequest
    {
        public string Email { get; set; }
    }

    public class GrokResponse
    {
        public string Intent { get; set; }
    }
}