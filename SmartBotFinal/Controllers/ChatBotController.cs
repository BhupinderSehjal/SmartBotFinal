
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartBotFinal.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Linq;


namespace SmartBotFinal.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatBotController : Controller
    {
        private readonly ApplicationDbContext _context;
        public ChatBotController(ApplicationDbContext applicationDbContext)
        {
            _context = applicationDbContext;

            if (!_context.BotResponses.Any())
            {
                _context.BotResponses.AddRange(
                    new BotResponse { ActionId = "class-schedule", ResponseText = "Here is your class schedule: ..." },
                    new BotResponse { ActionId = "view-grades", ResponseText = "Here are your current grades: ..." },
                    new BotResponse { ActionId = "course-registration", ResponseText = "Course registration starts soon." },
                    new BotResponse { ActionId = "exam-schedule", ResponseText = "Your final exam schedule is: ..." },
                    new BotResponse { ActionId = "financial-aid", ResponseText = "You have a scholarship and federal grant." }
                );
                _context.SaveChanges(); // Save to DB


            }
        }


        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Responses()
        {
            var data = _context.BotResponses.ToDictionary(x => x.ActionId, x => x.ResponseText);
            return Json(data);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ChatRequest request)
        {
            if (string.IsNullOrEmpty(request?.Message))
            {
                return BadRequest(new { error = "Message cannot be empty" });
            }

            // Detect intent based on the message
            var intent = DetectIntent(request.Message);

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

            return Ok(new { response });
        }

        private ChatbotIntent DetectIntent(string message)
        {
            // Simple keyword-based intent detection (replace with NLP for production)
            message = message.ToLower();
            if (message.Contains("admission") || message.Contains("apply") || message.Contains("application"))
                return ChatbotIntent.Admissions;
            if (message.Contains("course") || message.Contains("class") || message.Contains("registration"))
                return ChatbotIntent.Courses;
            if (message.Contains("fee") || message.Contains("cost") || message.Contains("tuition") || message.Contains("financial aid"))
                return ChatbotIntent.Fees;
            if (message.Contains("event") || message.Contains("activity") || message.Contains("festival"))
                return ChatbotIntent.Events;
            if (message.Contains("contact") || message.Contains("support") || message.Contains("help"))
                return ChatbotIntent.ContactSupport;
            if (message.Contains("map") || message.Contains("location") || message.Contains("housing") || message.Contains("facilities"))
                return ChatbotIntent.GeneralInquiry; // Could add a Campus intent
            return ChatbotIntent.GeneralInquiry;
        }

        private async Task<string> GenerateResponseAsync(string message, ChatbotIntent intent, string userId)
        {
            message = message.ToLower();

            switch (intent)
            {
                case ChatbotIntent.Admissions:
                    // Handle quick actions like "application status" or "admission requirements"
                    if (message.Contains("application status"))
                    {
                        // Simulate user-specific data (replace with actual user data query)
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
                    if (message.Contains("class schedule") || message.Contains("course registration"))
                    {
                        var course = await _context.Courses.FirstOrDefaultAsync();
                        if (course != null)
                        {
                            return $"Course: {course.CourseCode} - {course.Title}. Description: {course.Description}. Credits: {course.Credits}. Prerequisites: {course.Prerequisites ?? "None"}.";
                        }
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
                    var faqGeneral = await _context.FAQs
                        .Where(f => f.Category == FAQCategory.GeneralInquiry && f.Keywords.ToLower().Contains(message))
                        .FirstOrDefaultAsync();
                    return faqGeneral?.Answer ?? "I'm not sure how to help with that. Could you clarify or try a quick action?";
            }
        }
    }

    public class ChatRequest
    {
        public string Message { get; set; }
        public string UserId { get; set; }
    }
}
    

