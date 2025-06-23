using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using GCloudShared.Interface;
using GCloudShared.Service;
using GCloud.Shared.Dto.Domain;
using SQLite;
using GCloudShared.Repository;
using GCloudShared.Shared;

namespace GCloudPhone
{
    public class StartUpDataImport
    {
        public static List<Coupons> coupons { get; set; } = new List<Coupons>();

        public static string databasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Jetorder.db");
        public static SQLiteAsyncConnection _database = new SQLiteAsyncConnection(databasePath);

        public async Task ImportData(IAuthService authService)
        {
            List<StoreOpeningHours> openingHours = await GetStoreOpeningHours();
            List<ApiRecommendation> recommendations = await GetRecommendations();
            List<ApiRecommendation> defaultRecommendations = await GetDefaultRecommendations();
            List<Parameters> parameters = await GetParameters();


            var storeService = new StoreService();
            var userService = new UserCouponService();
            List<StoreDto> stores = await storeService.GetStores() as List<StoreDto>;
            var firstStore = new StoreDto();
            firstStore = stores[0];

            var storeFirstId = firstStore.Id.ToString();

            if (authService.IsLogged())
            {
                coupons = ConvertDtoToCoupons((List<CouponDto>)await userService.GetUserCouponsByStore(storeFirstId));
            }

            //coupons = (List<Coupons>)await userService.GetUserCouponsByStore(storeFirstId);

            using (var sqliteConn = new SqliteConnection($"Data Source={SQL.databasePath}"))
            {
                await sqliteConn.OpenAsync();
                // Zuerst alle vorhandenen Daten in der SQLite-Datenbank löschen
                await ClearSQLiteDataAsync(sqliteConn);
                foreach (var openingHour in openingHours)
                {
                    await SaveItemAsync(sqliteConn, openingHour);
                }
                foreach (var parameter in parameters)
                {
                    await SaveItemAsync(sqliteConn, parameter);
                }
                foreach (var coupon in coupons)
                {
                    await SaveItemAsync(sqliteConn, coupon);
                }

            }
            await SaveRecommendationsAsync(recommendations, _database);
            await SaveDefaultRecommendationsAsync(defaultRecommendations, _database);
        }
        private List<Coupons> ConvertDtoToCoupons(List<CouponDto> couponDtoList)
        {
            var couponsList = new List<Coupons>();

            foreach (var couponDto in couponDtoList)
            {
                var coupon = new Coupons
                {
                    Id = couponDto.Id,
                    Name = couponDto.Name ?? "",  // Standard auf leeren String setzen, wenn null
                    ShortDescription = couponDto.ShortDescription ?? "",
                    MaxRedeems = couponDto.MaxRedeems,
                    RedeemsLeft = couponDto.RedeemsLeft,
                    ValidFrom = couponDto.ValidFrom,
                    ValidTo = couponDto.ValidTo,
                    Value = couponDto.Value,
                    CouponType = (int)couponDto.CouponType,  // Assuming enum or corresponding int conversion
                    CouponScope = (int)couponDto.CouponScope,  // Assuming enum or corresponding int conversion
                    ArticleNumber = couponDto.ArticleNumber,
                    IsValid = couponDto.IsValid,
                    IconBase64 = couponDto.IconBase64 ?? "",
                    CouponPoints = couponDto.CouponPoints ?? "",
                    TextColor = couponDto.TextColor != null ? couponDto.TextColor.ToString() : "",  // Assuming color conversion
                    PointsText = couponDto.PointsText ?? "",
                    BorderColor = couponDto.BorderColor != null ? couponDto.BorderColor.ToString() : "",  // Assuming color conversion
                    PictureWidth = couponDto.PictureWidth
                };

                couponsList.Add(coupon);
            }

            return couponsList;
        }

        private async Task ClearSQLiteDataAsync(SqliteConnection sqliteConn)
        {
            // Alle vorhandenen Daten aus den relevanten Tabellen löschen
            using (var command = sqliteConn.CreateCommand())
            {
                command.CommandText = "DELETE FROM Recommendation";
                await command.ExecuteNonQueryAsync();
                command.CommandText = "DELETE FROM RecommendedProduct";
                await command.ExecuteNonQueryAsync();
                command.CommandText = "DELETE FROM StoreOpeningHours";
                await command.ExecuteNonQueryAsync();
                command.CommandText = "DELETE FROM Parameters";
                await command.ExecuteNonQueryAsync();
                command.CommandText = "DELETE FROM Coupons";
                await command.ExecuteNonQueryAsync();
            }
        }

