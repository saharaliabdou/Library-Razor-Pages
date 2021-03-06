using AgilSystemutveckling_Xamarin_Net5.Models;
using AgilSystemutveckling_Xamarin_Net5.Service.GetService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AgilSystemutveckling_Xamarin_Net5.Pages.Event
{
    public class EventPageModel : PageModel
    {
        public static List<Products?> EventsList;
        [BindProperty]
        public Products NewEvent { get; set; }

        // Gets everything for the category "Event"
        public void OnGet()
        {
            EventsList = Get.GetAllByCategory("Event");           
        }

        public void OnPost()
        {
            ViewData["Books"] = EventsList;
            // git test
        }

        // For adding new event
        public IActionResult OnPostAdd()
        {

            NewEvent.AuthorName = " ";
            NewEvent.ImgUrl = " ";
            NewEvent.CategoryName = "Event";
            NewEvent.SubCategoryName = "Event";
            NewEvent.UnitsInStock = 0;
            Service.CreateService.Create.AddProduct(NewEvent);
            return RedirectToPage("/Event/EventPage");
        }

        // Delete an event
        public IActionResult OnPostDelete(int id)
        {
            Service.DeleteService.Delete.DeleteProduct(id);
            TempData["success"] = "Event deleted successfully"; // Popup
            return RedirectToPage("/Event/EventPage");

        }
    }
}
   
