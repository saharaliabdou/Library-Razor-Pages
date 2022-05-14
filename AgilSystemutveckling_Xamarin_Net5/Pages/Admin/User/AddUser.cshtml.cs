using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AgilSystemutveckling_Xamarin_Net5.Pages.Admin.User
{
    public class AddUserModel : PageModel
    {
        [BindProperty]
        public static TestModels.User user { get; set; }
        public ActionResult OnPost()
        {
            if(!ModelState.IsValid)
            {
                return Page();
            }
            //TestService.GetService.Get.AddUser(user);
            return RedirectToPage("/Admin/User/UserListPage");
        }
    }
}
