﻿using AgilSystemutveckling_Xamarin_Net5.Models;
using Dapper;
using MySqlConnector;
using System.Xml;

namespace AgilSystemutveckling_Xamarin_Net5.Pages.Service.CreateService
{
    public class Create
    {
        static string connString = "Server=xamarindb.c6pefsvvniwb.eu-north-1.rds.amazonaws.com; Database=sys; UID=admin; Password=Xamarin321; AllowUserVariables=True;";

        #region User related
        /// <summary>
        /// Adds a user to User table in the database.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public static void AddUser(Users user)
        {
            FirstNames firstName = new();
            LastNames lastName = new();
            MySqlConnection connection = new MySqlConnection(connString);

            // WORKS
            var sql1 = @"INSERT INTO FirstNames (FirstName)
                                VALUES (@FirstName);
                               INSERT INTO LastNames (LastName)
                                VALUES (@LastName);";

            var cmdFirstLastNames = new MySqlCommand(sql1, connection);

            cmdFirstLastNames.Parameters.AddWithValue($"@FirstName", firstName.FirstName);
            cmdFirstLastNames.Parameters.AddWithValue($"@LastName", lastName.LastName);

            connection.Open();
            if (connection.State == System.Data.ConnectionState.Open)
            {
                try
                {
                    int r = cmdFirstLastNames.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    // throw exception
                }
                connection.Close();
            }
            else { /* connection is not open */ }

            FullNames fullName = new();
            //Next query 
            var sql2 = @$"INSERT INTO FullNames (FirstName.Id, LastName.Id) 
                                VALUES (@FirstName.Id, LastName.Id)";

            var cmdFullNameAdd = new MySqlCommand(sql2, connection);

            cmdFirstLastNames.Parameters.AddWithValue($"@FirstName.Id", fullName.FirstNameId);
            cmdFirstLastNames.Parameters.AddWithValue($"@LastName.Id", fullName.LastNameId);

            connection.Open();
            if (connection.State == System.Data.ConnectionState.Open)
            {
                try
                {
                    int r = cmdFullNameAdd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    // throw exception
                }
                connection.Close();
            }
            else { /* connection is not open */ }

            var sql3 = $@"INSERT INTO Users (Username, Password, Address, AccessId)
                                    VALUES(@Username, @Password, @Address, @AccessId)";

            var cmdUser = new MySqlCommand(sql3, connection);



            // error handling around inputs (remove if unnecessary)
            if (user.Username != null && user.Username != "admin")
                cmdUser.Parameters.AddWithValue($"@Username", user.Username);
            if (user.Password != null && user.Password.Length > 7)
                cmdUser.Parameters.AddWithValue($"@Password", user.Password);
            if (user.Address != null)
            cmdUser.Parameters.AddWithValue($"@Address", user.Address);
            if (user.AccessId > -1 && user.AccessId < 5)
            cmdUser.Parameters.AddWithValue($"@AccessId", user.AccessId);

            connection.Open();
            if (connection.State == System.Data.ConnectionState.Open)
            {
                try
                {
                    int r = cmdUser.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    // throw exception
                }
                connection.Close();
            }
            else { /* connection is not open */ }

            // Update user with fullnameid

            var sql4 = $@"UPDATE Users
                                SET Users.FullNameId = (
	                            SELECT FullNames.Id
	                            FROM FullNames
                                WHERE FullNames.Id = Users.Id);";

            var updateWithFullName = new MySqlCommand(sql4, connection);

            // comment in if necessary and change method return to Users.
            // return user;
        }
        #endregion

        #region Author related
        public static Models.Authors AddAuthor(Models.Authors author)
        {
            // author.Id = "<input from user>" - if auto_increment cant be used.

            MySqlConnection connection = new MySqlConnection(connString);


            var cmdText = @$"INSERT INTO Authors (AuthorName)
                                VALUES (@AuthorName)";


            var cmd = new MySqlCommand(cmdText, connection);
            // cmd.Parameters.AddWithValue($"@Id", author.Id); - same as above comment.
            cmd.Parameters.AddWithValue($"@AuthorName", author.AuthorName);
            connection.Open();
            cmd.ExecuteNonQuery();
            connection.Close();

            return author;
        }
        #endregion