        private async Task<List<StoreOpeningHours>> GetStoreOpeningHours()
        {
            string url = "https://protictest1.willessen.online/api/HomeApi/GetStoreOpeningHours";
            try
            {
                using (HttpClient httpClient = new HttpClient())
                {
                    HttpResponseMessage response = await httpClient.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    string jsonResponse = await response.Content.ReadAsStringAsync();

                    return JsonSerializer.Deserialize<List<StoreOpeningHours>>(jsonResponse);
                }
            }
            catch (HttpRequestException e)
            {
                throw new Exception("An error occurred while calling the API.", e);
            }
        }

        private async Task<List<ApiRecommendation>> GetRecommendations()
        {
            UserRepository ur = new UserRepository(DbBootstraper.Connection);
            var user = ur.GetCurrentUser();
            string userId = user?.UserId;
            if (string.IsNullOrEmpty(userId))
                return new List<ApiRecommendation>();
            string url = "https://protictest1.willessen.online/api/HomeApi/Recommendations?userId={userId}";
            /*string url = string.IsNullOrEmpty(userId)
                ? $"{baseUrl}/DefaultRecommendations"
                : $"{baseUrl}/Recommendations?userId={userId}";*/
            try
            {
                using (HttpClient httpClient = new HttpClient())
                {
                    HttpResponseMessage response = await httpClient.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<ApiRecommendation>>(jsonResponse);
                }
            }
            catch (HttpRequestException e)
            {
                // Log the exception
                throw new Exception($"An error occurred while calling the API: {url}", e);
            }
        }

        //default recommendations
        private async Task<List<ApiRecommendation>> GetDefaultRecommendations()
        {
            string url = "https://protictest1.willessen.online/api/HomeApi/DefaultRecommendations";
            try
            {
                using (HttpClient httpClient = new HttpClient())
                {
                    HttpResponseMessage response = await httpClient.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    string jsonResponse = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<ApiRecommendation>>(jsonResponse);
                }
            }
            catch (HttpRequestException e)
            {
                // Log the exception
                throw new Exception($"An error occurred while calling the API: {url}", e);
            }
        }

