
using AgilSystemutveckling_Xamarin_Net5.Models;
using Dapper;
using MySqlConnector;
using System.Data;

using static AgilSystemutveckling_Xamarin_Net5.Constants.Constant;
using static AgilSystemutveckling_Xamarin_Net5.Methods.Methods;
using static AgilSystemutveckling_Xamarin_Net5.Service.GetService.Get;
using static AgilSystemutveckling_Xamarin_Net5.Service.UpdateService.Update;

namespace AgilSystemutveckling_Xamarin_Net5.Service.CreateService
{
    // Static class to prevent instantiations of the class - it is only used to provide methods.
    public static class Create
    {
        #region User related
        /// <summary>
        /// Adds a user to the database.
        /// </summary>
        /// <param name="user"></param>
        /// <exception cref="Exception"></exception>
        public static void AddUser(Users? user)
        {
            // Null-checks running inline to prevent nullable reference warnings throughout code.
            if (user is null) { throw new ArgumentNullException(nameof(user)); }

            // Calls method that checks if passed in variable properties are null, correctly formatted for SQL or short enough.
            CheckStringFormat(user.FirstName, user.LastName);

            // Declaration of int variables for name ID's.
            int firstNameId = 0;
            int lastNameId = 0;
            int fullNameId = 0;

            // Declaration of bool variables.
            bool firstNameExists = false;
            bool lastNameExists = false;

            // Get the full list of first names to compare with the newly entered one from Front end.
            var firstNames = GetAllFirstNames();

            // Check if list is null.
            if (firstNames is null) { throw new NullReferenceException(nameof(firstNames)); }

            // For each first name, check if it is null and if it already exists in database.
            foreach (var item in firstNames)
            {
                if (item is not null && user.FirstName == item.FirstName)
                {
                    // Set the firstNameId to be the same as item ID, then break to the next statement.
                    firstNameId = item.Id;

                    // Switch firstNameExists to true.
                    firstNameExists = true;
                    // Break to the next statement.
                    break;
                }
            }

            // If first name did not exist in the database, go here and add it using INSERT.
            if (firstNameExists == false)
            {

                var sql = @$"INSERT INTO FirstNames (FirstName)
                                    VALUES ('{user.FirstName}')";

                // Using MySQLConnection with constant connectionstring stored in Constants folder.
                using (var connection = new MySqlConnection(ConnectionString))
                {
                    // Open MySQL connection.
                    connection.Open();
                    // Check whether connection state is set to open.
                    if (connection.State == ConnectionState.Open)
                        connection.Execute(sql); // Execute SQL query.

                    // Close connection before exiting block.
                    connection.Close();
                }
                // Query to get the Id of the first name of the current user.
                var sql2 = @$"SELECT Id
                                    FROM FirstNames
                                    WHERE FirstName = '{user.FirstName}'";

                using (var connection = new MySqlConnection(ConnectionString))
                {
                    connection.Open();
                    if (connection.State == ConnectionState.Open)
                        firstNameId = connection.QuerySingle<int>(sql2); // Execute single-row query, adding first name ID from database to an int variable.


                    connection.Close();
                }
            }

            // Get all last names from database.
            var lastNames = GetAllLastNames();
            if (lastNames is null) { throw new NullReferenceException(nameof(lastNames)); }

            // For each last name, check if it is null, and compare the user last name to the last names already present in database.
            foreach (var item in lastNames)
            {
                if (item is not null && user.LastName == item.LastName)
                {
                    // Set current last name ID to the existing last name's ID, then exit to the next statement, avoiding duplicates in database.
                    lastNameId = item.Id;
                    lastNameExists = true;
                    break;
                }
            }

            // If last name was not matched, add it to the database using INSERT.
            if (lastNameExists == false)
            {
                var sql = @$"INSERT INTO LastNames (LastName) 
                                    VALUES ('{user.LastName}')";


                using (var connection = new MySqlConnection(ConnectionString))
                {
                    connection.Open();
                    if (connection.State == ConnectionState.Open)
                        connection.Execute(sql);

                    connection.Close();
                }

                var sql2 = @$"SELECT Id
                                    FROM LastNames
                                    WHERE LastName = '{user.LastName}'";

                using (var connection = new MySqlConnection(ConnectionString))
                {
                    connection.Open();
                    if (connection.State == ConnectionState.Open)
                        lastNameId = connection.QuerySingle<int>(sql2); // Execute single row query adding last name to an int variable.

                    connection.Close();
                }
            }

            // Query for adding a new FullNames object using first name and last name ID's set by querysingle method.
            var sqlFN = @$"INSERT INTO FullNames (FirstNameId, LastNameId)
                                  VALUES ({firstNameId}, {lastNameId})";

            using (var connection = new MySqlConnection(ConnectionString))
            {
                connection.Open();
                if (connection.State == ConnectionState.Open)
                    connection.Execute(sqlFN); // Add new fullnames with execute method.

                connection.Close();
            }

            var sqlFN2 = @$"SELECT Id
                                  FROM FullNames
                                  WHERE LastNameId = {lastNameId} AND FirstNameId = {firstNameId}";

            using (var connection = new MySqlConnection(ConnectionString))
            {
                connection.Open();
                if (connection.State == ConnectionState.Open)
                    fullNameId = connection.QuerySingle<int>(sqlFN2);

                connection.Close();
            }

            // Perform format check for user string properties.
            CheckStringFormat(user.Username, user.Password, user.Address);

            // Query for adding a user.
            var sqlMain = @$"INSERT INTO Users (FullNameId, Username, Password, AccessId, Address, Blocked) 
                                    VALUES ({fullNameId}, '{user.Username}', '{user.Password}', {user.Level}, '{user.Address}', {user.Blocked})";

            using (var connection = new MySqlConnection(ConnectionString))
            {
                connection.Open();
                if (connection.State == ConnectionState.Open)
                    try { connection.Execute(sqlMain); }
                    catch (Exception e) { throw new Exception("Could not add user.", e); } // Throws new exception if user could not be added.

                connection.Close();
            }
        }

