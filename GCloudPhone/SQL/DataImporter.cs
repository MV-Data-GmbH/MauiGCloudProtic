using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using Microsoft.Maui.Controls;
using SkiaSharp;
using System.Net.Http;
using System.Net;
using System.Text.Json;

namespace GCloudPhone
{
    public class DataImporter
    {
        private string mssqlConnectionString = "Server=192.168.5.12;Database=GCloudProtic_PROD_new_WIP;User Id=sa;Password=kate1;TrustServerCertificate=True;Encrypt=False;Connection Timeout=30;";
        // private string sqliteDbPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Jetorder.db");
        public async Task ImportDataAsync()
        {
            //var sqliteDbFullPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), SQL.databasePath);

            //using (var sqliteConn = new SqliteConnection($"Data Source={sqliteDbFullPath}"))
            using (var sqliteConn = new SqliteConnection($"Data Source={SQL.databasePath}"))
            {
                await sqliteConn.OpenAsync();

                // Zuerst alle vorhandenen Daten in der SQLite-Datenbank löschen
                await ClearSQLiteDataAsync(sqliteConn);

                using (var sqlConnection = new SqlConnection(mssqlConnectionString))
                {
                    await sqlConnection.OpenAsync();

                    // Importieren der Categories
                    var categories = new List<Categories>();
                    using (var sqlCommand = new SqlCommand("SELECT Number, Description1, isnull(Description2,''),isnull( Description3,''), VAT, Groups, isnull(Pictures,0), isnull(Reference,''),Sort,IsVisible FROM Categories", sqlConnection))
                    using (var reader = await sqlCommand.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var category = new Categories
                            {
                                Number = reader.GetInt32(0),
                                Description1 = reader.GetString(1),
                                Description2 = reader.GetString(2),
                                Description3 = reader.GetString(3),
                                VAT = reader.IsDBNull(4) ? (int?)null : reader.GetInt32(4),
                                Groups = reader.IsDBNull(5) ? (int?)null : reader.GetInt32(5),
                                Pictures = reader.IsDBNull(6) ? (int?)null : reader.GetInt32(6),
                                Reference = reader.IsDBNull(7) ? null : reader.GetString(7),
                                Sort = reader.GetInt32(8),
                                IsVisible = reader.GetInt32(9)
                            };
                            categories.Add(category);
                        }
                    }

                    foreach (var category in categories)
                    {
                        await SaveItemAsync(sqliteConn, category);
                    }

                    // Importieren der Groups
                    var groups = new List<Groups>();
                    using (var sqlCommand = new SqlCommand("SELECT Number, Description1, Description2 FROM Groups", sqlConnection))
                    using (var reader = await sqlCommand.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var group = new Groups
                            {
                                Number = reader.GetInt32(0),
                                Description1 = reader.GetString(1),
                                Description2 = reader.GetString(2)
                            };
                            groups.Add(group);
                        }
                    }

                    foreach (var group in groups)
                    {
                        await SaveItemAsync(sqliteConn, group);
                    }

                    // Importieren der Pictures
                    var pictures = new List<Pictures>();
                    using (var sqlCommand = new SqlCommand("SELECT Number, Picturestring FROM Pictures", sqlConnection))
                    using (var reader = await sqlCommand.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var picture = new Pictures
                            {
                                Number = reader.GetInt32(0),
                                Picturestring = reader.IsDBNull(1) ? null : reader.GetString(1)
                            };

                            pictures.Add(picture);
                        }
                    }

                    foreach (var picture in pictures)
                    {
                        if (!string.IsNullOrEmpty(picture.Picturestring))
                        {
                            // Validate
                            if (!IsValidBase64(picture.Picturestring))
                            {
                                // Option A: skip saving this record
                                Console.WriteLine($"Invalid Base64 detected for picture #{picture.Number}. Skipping...");
                                continue;

                                // Option B: set to empty
                                // picture.Picturestring = string.Empty;
                            }
                        }

                        // If valid, proceed to save in the DB
                        await SaveItemAsync(sqliteConn, picture);
                    }

                    // Importieren der VAT
                    var vats = new List<VAT>();
                    using (var sqlCommand = new SqlCommand("SELECT Number, Description1, isnull(Description2,''), [Value], isnull(Reference,'') FROM VAT", sqlConnection))
                    using (var reader = await sqlCommand.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var vat = new VAT
                            {
                                Number = reader.GetInt32(0),
                                Description1 = reader.GetString(1),
                                Description2 = reader.GetString(2),
                                Value = reader.GetDecimal(3),
                                Reference = reader.GetString(4)
                            };
                            vats.Add(vat);
                        }
                    }

                    foreach (var vat in vats)
                    {
                        await SaveItemAsync(sqliteConn, vat);
                    }