        public static void AddProduct(Products product)
        {
            static List<Models.Categories> GetAllCategories()
            {
                var sql = @$"Select Id, CategoryName 
                                From Categories";
                var categories = new List<Models.Categories>();
                using (var connection = new MySqlConnection(connString))
                {
                    connection.Open();
                    categories = connection.Query<Models.Categories>(sql).ToList();
                }

                return categories;
            }

            static List<Models.SubCategories> GetAllSubCategories()
            {
                var sql = @$"Select Id, SubCategoryName 
                                From SubCategories";
                var subCategories = new List<Models.SubCategories>();
                using (var connection = new MySqlConnection(connString))
                {
                    connection.Open();
                    subCategories = connection.Query<Models.SubCategories>(sql).ToList();
                }

                return subCategories;
            }

            int AuthorId = 0;
            int CategoryId = 0;
            int SubCategoryId = 0;

            bool AuthorExists = false;
            bool CategoryExists = false;
            bool SubCategoryExists = false;

            List<Models.Authors> authors = GetService.Get.GetAllAuthors();
            foreach (var author in authors) 
            {
                if (author.AuthorName == product.AuthorName) 
                {
                    AuthorId = author.Id;
                    AuthorExists = true;
                    break;
                }
            }

            if (AuthorExists == false)
            {
                var sql = @$"insert into Authors (AuthorName) 
                        values ('{product.AuthorName}')";
                using (var connection = new MySqlConnection(connString))
                {
                    connection.Open();
                    connection.Execute(sql);
                }

                var sql2 = @$"SELECT Id
                                FROM Authors
                                where AuthorName = '{product.AuthorName}'";

                using (var connection = new MySqlConnection(connString))
                {
                    connection.Open();
                    AuthorId = connection.QuerySingle<int>(sql2);
                }
            }

            List<Models.Categories> categories = GetAllCategories();
            foreach (var category in categories)
            {
                if (category.CategoryName == product.CategoryName)
                {
                    CategoryId = category.Id;
                    CategoryExists = true;
                    break;
                }
            }

            if (CategoryExists == false)
            {
                var sql = @$"insert into Categories (CategoryName) 
                        values ('{product.CategoryName}')";
                using (var connection = new MySqlConnection(connString))
                {
                    connection.Open();
                    connection.Execute(sql);
                }

                var sql2 = @$"SELECT Id
                                FROM Categories
                                where CategoryName = '{product.CategoryName}'";

                using (var connection = new MySqlConnection(connString))
                {
                    connection.Open();
                    CategoryId = connection.QuerySingle<int>(sql2);
                }
            }

            List<Models.SubCategories> subCategories = GetAllSubCategories();
            foreach (var subCategory in subCategories)
            {
                if (subCategory.SubCategoryName == product.SubCategoryName)
                {
                    SubCategoryId = subCategory.Id;
                    SubCategoryExists = true;
                    break;
                }
            }

            if (SubCategoryExists == false)
            {
                var sql = @$"insert into SubCategories (SubCategoryName) 
                        values ('{product.SubCategoryName}')";
                using (var connection = new MySqlConnection(connString))
                {
                    connection.Open();
                    connection.Execute(sql);
                }

                var sql2 = @$"SELECT Id
                                FROM SubCategories
                                where SubCategoryName = '{product.SubCategoryName}'";

                using (var connection = new MySqlConnection(connString))
                {
                    connection.Open();
                    SubCategoryId = connection.QuerySingle<int>(sql2);
                }
            }

            var sqlMain = @$"insert into Products (Title, Description, AuthorId, CategoryId,
                                                    SubCategoryId, UnitsInStock, ImgUrl) 
                        values ('{product.Title}', '{product.Description}', {AuthorId}, {CategoryId}, {SubCategoryId},
                                {product.UnitsInStock}, '{product.ImgUrl}')";
            using (var connection = new MySqlConnection(connString))
            {
                connection.Open();
                connection.Execute(sqlMain);
            }
        }

        #region Category related
        public static Models.Categories AddCategory(Categories category)
        {

            MySqlConnection connection = new MySqlConnection(connString);

            var cmdText = @$"INSERT INTO Categories (CategoryName)
                                VALUES (@CategoryName)";

            var cmd = new MySqlCommand(cmdText, connection);
            cmd.Parameters.AddWithValue($"@CategoryName", category.CategoryName);

            connection.Open();
            int r = cmd.ExecuteNonQuery();
            connection.Close();


            return category;
        }
        #endregion

        #region Subcategory related
        public static Models.SubCategories AddSubCategory(Models.SubCategories subcategory)
        {

            MySqlConnection connection = new MySqlConnection(connString);

            var cmdText = @$"INSERT INTO SubCategory (SubCategoryName)
                                VALUES (@SubCategoryName)";

            var cmd = new MySqlCommand(cmdText, connection);
            cmd.Parameters.AddWithValue($"@SubCategoryName", subcategory.SubCategoryName);

            connection.Open();
            int r = cmd.ExecuteNonQuery();
            connection.Close();

            return subcategory;
        }
        #endregion

        

    }
}