        #endregion

        #region Product related
        /// <summary>
        /// Adds a product to the library database.
        /// </summary>
        /// <param name="product"></param>
        public static void AddProduct(Products? product)
        {
            // Null-check to avoid nullable reference warnings.
            if (product is null) throw new ArgumentNullException(nameof(product));

            // Check format of strings to avoid database errors.
            CheckStringFormat(product.Description, product.CategoryName, product.SubCategoryName);

            // Declare int and bool variables.
            int AuthorId = 0;
            int CategoryId = 0;
            int SubCategoryId = 0;

            bool AuthorExists = false;
            bool CategoryExists = false;
            bool SubCategoryExists = false;

            // Populate a new list of authors with authors in database using GetAllAuthors method.
            List<Authors?>? authors = GetAllAuthors();

            // Check if list was populated.
            if (authors is null) { throw new ArgumentNullException(nameof(authors)); }

            // Iterate through authors list to see if there is an author with the same name.
            foreach (var author in authors)
            {
                // Null-check to avoid .NET 6 null reference warnings.
                if (author is null) { throw new NullReferenceException(nameof(author)); }

                // When the id matches, enter if clause.
                if (author.AuthorName == product.AuthorName)
                {
                    // Set the authorid int variable equal to the author.id from database.
                    AuthorId = author.Id;

                    // The author already exists, so break out to the next statement.
                    AuthorExists = true;
                    break;
                }
            }
            // Go here if no match was found for authorname.
            if (AuthorExists == false)
            {
                // Add the author using INSERT.
                var sql = @$"INSERT INTO Authors (AuthorName) 
                                    VALUES ('{product.AuthorName}')";

                using (var connection = new MySqlConnection(ConnectionString))
                {
                    connection.Open();
                    if (connection.State == ConnectionState.Open)
                        connection.Execute(sql);

                    connection.Close();
                }

                // Select the author ID that corresponds to the product's AuthorName.
                var sql2 = @$"SELECT Id
                                    FROM Authors
                                    WHERE AuthorName = '{product.AuthorName}'";

                using (var connection = new MySqlConnection(ConnectionString))
                {
                    connection.Open();
                    if (connection.State == ConnectionState.Open)
                        AuthorId = connection.QuerySingle<int>(sql2);

                    connection.Close();
                }
            }

            /* The following code mostly repeats the above code. */

            List<Categories?>? categories = GetAllCategories();

            if (categories is null) { throw new ArgumentNullException(nameof(categories)); }

            foreach (Categories? category in categories)
            {
                if (category is null) { throw new NullReferenceException(nameof(category)); }

                if (category.CategoryName == product.CategoryName)
                {
                    CategoryId = category.Id;
                    CategoryExists = true;
                    break;
                }
            }

            if (CategoryExists == false)
            {
                var sql = @$"INSERT INTO Categories (CategoryName) 
                                    VALUES ('{product.CategoryName}')";

                using (var connection = new MySqlConnection(ConnectionString))
                {
                    connection.Open();
                    if (connection.State == ConnectionState.Open)
                        connection.Execute(sql);

                    connection.Close();
                }

                var sql2 = @$"SELECT Id
                                    FROM Categories
                                    WHERE CategoryName = '{product.CategoryName}'";

                using (var connection = new MySqlConnection(ConnectionString))
                {

                    connection.Open();
                    if (connection.State == ConnectionState.Open)
                        CategoryId = connection.QuerySingle<int>(sql2);

                    connection.Close();
                }
            }

            List<SubCategories?>? subCategories = GetAllSubCategories();

            if (subCategories is null) { throw new ArgumentNullException(nameof(subCategories)); }

            foreach (SubCategories? subCategory in subCategories)
            {
                if (subCategory is null) { throw new NullReferenceException(nameof(subCategory)); }

                if (subCategory.SubCategoryName == product.SubCategoryName)
                {
                    SubCategoryId = subCategory.Id;
                    SubCategoryExists = true;
                    break;
                }

            }

            if (SubCategoryExists == false)
            {
                var sql = @$"INSERT INTO SubCategories (SubCategoryName) 
                                    VALUES ('{product.SubCategoryName}')";

                using (var connection = new MySqlConnection(ConnectionString))
                {
                    connection.Open();
                    if (connection.State == ConnectionState.Open)
                        connection.Execute(sql);

                    connection.Close();
                }

                var sql2 = @$"SELECT Id
                                    FROM SubCategories
                                    WHERE SubCategoryName = '{product.SubCategoryName}'";

                using (var connection = new MySqlConnection(ConnectionString))
                {
                    connection.Open();
                    if (connection.State == ConnectionState.Open)
                        SubCategoryId = connection.QuerySingle<int>(sql2);

                    connection.Close();
                }
            }
            // Check if string format is correct (no single quotes and not above 3000 characters long.
            CheckStringFormat(product.Title, product.Description, product.ImgUrl);

            var sqlMain = @$"INSERT INTO Products (Title, Description, AuthorId, CategoryId, SubCategoryId, UnitsInStock, ImgUrl) 
                                    VALUES ('{product.Title}', '{product.Description}', {AuthorId}, {CategoryId}, {SubCategoryId}, {product.UnitsInStock}, '{product.ImgUrl}')";

            using (var connection = new MySqlConnection(ConnectionString))
            {
                connection.Open();
                if (connection.State == ConnectionState.Open)
                    connection.Execute(sqlMain);

                connection.Close();
            }
        }