        private async Task<List<Parameters>> GetParameters()
        {
            string url = "https://protictest1.willessen.online/api/HomeApi/GetParameters"; // Replace with your actual URL

            try
            {
                using (HttpClient httpClient = new HttpClient())
                {
                    HttpResponseMessage response = await httpClient.GetAsync(url);
                    response.EnsureSuccessStatusCode();
                    string jsonResponse = await response.Content.ReadAsStringAsync();

                    return JsonSerializer.Deserialize<List<Parameters>>(jsonResponse);
                }
            }
            catch (HttpRequestException e)
            {
                throw new Exception("An error occurred while calling the API.", e);
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
                        if (typeof(T) == typeof(Parameters))
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

                        else if (typeof(T) == typeof(Coupons))
                        {
                            var coupon = item as Coupons;

                            command.CommandText = @"INSERT INTO Coupons (Id, Name, ShortDescription, MaxRedeems, RedeemsLeft, ValidFrom, ValidTo, Value, CouponType, CouponScope, ArticleNumber, IsValid, IconBase64, CouponPoints, TextColor, PointsText, BorderColor, PictureWidth)
                            VALUES (@Id, @Name, @ShortDescription, @MaxRedeems, @RedeemsLeft, @ValidFrom, @ValidTo, @Value, @CouponType, @CouponScope, @ArticleNumber, @IsValid, @IconBase64, @CouponPoints, @TextColor, @PointsText, @BorderColor, @PictureWidth)";

                            command.Parameters.AddWithValue("@Id", coupon.Id);
                            command.Parameters.AddWithValue("@Name", coupon.Name ?? "");
                            command.Parameters.AddWithValue("@ShortDescription", coupon.ShortDescription ?? "");
                            command.Parameters.AddWithValue("@MaxRedeems", coupon.MaxRedeems.HasValue ? (object)coupon.MaxRedeems.Value : DBNull.Value);
                            command.Parameters.AddWithValue("@RedeemsLeft", coupon.RedeemsLeft.HasValue ? (object)coupon.RedeemsLeft.Value : DBNull.Value);
                            command.Parameters.AddWithValue("@ValidFrom", coupon.ValidFrom.HasValue ? (object)coupon.ValidFrom.Value : DBNull.Value);
                            command.Parameters.AddWithValue("@ValidTo", coupon.ValidTo.HasValue ? (object)coupon.ValidTo.Value : DBNull.Value);
                            command.Parameters.AddWithValue("@Value", coupon.Value);
                            command.Parameters.AddWithValue("@CouponType", coupon.CouponType);  // Assuming CouponType as int
                            command.Parameters.AddWithValue("@CouponScope", coupon.CouponScope);  // Assuming CouponScope as int
                            command.Parameters.AddWithValue("@ArticleNumber", coupon.ArticleNumber.HasValue ? (object)coupon.ArticleNumber.Value : DBNull.Value);
                            command.Parameters.AddWithValue("@IsValid", coupon.IsValid ? 1 : 0);  // BOOLEAN represented as 0 or 1 in SQLite
                            command.Parameters.AddWithValue("@IconBase64", coupon.IconBase64 ?? "");
                            command.Parameters.AddWithValue("@CouponPoints", coupon.CouponPoints ?? "");
                            command.Parameters.AddWithValue("@TextColor", coupon.TextColor ?? "");
                            command.Parameters.AddWithValue("@PointsText", coupon.PointsText ?? "");
                            command.Parameters.AddWithValue("@BorderColor", coupon.BorderColor ?? "");
                            command.Parameters.AddWithValue("@PictureWidth", coupon.PictureWidth);
                        }


                        await command.ExecuteNonQueryAsync();
                    }
                    await transaction.CommitAsync();
                }
            }
            catch (Exception)
            {

            }

        }

        public static async Task SaveRecommendationsAsync(List<ApiRecommendation> recommendations, SQLiteAsyncConnection database)
        {
            if (recommendations == null || !recommendations.Any())
            {
                // If the list is null or empty, we don't need to do anything
                return;
            }

            await database.RunInTransactionAsync((SQLiteConnection conn) =>
            {
                UserRepository ur = new UserRepository(DbBootstraper.Connection);
                var user = ur.GetCurrentUser();
                string userId = user?.UserId;

                if (!string.IsNullOrEmpty(userId))
                {
                    // Get all RecommendationIds for the current user
                    var userRecommendationIds = conn.Table<Recommendation>()
                        .Where(r => r.UserId == userId)
                        .Select(r => r.RecommendationId)
                        .ToList();

                    // Delete RecommendedProducts associated with the user's recommendations
                    conn.Table<RecommendedProduct>()
                        .Delete(rp => userRecommendationIds.Contains(rp.RecommendationId));

                    // Delete Recommendations for the specific userId
                    conn.Table<Recommendation>().Delete(r => r.UserId == userId);
                }

                foreach (var recommendation in recommendations)
                {
                    // Insert into Recommendation table
                    var newRecommendation = new Recommendation
                    {
                        UserId = recommendation.UserId,
                        ProductId = recommendation.ProductId
                    };
                    conn.Insert(newRecommendation);

                    // Get the last inserted ID
                    var lastInsertId = newRecommendation.RecommendationId;

                    // Insert into RecommendedProduct table
                    foreach (var recommendedProductId in recommendation.RecommendedProductIds)
                    {
                        var newRecommendedProduct = new RecommendedProduct
                        {
                            RecommendationId = lastInsertId,
                            RecommendedProductId = recommendedProductId
                        };
                        conn.Insert(newRecommendedProduct);
                    }
                }
            });
        }


        public static async Task SaveDefaultRecommendationsAsync(List<ApiRecommendation> recommendations, SQLiteAsyncConnection database)
        {
            if (recommendations == null || !recommendations.Any())
            {
                // If the list is null or empty, we don't need to do anything
                return;
            }

            await database.RunInTransactionAsync((SQLiteConnection conn) =>
            {

                string userId = "default";

                if (!string.IsNullOrEmpty(userId))
                {
                    // Get all RecommendationIds for the current user
                    var userRecommendationIds = conn.Table<Recommendation>()
                        .Where(r => r.UserId == userId)
                        .Select(r => r.RecommendationId)
                        .ToList();

                    // Delete RecommendedProducts associated with the user's recommendations
                    conn.Table<RecommendedProduct>()
                        .Delete(rp => userRecommendationIds.Contains(rp.RecommendationId));

                    // Delete Recommendations for the specific userId
                    conn.Table<Recommendation>().Delete(r => r.UserId == userId);
                }

                foreach (var recommendation in recommendations)
                {
                    // Insert into Recommendation table
                    var newRecommendation = new Recommendation
                    {
                        UserId = recommendation.UserId,
                        ProductId = recommendation.ProductId
                    };
                    conn.Insert(newRecommendation);

                    // Get the last inserted ID
                    var lastInsertId = newRecommendation.RecommendationId;

                    // Insert into RecommendedProduct table
                    foreach (var recommendedProductId in recommendation.RecommendedProductIds)
                    {
                        var newRecommendedProduct = new RecommendedProduct
                        {
                            RecommendationId = lastInsertId,
                            RecommendedProductId = recommendedProductId
                        };
                        conn.Insert(newRecommendedProduct);
                    }
                }
            });
        }
    }

        public class ApiRecommendation
    {
        public string UserId { get; set; }
        public int ProductId { get; set; }
        public List<int> RecommendedProductIds { get; set; }
    }
}
