using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AgilSystemutveckling_Xamarin_Net5.Pages
{
    public class DescriptionPageModel : PageModel
    {
        public static Models.Products product = new Models.Products();
        public static string description;
        public static string title;
        public static string author;
        public static string imgUrl;
        public static int unitsinstock;
        public static int productId;

        // Save product information into props 
        // To use in front-end page
        public void OnGet(int id)
        {
            product = Service.GetService.Get.GetProductById(id);
            description = product.Description;
            title = product.Title;
            author = product.AuthorName;
            imgUrl = product.ImgUrl;
            unitsinstock = product.UnitsInStock;
            productId = id;
        }

        public void OnPost()
        {
            ViewData["Books"] = description;
            // git test
        }
        
        // Adds the chosen product to cart
        public void OnPostAddToCart()
        { 
            Globals.CartList.Add(product);
            TempData["success"] = product.Title + " has been added";
        }

    }
}