                    // Importieren der Products
                    var products = new List<Products>();
                    using (var sqlCommand = new SqlCommand("SELECT Number, Description1, Description2, Description3, isnull(Prices,0), Categories, isnull(Pictures,0), Sort FROM Products", sqlConnection))
                    using (var reader = await sqlCommand.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var product = new Products
                            {
                                Number = reader.GetInt32(0),
                                Description1 = reader.GetString(1),
                                Description2 = reader.GetString(2),
                                Description3 = reader.GetString(3),
                                Prices = reader.IsDBNull(4) ? (int?)null : reader.GetInt32(4),
                                Categories = reader.IsDBNull(5) ? (int?)null : reader.GetInt32(5),
                                Pictures = reader.IsDBNull(6) ? (int?)null : reader.GetInt32(6),
                                Sort = reader.GetInt32(0)
                            };
                            products.Add(product);
                        }
                    }

                    foreach (var product in products)
                    {
                        await SaveItemAsync(sqliteConn, product);
                    }

                    // Importieren der Sidedishes
                    var sidedishes = new List<Sidedishes>();
                    using (var sqlCommand = new SqlCommand("SELECT Number, Description1, Description2, Description3, isnull(Prices,0), Categories, isnull(Pictures,0), Sort FROM Sidedishes", sqlConnection))
                    using (var reader = await sqlCommand.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var sidedish = new Sidedishes
                            {
                                Number = reader.GetInt32(0),
                                Description1 = reader.GetString(1),
                                Description2 = reader.GetString(2),
                                Description3 = reader.GetString(3),
                                Prices = reader.IsDBNull(4) ? (int?)null : reader.GetInt32(4),
                                Categories = reader.IsDBNull(5) ? (int?)null : reader.GetInt32(5),
                                Pictures = reader.IsDBNull(6) ? (int?)null : reader.GetInt32(6),
                                Sort = reader.GetInt32(0)
                            };
                            sidedishes.Add(sidedish);
                        }
                    }

                    foreach (var sidedish in sidedishes)
                    {
                        await SaveItemAsync(sqliteConn, sidedish);
                    }

                    // Importieren der Prices
                    var prices = new List<Prices>();
                    using (var sqlCommand = new SqlCommand("SELECT Description, Amount, isnull(Products,0), isnull(Sidedishes,0), PricesType FROM Prices", sqlConnection))
                    using (var reader = await sqlCommand.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var price = new Prices
                            {
                                Description = reader.GetString(0),
                                Amount = reader.GetDecimal(1),
                                Products = reader.IsDBNull(2) ? (int?)null : reader.GetInt32(2),
                                Sidedishes = reader.IsDBNull(3) ? (int?)null : reader.GetInt32(3),
                                PricesType = reader.GetInt32(4)
                            };
                            prices.Add(price);
                        }
                    }

                    foreach (var price in prices)
                    {
                        await SaveItemAsync(sqliteConn, price);
                    }

                    // Importieren der Products_SD
                    var productsSDs = new List<Products_SD>();
                    using (var sqlCommand = new SqlCommand("SELECT Products, Sidedisches, SDGroups, PageNumber FROM Products_SD", sqlConnection))
                    using (var reader = await sqlCommand.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var productSD = new Products_SD
                            {
                                Products = reader.IsDBNull(0) ? (int?)null : reader.GetInt32(0),
                                Sidedisches = reader.IsDBNull(1) ? (int?)null : reader.GetInt32(1),
                                SDGroups = reader.IsDBNull(2) ? (int?)null : reader.GetInt32(2),
                                PageNumber = reader.IsDBNull(3) ? (int?)null : reader.GetInt32(3)
                            };
                            productsSDs.Add(productSD);
                            Console.WriteLine($"Read Products_SD: Products={productSD.Products}, Sidedisches={productSD.Sidedisches}, SDGroups={productSD.SDGroups}, PageNumber={productSD.PageNumber}");
                        }
                    }

                    foreach (var productSD in productsSDs)
                    {
                        await SaveItemAsync(sqliteConn, productSD);
                    }

                    // Importieren der SDGroups
                    var sdGroups = new List<SDGroups>();
                    using (var sqlCommand = new SqlCommand("SELECT Number, Min, Max FROM SDGroups", sqlConnection))
                    using (var reader = await sqlCommand.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var sdGroup = new SDGroups
                            {
                                Number = reader.GetInt32(0),
                                Min = reader.GetInt32(1),
                                Max = reader.GetInt32(2),
                            };
                            sdGroups.Add(sdGroup);
                        }
                    }

                    foreach (var sdGroup in sdGroups)
                    {
                        await SaveItemAsync(sqliteConn, sdGroup);
                    }

                    // Importieren der SDPages
                    var sdPages = new List<SDPages>();
                    using (var sqlCommand = new SqlCommand("SELECT Number, [Description] FROM SDPages", sqlConnection))
                    using (var reader = await sqlCommand.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var sdPage = new SDPages
                            {
                                Number = reader.GetInt32(0),
                                Description = reader.GetString(1)
                            };
                            sdPages.Add(sdPage);
                        }
                    }

                    foreach (var sdPage in sdPages)
                    {
                        await SaveItemAsync(sqliteConn, sdPage);
                    }

                    // Importieren der Prices_Type
                    var pricesTypes = new List<Prices_Type>();
                    using (var sqlCommand = new SqlCommand("SELECT Number, Description, Idc FROM Prices_Type", sqlConnection))
                    using (var reader = await sqlCommand.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var pricesType = new Prices_Type
                            {
                                Number = reader.GetInt32(0),
                                Description = reader.GetString(1),
                                //Idc = reader.GetInt32(2)
                            };
                            pricesTypes.Add(pricesType);
                        }
                    }

                    foreach (var pricesType in pricesTypes)
                    {
                        await SaveItemAsync(sqliteConn, pricesType);
                    }
                }
            }
        }

        private async Task ClearSQLiteDataAsync(SqliteConnection sqliteConn)
        {
            // Alle vorhandenen Daten aus den relevanten Tabellen löschen
            using (var command = sqliteConn.CreateCommand())
            {
                command.CommandText = "DELETE FROM Products_SD";
                await command.ExecuteNonQueryAsync();
                command.CommandText = "DELETE FROM Prices";
                await command.ExecuteNonQueryAsync();
                command.CommandText = "DELETE FROM Prices_Type";
                await command.ExecuteNonQueryAsync();
                command.CommandText = "DELETE FROM Products";
                await command.ExecuteNonQueryAsync();
                command.CommandText = "DELETE FROM Sidedishes";
                await command.ExecuteNonQueryAsync();
                command.CommandText = "DELETE FROM Categories";
                await command.ExecuteNonQueryAsync();
                command.CommandText = "DELETE FROM Groups";
                await command.ExecuteNonQueryAsync();
                command.CommandText = "DELETE FROM Pictures";
                await command.ExecuteNonQueryAsync();
                command.CommandText = "DELETE FROM VAT";
                await command.ExecuteNonQueryAsync();
                command.CommandText = "DELETE FROM SDGroups";
                await command.ExecuteNonQueryAsync();
                command.CommandText = "DELETE FROM SDPages";
                await command.ExecuteNonQueryAsync();
                command.CommandText = "DELETE FROM Parameters";
                await command.ExecuteNonQueryAsync();
                command.CommandText = "DELETE FROM TimeStampTable";
                await command.ExecuteNonQueryAsync();
                command.CommandText = "DELETE FROM StaticPicture";
                await command.ExecuteNonQueryAsync();
            }
        }
        public bool IsBase64String(string base64)
        {
            // Quick check for obvious non-Base64 patterns
            base64 = base64.Trim();
            if (base64.Length == 0 || base64.Length % 4 != 0
                || base64.Contains(" ") || base64.Contains("\t")
                || base64.Contains("\r") || base64.Contains("\n"))
            {
                return false;
            }

            try
            {
                // If this fails, it's not valid Base64
                Convert.FromBase64String(base64);
                return true;
            }
            catch
            {
                return false;
            }
        }
        public async Task<bool> IsValidPicture(Pictures picture)
        {
            bool isValid = IsBase64String(picture.Picturestring);
            if (!isValid)
            {
                // Switch to the UI thread to display an alert
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Invalid Image",
                        $"Picture Number {picture.Number} has invalid Base64. Skipping or clearing the string.",
                        "OK"
                    );
                });
            }
            return isValid;
        }
        private bool IsValidBase64(string base64String)
        {
            if (string.IsNullOrWhiteSpace(base64String))
                return false;

            // Optionally trim
            base64String = base64String.Trim();

            // Quick length check: Base64 length should be multiple of 4
            if (base64String.Length % 4 != 0)
                return false;

            try
            {
                _ = Convert.FromBase64String(base64String);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }
        private async Task SaveItemAsync<T>(SqliteConnection sqliteConn, T item)
        {
            // Beispielhafte Methode zum Speichern eines Elements in die SQLite-Datenbank
            try
            {
                using (var transaction = sqliteConn.BeginTransaction())
                {
                    using (var command = sqliteConn.CreateCommand())
                    {
                        if (typeof(T) == typeof(Categories))
                        {
                            var category = item as Categories;
                            category.Description1 = category.Description1 ?? "";
                            category.Description2 = category.Description2 ?? "";
                            category.Description3 = category.Description3 ?? "";
                            category.Pictures = category.Pictures ?? 0;
                            category.Reference = category.Reference ?? "";
                            category.Sort = category.Sort > 0 ? category.Sort : 0;
                            category.IsVisible = category.IsVisible > 0 ? 1 : 0;

                            command.CommandText = @"INSERT INTO Categories (Number, Description1, Description2, Description3, VAT, Groups, Pictures, Reference,sort,isvisible)
                                        VALUES (@Number, @Description1, @Description2, @Description3, @VAT, @Groups, @Pictures, @Reference, @sort, @isvisible)";
                            command.Parameters.AddWithValue("@Number", category.Number);
                            command.Parameters.AddWithValue("@Description1", category.Description1);
                            command.Parameters.AddWithValue("@Description2", category.Description2);
                            command.Parameters.AddWithValue("@Description3", category.Description3);
                            command.Parameters.AddWithValue("@VAT", category.VAT);
                            command.Parameters.AddWithValue("@Groups", category.Groups);
                            command.Parameters.AddWithValue("@Pictures", category.Pictures);
                            command.Parameters.AddWithValue("@Reference", category.Reference);
                            command.Parameters.AddWithValue("@sort", category.Sort);
                            command.Parameters.AddWithValue("@isvisible", category.IsVisible);
                        }
                        else if (typeof(T) == typeof(Groups))
                        {
                            var group = item as Groups;
                            group.Description1 = group.Description1 ?? "";
                            group.Description2 = group.Description2 ?? "";

                            command.CommandText = @"INSERT INTO Groups (Number, Description1, Description2)
                                        VALUES (@Number, @Description1, @Description2)";
                            command.Parameters.AddWithValue("@Number", group.Number);
                            command.Parameters.AddWithValue("@Description1", group.Description1);
                            command.Parameters.AddWithValue("@Description2", group.Description2);
                        }
                        if (typeof(T) == typeof(Pictures))
                        {
                            var picture = item as Pictures;
                            picture.Picturestring = picture.Picturestring ?? "";
                            picture.Description = picture.Description ?? "";

                            // ✅ Validate the base64 string
                            if (!IsValidBase64(picture.Picturestring))
                            {
                                Console.WriteLine($"Skipping picture #{picture.Number} because its Picturestring is invalid Base64.");
                                // Return without inserting
                                return;
                            }

                            command.CommandText = @"
                        INSERT OR REPLACE INTO Pictures (Number, Picturestring, Description)
                        VALUES (@Number, @Picturestring, @Description)";
                            command.Parameters.AddWithValue("@Number", picture.Number);
                            command.Parameters.AddWithValue("@Picturestring", picture.Picturestring);
                            command.Parameters.AddWithValue("@Description", picture.Description);

                            command.ExecuteNonQuery();
                        }
                        else if (typeof(T) == typeof(VAT))
                        {
                            var vat = item as VAT;
                            vat.Description1 = vat.Description1 ?? "";
                            vat.Description2 = vat.Description2 ?? "";
                            vat.Reference = vat.Reference ?? "";

                            command.CommandText = @"INSERT INTO VAT (Number, Description1, Description2, Value, Reference)
                                        VALUES (@Number, @Description1, @Description2, @Value, @Reference)";
                            command.Parameters.AddWithValue("@Number", vat.Number);
                            command.Parameters.AddWithValue("@Description1", vat.Description1);
                            command.Parameters.AddWithValue("@Description2", vat.Description2);
                            command.Parameters.AddWithValue("@Value", vat.Value);
                            command.Parameters.AddWithValue("@Reference", vat.Reference);
                        }
                        else if (typeof(T) == typeof(Products))
                        {
                            var product = item as Products;
                            product.Description1 = product.Description1 ?? "";
                            product.Description2 = product.Description2 ?? "";
                            product.Description3 = product.Description3 ?? "";
                            product.Prices = product.Prices ?? 0;
                            product.Categories = product.Categories ?? 0;
                            product.Pictures = product.Pictures ?? 0;
                            //product.PicturesBanner = product.PicturesBanner ?? 0;
                            product.Sort = product.Sort;

                            command.CommandText = @"INSERT INTO Products (Number, Description1, Description2, Description3, Prices, Categories, Pictures, Sort)
                                        VALUES (@Number, @Description1, @Description2, @Description3, @Prices, @Categories, @Pictures, @Sort)";
                            command.Parameters.AddWithValue("@Number", product.Number);
                            command.Parameters.AddWithValue("@Description1", product.Description1);
                            command.Parameters.AddWithValue("@Description2", product.Description2);
                            command.Parameters.AddWithValue("@Description3", product.Description3);
                            command.Parameters.AddWithValue("@Prices", product.Prices);
                            command.Parameters.AddWithValue("@Categories", product.Categories);
                            command.Parameters.AddWithValue("@Pictures", product.Pictures);
                            command.Parameters.AddWithValue("@Sort", product.Sort);

                        }
                        else if (typeof(T) == typeof(Sidedishes))
                        {
                            var sidedish = item as Sidedishes;
                            sidedish.Description1 = sidedish.Description1 ?? "";
                            sidedish.Description2 = sidedish.Description2 ?? "";
                            sidedish.Description3 = sidedish.Description3 ?? "";
                            sidedish.Prices = sidedish.Prices ?? 0;
                            sidedish.Categories = sidedish.Categories ?? 0;
                            sidedish.Pictures = sidedish.Pictures ?? 0;
                            sidedish.Sort = sidedish.Sort;

                            command.CommandText = @"INSERT INTO Sidedishes (Number, Description1, Description2, Description3, Prices, Categories, Pictures, Sort)
                                        VALUES (@Number, @Description1, @Description2, @Description3, @Prices, @Categories, @Pictures,@Sort)";
                            command.Parameters.AddWithValue("@Number", sidedish.Number);
                            command.Parameters.AddWithValue("@Description1", sidedish.Description1);
                            command.Parameters.AddWithValue("@Description2", sidedish.Description2);
                            command.Parameters.AddWithValue("@Description3", sidedish.Description3);
                            command.Parameters.AddWithValue("@Prices", sidedish.Prices);
                            command.Parameters.AddWithValue("@Categories", sidedish.Categories);
                            command.Parameters.AddWithValue("@Pictures", sidedish.Pictures);
                            command.Parameters.AddWithValue("@Sort", sidedish.Sort);
                        }
                        else if (typeof(T) == typeof(Prices))
                        {
                            var price = item as Prices;
                            price.Description = price.Description ?? "";
                            price.Products = price.Products ?? 0;
                            price.Sidedishes = price.Sidedishes ?? 0;
                            price.PricesType = price.PricesType ?? 0;
                            //price.Amount = price.Amount ?? 0.0m;

                            command.CommandText = @"INSERT INTO Prices (Description, Amount, Products, Sidedishes, PricesType)
                                        VALUES (@Description, @Amount, @Products, @Sidedishes, @PricesType)";
                            command.Parameters.AddWithValue("@Description", price.Description);
                            command.Parameters.AddWithValue("@Amount", price.Amount);
                            command.Parameters.AddWithValue("@Products", price.Products);
                            command.Parameters.AddWithValue("@Sidedishes", price.Sidedishes);
                            command.Parameters.AddWithValue("@PricesType", price.PricesType);
                        }
                        else if (typeof(T) == typeof(Products_SD))
                        {
                            var productSD = item as Products_SD;
                            productSD.Products = productSD.Products ?? 0;
                            productSD.Sidedisches = productSD.Sidedisches ?? 0;
                            productSD.SDGroups = productSD.SDGroups ?? 0;
                            productSD.PageNumber = productSD.PageNumber ?? 0;

                            //if(productSD.Sidedisches!=0)
                            //        {
                            //    Console.WriteLine("Sidedisches: " + productSD.Sidedisches);
                            //}
                            command.CommandText = @"INSERT INTO Products_SD (Products, Sidedisches, SDGroups, PageNumber)
                                        VALUES (@Products, @Sidedisches, @SDGroups, @PageNumber)";
                            command.Parameters.AddWithValue("@Products", productSD.Products);
                            command.Parameters.AddWithValue("@Sidedisches", productSD.Sidedisches);
                            command.Parameters.AddWithValue("@SDGroups", productSD.SDGroups);
                            command.Parameters.AddWithValue("@PageNumber", productSD.PageNumber);
                        }
                        else if (typeof(T) == typeof(SDGroups))
                        {
                            var sdGroup = item as SDGroups;
                            sdGroup.Min = sdGroup.Min ?? 0;
                            sdGroup.Max = sdGroup.Max ?? 0;

                            command.CommandText = @"INSERT INTO SDGroups (Number, Min, Max)
                                        VALUES (@Number, @Min, @Max)";
                            command.Parameters.AddWithValue("@Number", sdGroup.Number);
                            command.Parameters.AddWithValue("@Min", sdGroup.Min);
                            command.Parameters.AddWithValue("@Max", sdGroup.Max);

                        }
                        else if (typeof(T) == typeof(SDPages))
                        {
                            var sdPage = item as SDPages;
                            sdPage.Description = sdPage.Description ?? "";

                            command.CommandText = @"INSERT INTO SDPages (Number, Description)
                                        VALUES (@Number, @Description)";
                            command.Parameters.AddWithValue("@Number", sdPage.Number);
                            command.Parameters.AddWithValue("@Description", sdPage.Description);
                        }
                        else if (typeof(T) == typeof(Prices_Type))
                        {
                            var pricesType = item as Prices_Type;
                            pricesType.Description = pricesType.Description ?? "";

                            command.CommandText = @"INSERT INTO Prices_Type (Number, Description)
                                        VALUES (@Number, @Description)";
                            command.Parameters.AddWithValue("@Number", pricesType.Number);
                            command.Parameters.AddWithValue("@Description", pricesType.Description);
                        }
                        else if (typeof(T) == typeof(Parameters))
                        {
                            var parameters = item as Parameters;
                            parameters.Parameter = parameters.Parameter ?? "";
                            parameters.Value = parameters.Value ?? "";

                            command.CommandText = @"INSERT INTO parameters (Parameter, Value)
                                        VALUES (@Parameter, @Value)";
                            command.Parameters.AddWithValue("@Parameter", parameters.Parameter);
                            command.Parameters.AddWithValue("@Value", parameters.Value);
                        }
                        else if (typeof(T) == typeof(StoreOpeningHours))
                        {
                            var storeOpeningHours = item as StoreOpeningHours;
                            storeOpeningHours.OpenFrom = storeOpeningHours.OpenFrom ?? "";
                            storeOpeningHours.OpenTo = storeOpeningHours.OpenTo ?? "";

                            command.CommandText = @"INSERT INTO StoreOpeningHours (StoreID, DayOfWeek, OpenFrom, OpenTo, TimeSlotLength, OrdersInTimeSlot)
                                        VALUES (@StoreID, @DayOfWeek, @OpenFrom, @OpenTo, @TimeSlotLength, @OrdersInTimeSlot)";
                            command.Parameters.AddWithValue("@StoreID", storeOpeningHours.StoreID);
                            command.Parameters.AddWithValue("@DayOfWeek", storeOpeningHours.DayOfWeek);
                            command.Parameters.AddWithValue("@OpenFrom", storeOpeningHours.OpenFrom);
                            command.Parameters.AddWithValue("@OpenTo", storeOpeningHours.OpenTo);
                            command.Parameters.AddWithValue("@TimeSlotLength", storeOpeningHours.TimeSlotLength);
                            command.Parameters.AddWithValue("@OrdersInTimeSlot", storeOpeningHours.OrdersInTimeSlot);
                        }
                        else if (typeof(T) == typeof(Stores))
                        {
                            var stores = item as Stores;
                            stores.Name = stores.Name ?? "";
                            stores.City = stores.City ?? "";
                            stores.Street = stores.Street ?? "";
                            stores.HouseNr = stores.HouseNr ?? "";
                            stores.Plz = stores.Plz ?? "";
                            stores.ApiToken = stores.ApiToken ?? "";
                            stores.EBillCategory_Id = stores.EBillCategory_Id ?? "";
                            stores.SpecialProduct_Id = stores.SpecialProduct_Id ?? "";
                            stores.StoreWebSite = stores.StoreWebSite ?? "";
                            stores.CashRegisterName = stores.CashRegisterName ?? "";
                            stores.IPAddress = stores.IPAddress ?? "";
                            stores.ShortId = stores.ShortId ?? "";

                            command.CommandText = @"INSERT INTO Stores (Id, Name, City, Street, HouseNr, Plz, CreationDateTime, ApiToken, CompanyId, CountryId, IsDeleted, Latitude, Longitude, EBillCategory_Id, PointsPerEuro, EuroPerPoints, SpecialProduct_Id, StoreWebSite, CashRegisterName, IPAddress, ShortId, StoreGroup)
                                        VALUES (@Id, @Name, @City, @Street, @HouseNr, @Plz, @CreationDateTime, @ApiToken, @CompanyId, @CountryId, @IsDeleted, @Latitude, @Longitude, @EBillCategory_Id, @PointsPerEuro, @EuroPerPoints, @SpecialProduct_Id, @StoreWebSite, @CashRegisterName, @IPAddress, @ShortId, @StoreGroup)";
                            command.Parameters.AddWithValue("@Id", stores.Id);
                            command.Parameters.AddWithValue("@Name", stores.Name);
                            command.Parameters.AddWithValue("@City", stores.City);
                            command.Parameters.AddWithValue("@Street", stores.Street);
                            command.Parameters.AddWithValue("@HouseNr", stores.HouseNr);
                            command.Parameters.AddWithValue("@Plz", stores.Plz);
                            command.Parameters.AddWithValue("@CreationDateTime", stores.CreationDateTime);
                            command.Parameters.AddWithValue("@ApiToken", stores.ApiToken);
                            command.Parameters.AddWithValue("@CompanyId", stores.CompanyId);
                            command.Parameters.AddWithValue("@CountryId", stores.CountryId);
                            command.Parameters.AddWithValue("@IsDeleted", stores.IsDeleted);
                            command.Parameters.AddWithValue("@Latitude", stores.Latitude);
                            command.Parameters.AddWithValue("@Longitude", stores.Longitude);
                            command.Parameters.AddWithValue("@EBillCategory_Id", stores.EBillCategory_Id);
                            command.Parameters.AddWithValue("@PointsPerEuro", stores.PointsPerEuro);
                            command.Parameters.AddWithValue("@EuroPerPoints", stores.EuroPerPoints);
                            command.Parameters.AddWithValue("@SpecialProduct_Id", stores.SpecialProduct_Id);
                            command.Parameters.AddWithValue("@StoreWebSite", stores.StoreWebSite);
                            command.Parameters.AddWithValue("@CashRegisterName", stores.CashRegisterName);
                            command.Parameters.AddWithValue("@IPAddress", stores.IPAddress);
                            command.Parameters.AddWithValue("@ShortId", stores.ShortId);
                            command.Parameters.AddWithValue("@StoreGroup", stores.StoreGroup);
                        }
                        /*else if (typeof(T) == typeof(Recommendations))
                        {
                            var recommendation = item as Recommendations;
                            recommendation.NewDescription = recommendation.NewDescription ?? "";

                            command.CommandText = @"INSERT INTO Recommendations (Number, ProductNumber, NewPrice, NewDescription)
                                        VALUES (@Number, @ProductNumber, @NewPrice, @NewDescription)";
                            command.Parameters.AddWithValue("@Number", recommendation.Number);
                            command.Parameters.AddWithValue("@ProductNumber", recommendation.ProductNumber);
                            command.Parameters.AddWithValue("@NewPrice", recommendation.NewPrice);
                            command.Parameters.AddWithValue("@NewDescription", recommendation.NewDescription);
                        }*/
                        else if (typeof(T) == typeof(Addresses))
                        {
                            var address = item as Addresses;
                            address.Name = address.Name ?? "";
                            address.MiddleName = address.MiddleName ?? "";
                            address.SurName = address.SurName ?? "";
                            address.AddressLine1 = address.AddressLine1 ?? "";
                            address.AddressLine2 = address.AddressLine2 ?? "";
                            address.AddressLine3 = address.AddressLine3 ?? "";
                            address.City = address.City ?? "";
                            address.Zip = address.Zip ?? "";
                            address.Country = address.Country ?? "";
                            address.Phone = address.Phone ?? "";
                            address.Email = address.Email ?? "";
                            address.ContactPerson = address.ContactPerson ?? "";
                            address.Notes = address.Notes ?? "";
                            address.AddressType = address.AddressType ?? "";

                            command.CommandText = @"INSERT INTO Addresses (UserID, Name, MiddleName, SurName, AddressLine1, AddressLine2, AddressLine3, City, Zip, Country, Phone, Email, ContactPerson, Notes, AddressType, IsDefault)
                                            VALUES (@UserID, @Name, @MiddleName, @SurName, @AddressLine1, @AddressLine2, @AddressLine3, @City, @Zip, @Country, @Phone, @Email, @ContactPerson, @Notes, @AddressType, @IsDefault)";

                            command.Parameters.AddWithValue("@UserID", address.UserID);
                            command.Parameters.AddWithValue("@Name", address.Name);
                            command.Parameters.AddWithValue("@MiddleName", address.MiddleName);
                            command.Parameters.AddWithValue("@SurName", address.SurName);
                            command.Parameters.AddWithValue("@AddressLine1", address.AddressLine1);
                            command.Parameters.AddWithValue("@AddressLine2", address.AddressLine2);
                            command.Parameters.AddWithValue("@AddressLine3", address.AddressLine3);
                            command.Parameters.AddWithValue("@City", address.City);
                            command.Parameters.AddWithValue("@Zip", address.Zip);
                            command.Parameters.AddWithValue("@Country", address.Country);
                            command.Parameters.AddWithValue("@Phone", address.Phone);
                            command.Parameters.AddWithValue("@Email", address.Email);
                            command.Parameters.AddWithValue("@ContactPerson", address.ContactPerson);
                            command.Parameters.AddWithValue("@Notes", address.Notes);
                            command.Parameters.AddWithValue("@AddressType", address.AddressType);
                            command.Parameters.AddWithValue("@IsDefault", address.IsDefault);
                        }
                        else if (typeof(T) == typeof(TimeStampTable))
                        {
                            var timestamp = item as TimeStampTable;
                            //timestamp.DataUpdated = timestamp.DataUpdated ?? DateTime.Now;

                            command.CommandText = @"INSERT INTO TimeStampTable (DataUpdated)
                            VALUES (@DataUpdated)";
                            command.Parameters.AddWithValue("@DataUpdated", timestamp.DataUpdated);
                        }
                        await command.ExecuteNonQueryAsync();
                    }
                    await transaction.CommitAsync();
                }
            }
            catch (Exception ex)
            {
                var s = ex.Message;

            }

        }

        private async Task<string> ResizePicture(string pictureString)
        {
            try
            {
                return await Task.Run(() =>
                {
                    // Decode the base64 string to a byte array
                    byte[] imageBytes = Convert.FromBase64String(pictureString);

                    // Load the image into a SkiaSharp bitmap
                    using var inputStream = new MemoryStream(imageBytes);
                    using var original = SKBitmap.Decode(inputStream);

                    // Resize the image to 100x100 pixels
                    using var resized = original.Resize(new SKImageInfo(100, 100), SKFilterQuality.Medium);

                    // Encode the resized image back to a byte array
                    using var image = SKImage.FromBitmap(resized);
                    using var outputStream = new MemoryStream();
                    image.Encode(SKEncodedImageFormat.Png, 100).SaveTo(outputStream);
                    byte[] resizedImageBytes = outputStream.ToArray();

                    // Convert the byte array back to a base64 string
                    return Convert.ToBase64String(resizedImageBytes);
                });
            }
            catch (FormatException ex)
            {
                // A FormatException usually indicates invalid Base64
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Base64 Error",
                        $"An invalid Base64 string was detected: {ex.Message}",
                        "OK"
                    );
                });

                return null;
            }
        }



        public async Task ImportDataAsyncFromCloud(byte[] data)
        {


            var compressedData = data;
            var dataDecompressor = new DataDecompressor();
            var (categories, groups, pictures, prices, priceTypes, products,
                    productSDs, sdGroups, sdPages, sidedishes, vats, stores, staticPictures) =
                    await dataDecompressor.DecompressAndDeserializeAllData(compressedData);
            //proba
            //var sqliteDbFullPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), SQL.databasePath);
            //using (var sqliteConn = new SqliteConnection($"Data Source={sqliteDbFullPath}"))
            using (var sqliteConn = new SqliteConnection($"Data Source={SQL.databasePath}"))
            {
                await sqliteConn.OpenAsync();
                // Zuerst alle vorhandenen Daten in der SQLite-Datenbank löschen
                await ClearSQLiteDataAsync(sqliteConn);
                foreach (var category in categories)
                {
                    await SaveItemAsync(sqliteConn, category);
                }
                foreach (var group in groups)
                {
                    await SaveItemAsync(sqliteConn, group);
                }
                foreach (var picture in pictures)
                {
                    if (!string.IsNullOrEmpty(picture.Picturestring))
                    {
                        // Validate
                        if (!IsValidBase64(picture.Picturestring))
                        {
                            // Option A: skip saving this record
                            Console.WriteLine($"Invalid Base64 detected for picture #{picture.Number}. Skipping...");
                            continue;

                            // Option B: set to empty
                            // picture.Picturestring = string.Empty;
                        }
                    }

                    // If valid, proceed to save in the DB
                    await SaveItemAsync(sqliteConn, picture);
                }
                foreach (var pricesType in priceTypes)
                {
                    await SaveItemAsync(sqliteConn, pricesType);
                }
                foreach (var product in products)
                {
                    await SaveItemAsync(sqliteConn, product);
                }
                foreach (var productSD in productSDs)
                {
                    await SaveItemAsync(sqliteConn, productSD);
                }
                foreach (var sdGroup in sdGroups)
                {
                    await SaveItemAsync(sqliteConn, sdGroup);
                }
                foreach (var sdPage in sdPages)
                {
                    await SaveItemAsync(sqliteConn, sdPage);
                }
                foreach (var sidedish in sidedishes)
                {
                    await SaveItemAsync(sqliteConn, sidedish);
                }
                foreach (var price in prices)
                {
                    await SaveItemAsync(sqliteConn, price);
                }
                foreach (var vat in vats)
                {
                    await SaveItemAsync(sqliteConn, vat);
                }
                foreach (var store in stores)
                {
                    await SaveItemAsync(sqliteConn, store);
                }
                foreach (var staticPicture in staticPictures)
                {
                    await SaveItemAsync(sqliteConn, staticPicture);
                }
            }
        }

        public async Task<byte[]> GetDataFromDataManagerAsync()
        {
            string url = "https://protictest1.willessen.online/api/HomeApi/GetDataFromDataManager";
            try
            {
                using (HttpClient httpClient = new HttpClient())
                {
                    HttpResponseMessage response = await httpClient.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    byte[] data = await response.Content.ReadAsByteArrayAsync();
                    return data;
                }
            }
            catch (HttpRequestException e)
            {
                // Handle the exception as needed
                throw new Exception("An error occurred while calling the API.", e);
            }
        }
        public async Task<byte[]> GetDataFromDataManagerWithDateAsync(DateTime timestamp)
        {

            // Format the DateTime timestamp to ensure it is in the correct format
            string formattedTimestamp = timestamp.ToString("yyyy-MM-ddTHH:mm:ss");

            // Construct the URL with the formatted timestamp as a query parameter
            string url = $"https://protictest1.willessen.online/api/HomeApi/GetDataFromDataManagerWithDate?timestamp={formattedTimestamp}";


            try
            {
                using (HttpClient httpClient = new HttpClient())
                {
                    HttpResponseMessage response = await httpClient.GetAsync(url);
                    response.EnsureSuccessStatusCode();

                    if (response.StatusCode == HttpStatusCode.NoContent)
                    {
                        // Handle the case where there is no content
                        return null; // or handle it as needed
                    }

                    byte[] data = await response.Content.ReadAsByteArrayAsync();
                    return data;
                }
            }
            catch (HttpRequestException e)
            {
                // Handle the exception as needed
                throw new Exception("An error occurred while calling the API.", e);
            }
        }




        public async Task<TimeStampTable> GetTimestamp()
        {
            string url = "https://protictest1.willessen.online/api/HomeApi/GetTimeStamp";
            try
            {
                using (HttpClient httpClient = new HttpClient())
                {
                    HttpResponseMessage response = await httpClient.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    string jsonResponse = await response.Content.ReadAsStringAsync();

                    return JsonSerializer.Deserialize<TimeStampTable>(jsonResponse);
                }
            }
            catch (HttpRequestException e)
            {
                throw new Exception("An error occurred while calling the API.", e);
            }

        }

        public async Task CheckForImport()


        {
            TimeStampTable timeStamp = await SQL.GetTimeStampTable();

            if (timeStamp == null)
            {
                // No timestamp exists, import data from cloud
                await ImportDataAsyncFromCloud(await GetDataFromDataManagerAsync());
                await SaveTimeStamp();
            }
            else
            {
                // Timestamp exists, check if new data is available
                byte[] data = await GetDataFromDataManagerWithDateAsync(timeStamp.DataUpdated);

                if (data == null)
                {
                    // No new content available (204 No Content), data is already up-to-date
                    // No further action needed
                    return; // Exits the method early
                }
                else
                {
                    // New data available, import it
                    await ImportDataAsyncFromCloud(data);
                    await SaveTimeStamp();
                }
            }
        }




        public async Task SaveTimeStamp()
        {
            TimeStampTable timestap = await GetTimestamp();
            using (var sqliteConn = new SqliteConnection($"Data Source={SQL.databasePath}"))
            {
                await sqliteConn.OpenAsync();

                using (var command = sqliteConn.CreateCommand())
                {
                    command.CommandText = "DELETE FROM TimeStampTable";
                    await command.ExecuteNonQueryAsync();
                }
                await SaveItemAsync(sqliteConn, timestap);

            }
        }
    }
}