        #endregion

        #region Category related
        /// <summary>
        /// Adds a new category to the database.
        /// </summary>
        /// <param name="category"></param>
        public static void AddCategory(Categories? category)
        {
            // Check if category passed to the method is null.
            if (category is null) { throw new ArgumentNullException(nameof(category)); }

            // Check format of the categoryname.
            CheckStringFormat(category.CategoryName);

            var cmdText = @$"INSERT INTO Categories (CategoryName)
                                    VALUES ('{category.CategoryName}')";

            using (var connection = new MySqlConnection(ConnectionString))
            {
                connection.Open();
                if (connection.State == ConnectionState.Open)
                    connection.Execute(cmdText);

                connection.Close();
            }
        }

        #endregion

        #region Subcategory related
        /// <summary>
        /// Adds a new subcategory to the database.
        /// </summary>
        /// <param name="subcategory"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public static void AddSubCategory(SubCategories? subcategory)
        {
            // Check if subcategory passed to the method is null.
            if (subcategory is null) { throw new ArgumentNullException(nameof(subcategory)); }

            // Check if format is correct to avoid database errors.
            CheckStringFormat(subcategory.SubCategoryName);

            var cmdText = @$"INSERT INTO SubCategory (SubCategoryName)
                                    VALUES ({subcategory.SubCategoryName})";

            using (var connection = new MySqlConnection(ConnectionString))
            {
                connection.Open();
                if (connection.State == ConnectionState.Open)
                    connection.Execute(cmdText);

                connection.Close();
            }
        }
        #endregion

        #region Loan related
        /// <summary>
        /// Adds a new history related to a user id, using a product id and specifies the action performed by a user on a product.
        /// </summary>
        /// <param name="UserId"></param>
        /// <param name="ProductId"></param>
        /// <param name="ActionId"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="Exception"></exception>
        public static void AddHistory(int UserId, int ProductId, int ActionId)
        {
            // Check if id is a positive integer.
            if (UserId > -1 || ProductId > -1 || ActionId > -1)
            {
                // Query for inserting a new history, here we are using DateTime.Now inline as time is also a datetime object in mysql.
                // Could also use DateTime.UtcNow and add time zone difference to get a more other-zone-friendly time.
                var sqlMain = @$"INSERT INTO History (UserId, ProductId, Datetime, ActionId)
                                    VALUES ({UserId}, {ProductId}, '{DateTime.Now}', {ActionId})";

                using (var connection = new MySqlConnection(ConnectionString))
                {
                    connection.Open();
                    if (connection.State == ConnectionState.Open)
                        connection.Execute(sqlMain);

                    connection.Close();
                }

                // Get product using method that passes a product ID.
                Products? product = GetProductById(ProductId);

                if (product is null) { throw new ArgumentNullException(nameof(product)); }

                // ActionID 1 corresponds to loan.
                if (ActionId == 1)
                {
                    // Remove unit that is loaned from the stock.
                    int unitsInStock = product.UnitsInStock - 1;
                    // Update the number of units currently in stock.
                    UpdateUnitsInStock(ProductId, unitsInStock);
                }

            }
            else { throw new Exception("ID's cannot be negative."); }
        }
        #endregion
    }
}