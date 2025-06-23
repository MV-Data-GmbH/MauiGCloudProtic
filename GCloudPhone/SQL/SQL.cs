using SQLite; // SQLite ORM biblioteka za sinhrone operacije (u kombinaciji sa SQLiteAsyncConnection)
using System; // Osnovni .NET tipovi, npr. DateTime, Environment
using System.Collections.Generic; // Generičke kolekcije (npr. List<T>)
using System.Linq; // LINQ (Language Integrated Query) za upite nad kolekcijama
using System.Text; // Klase za rad sa tekstom
using System.Threading.Tasks; // Podrška za asinhrono programiranje (Task, async/await)
using Microsoft.Data.Sqlite; // Microsoft-ov SQLite provider za direktan rad sa SQL upitima
using System.Linq.Expressions; // Omogućava kreiranje i evaluaciju izraznih stabala (npr. za lambda izraze)
using GCloud.Shared.Dto.Domain; // Projekt-specifični DTO objekti za domenske entitete
using GCloudShared.Repository; // Repozitorijum klase za pristup podacima u projektu
using GCloudShared.Shared; // Deljene (shared) klase i pomoćne metode unutar projekta
using GCloudPhone.Models;
using System.Diagnostics; // Modeli specifični za GCloudPhone aplikaciju

namespace GCloudPhone
{
    // Klasa SQL sadrži statičke metode za rad sa SQLite bazom "Jetorder.db"
    public class SQL
    {
        // Definiše putanju do baze u lokalnom skladištu aplikacije
        public static string databasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Jetorder.db");
        // Kreira asinhronu SQLite konekciju za rad sa bazom podataka
        public static SQLiteAsyncConnection _database = new SQLiteAsyncConnection(databasePath);

        // Inicijalizuje bazu kreiranjem potrebnih tabela ako one ne postoje
        public static async Task InitializeAsync()
        {
            

            // Kreira tabele za različite entitete
            await _database.CreateTableAsync<Categories>();
            await _database.CreateTableAsync<Groups>();
            await _database.CreateTableAsync<Pictures>();
            await _database.CreateTableAsync<Prices>();
            await _database.CreateTableAsync<Prices_Type>();
            await _database.CreateTableAsync<Products>();
            await _database.CreateTableAsync<Products_SD>();
            await _database.CreateTableAsync<SDGroups>();
            await _database.CreateTableAsync<SDPages>();
            await _database.CreateTableAsync<Sidedishes>();
            await _database.CreateTableAsync<VAT>();
            await _database.CreateTableAsync<Orders>();
            await _database.CreateTableAsync<OrderItems>();
            await _database.CreateTableAsync<Parameters>();
            await _database.CreateTableAsync<TimeStampTable>();
            await _database.CreateTableAsync<Stores>();
            await _database.CreateTableAsync<StoreOpeningHours>();
            await _database.CreateTableAsync<Recommendation>();
            await _database.CreateTableAsync<RecommendedProduct>();
            await _database.CreateTableAsync<Addresses>();
            await _database.CreateTableAsync<Coupons>();
            await _database.CreateTableAsync<StaticPicture>();
            await _database.CreateTableAsync<PushNotifications>();
        }

        // Generička metoda koja vraća sve stavke iz tabele tipa T
        public static async Task<List<T>> GetItemsAsync<T>() where T : new()
        {
            // Dohvata sve zapise iz tabele T i pretvara ih u listu
            var rt = await _database.Table<T>().ToListAsync();
            return rt;
        }

        // Generička metoda koja vraća sve stavke tipa T koje zadovoljavaju uslov (predicate)
        public static async Task<List<T>> GetItemsAsync<T>(Expression<Func<T, bool>> predicate) where T : new()
        {
            // Filtrira zapise tabele T prema zadatom predicate-u
            var rt = await _database.Table<T>().Where(predicate).ToListAsync();
            return rt;
        }

        // Generička metoda koja vraća jednu stavku tipa T na osnovu njenog ID-ja
        public static async Task<T> GetItemAsync<T>(int id) where T : new()
        {
            // Pronalazi zapise sa datim ID-jem
            return await _database.FindAsync<T>(id);
        }

        // Generička metoda koja vraća prvu stavku tipa T koja zadovoljava uslov (predicate)
        public static async Task<T> GetItemAsync<T>(Expression<Func<T, bool>> predicate) where T : new()
        {
            var s = await _database.Table<T>().FirstOrDefaultAsync(predicate);
            return s;
        }

        // Generička metoda za dodavanje nove stavke u bazu
        public static async Task<int> SaveItemAsync<T>(T item)
        {
            return await _database.InsertAsync(item);
        }

        // Generička metoda za ažuriranje postojeće stavke u bazi
        public static async Task<int> UpdateItemAsync<T>(T item)
        {
            return await _database.UpdateAsync(item);
        }

        // Generička metoda za brisanje stavke iz baze
        public static async Task<int> DeleteItemAsync<T>(T item)
        {
            return await _database.DeleteAsync(item);
        }

        // Metoda koja vraća listu proizvoda (ProductsView) filtriranih po ID-ju kategorije
        public static async Task<List<ProductsView>> GetProductsByCategoryIdAsync(int categoryId)
        {
            var productsList = new List<ProductsView>(); // Lista za čuvanje rezultata
            var databasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Jetorder.db");
            try
            {
                // Kreira novu SQL konekciju pomoću Microsoft.Data.Sqlite
                using (var connection = new SqliteConnection($"Data Source={databasePath}"))
                {
                    await connection.OpenAsync(); // Otvara konekciju asinhrono
                    var command = connection.CreateCommand(); // Kreira SQL komandu

                    // Određuje tip cene na osnovu trenutnog tipa porudžbine (OrderType)
                    int priceType = 1; // Podrazumevani tip: Delivery
                    if (App.OrderType == "Delivery") priceType = 5;
                    if (App.OrderType == "PickUp") priceType = 6;
                    if (App.OrderType == "DineIn") priceType = 1;
                    if (App.OrderType == "Parking") priceType = 4;

                    // SQL upit sa spajanjem (JOIN) tabela: Products, Pictures, Prices, Categories i VAT
                    command.CommandText = @"
                        SELECT p.Number, 
                               p.Description1, 
                               p.Description2, 
                               p.Categories, 
                               p.Sort,
                               pic.Picturestring, 
                               pr.Amount AS PriceAmount, 
                               vat.Value AS VAT
                        FROM Products p
                        LEFT JOIN Pictures pic ON p.Pictures = pic.Number
                        LEFT JOIN Prices pr ON p.Number = pr.Products AND pr.PricesType = @PriceType
                        LEFT JOIN Categories cat ON p.Categories = cat.Number
                        LEFT JOIN VAT vat ON cat.VAT = vat.Number
                        WHERE p.Categories = @CategoryId
                        ORDER BY p.Sort";

                    // Postavlja parametre upita
                    command.Parameters.AddWithValue("@CategoryId", categoryId);
                    command.Parameters.AddWithValue("@PriceType", priceType);

                    // Izvršava upit i čita rezultate
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            // Kreira instancu ProductsView i popunjava je podacima iz rezultata upita
                            var product = new ProductsView
                            {
                                Number = reader.GetInt32(reader.GetOrdinal("Number")),
                                Description1 = reader.IsDBNull(reader.GetOrdinal("Description1")) ? null : reader.GetString(reader.GetOrdinal("Description1")),
                                // Napomena: Provera za Description2 koristi "Description1" kao uslov – proveriti ispravnost
                                Description2 = reader.IsDBNull(reader.GetOrdinal("Description1")) ? null : reader.GetString(reader.GetOrdinal("Description2")),
                                Categories = reader.IsDBNull(reader.GetOrdinal("Categories")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("Categories")),
                                Picturestring = reader.IsDBNull(reader.GetOrdinal("Picturestring")) ? null : reader.GetString(reader.GetOrdinal("Picturestring")),
                                PriceAmount = reader.IsDBNull(reader.GetOrdinal("PriceAmount")) ? 0m : reader.GetDecimal(reader.GetOrdinal("PriceAmount")),
                                VAT = reader.IsDBNull(reader.GetOrdinal("VAT")) ? 0m : reader.GetDecimal(reader.GetOrdinal("VAT"))
                            };
                            productsList.Add(product);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // U slučaju greške, ispisuje poruku u konzolu
                Console.WriteLine($"Database error: {ex.Message}");
            }
            return productsList;
        }

        // Metoda koja vraća listu preporučenih proizvoda (ProductsView) na osnovu proizvoda u korpi
        public static async Task<List<ProductsView>> GetRecommendationsAsync(List<int> productsFromCart)
        {
            // Dobija listu jedinstvenih preporučenih proizvoda na osnovu proizvoda iz korpe
            List<int> recommendedProducts = await GetDistinctRecommendationsForCartAsync(productsFromCart);
            var productsList = new List<ProductsView>();
            var databasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Jetorder.db");

            try
            {
                // Otvara novu SQL konekciju
                using (var connection = new SqliteConnection($"Data Source={databasePath}"))
                {
                    await connection.OpenAsync();
                    // Učitava sve potrebne podatke iz tabela
                    var products = await _database.Table<Products>().ToListAsync();
                    var pictures = await _database.Table<Pictures>().ToListAsync();
                    var prices = await _database.Table<Prices>().ToListAsync();
                    var categories = await _database.Table<Categories>().ToListAsync();
                    var vats = await _database.Table<VAT>().ToListAsync();
                    // Koristi LINQ upit za filtriranje i spajanje podataka
                    productsList = (from p in products
                                    where recommendedProducts.Contains(p.Number)
                                    let pic = pictures.FirstOrDefault(pic => pic.Number == p.Pictures)
                                    let pr = prices.FirstOrDefault(pr => pr.Products == p.Number && pr.PricesType == 1)
                                    let cat = categories.FirstOrDefault(c => c.Number == p.Categories)
                                    let vat = cat != null ? vats.FirstOrDefault(v => v.Number == cat.VAT) : null
                                    select new ProductsView
                                    {
                                        Number = p.Number,
                                        Description1 = p.Description1,
                                        Description2 = p.Description2,
                                        Categories = p.Categories,
                                        Picturestring = pic?.Picturestring,
                                        PriceAmount = pr?.Amount ?? 0m,
                                        VAT = vat?.Value ?? 0m
                                    }).ToList();
                }
            }
            catch (Exception ex)
            {
                // Ispis greške u slučaju problema
                Console.WriteLine($"An error occurred: {ex.Message}");
            }

            return productsList;
        }

        // Metoda koja vraća listu proizvoda povezanu sa kuponima
        public static async Task<List<ProductsView>> GetCouponProductsAsync()
        {
            var productsList = new List<ProductsView>(); // Lista za cuvanje proizvoda sa kuponima
            var databasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Jetorder.db");
            try
            {
                // Otvara SQL konekciju
                using (var connection = new SqliteConnection($"Data Source={databasePath}"))
                {
                    await connection.OpenAsync();
                    var command = connection.CreateCommand();
                    // SQL upit koji spaja tabele Products, Coupons, Pictures, Categories i VAT
                    command.CommandText = @"
                               SELECT p.Number, 
                                      p.Description1, 
                                      p.Description2, 
                                      p.Categories, 
                                      pic.Picturestring, 
                                      0 AS PriceAmount, 
                                      vat.Value AS VAT,
                                      c.Value AS CouponValue 
                               FROM Products AS p
                               INNER JOIN Coupons AS c ON p.Number = c.ArticleNumber
                               LEFT OUTER JOIN Pictures AS pic ON p.Pictures = pic.Number
                               LEFT JOIN Categories cat ON p.Categories = cat.Number
                               LEFT JOIN VAT vat ON cat.VAT = vat.Number";

                    // Čitanje rezultata upita
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            // Kreira instancu ProductsView za svaki red
                            var product = new ProductsView
                            {
                                Number = reader.GetInt32(reader.GetOrdinal("Number")),
                                Description1 = reader.IsDBNull(reader.GetOrdinal("Description1")) ? null : reader.GetString(reader.GetOrdinal("Description1")),
                                Description2 = reader.IsDBNull(reader.GetOrdinal("Description2")) ? null : reader.GetString(reader.GetOrdinal("Description2")),
                                Categories = reader.IsDBNull(reader.GetOrdinal("Categories")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("Categories")),
                                Picturestring = reader.IsDBNull(reader.GetOrdinal("Picturestring")) ? null : reader.GetString(reader.GetOrdinal("Picturestring")),
                                PriceAmount = reader.IsDBNull(reader.GetOrdinal("PriceAmount")) ? 0m : reader.GetDecimal(reader.GetOrdinal("PriceAmount")),
                                VAT = reader.IsDBNull(reader.GetOrdinal("VAT")) ? 0m : reader.GetDecimal(reader.GetOrdinal("VAT")),
                                IsCoupon = true, // Obeležava da je proizvod deo kupona
                                CouponValue = reader.IsDBNull(reader.GetOrdinal("CouponValue")) ? 0m : reader.GetDecimal(reader.GetOrdinal("CouponValue"))
                            };
                            productsList.Add(product);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Trenutno se greške ne loguju – razmotriti dodavanje logovanja
            }
            return productsList;
        }

        // Metoda koja vraća listu dodatnih proizvoda (Products_SD) za dati proizvod (prema njegovom ID-ju)
        public static async Task<List<Products_SD>> GetProductSDsByProductIdAsync(int productId)
        {
            return await _database.Table<Products_SD>().Where(psd => psd.Products == productId).ToListAsync();
        }

        // Metoda koja vraća listu side dish stavki (SidedishesView) za dati proizvod
        public static async Task<List<SidedishesView>> GetSidedishesByProductIdAsync(int productId)
        {
            var sidedishesList = new List<SidedishesView>(); // Lista za sidedishes
            var databasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Jetorder.db");
            try
            {
                using (var connection = new SqliteConnection($"Data Source={databasePath}"))
                {
                    await connection.OpenAsync();
                    var command = connection.CreateCommand();
                    // SQL upit koji spaja tabele za sidedishes, cene, strane, grupe, i VAT
                    command.CommandText = @" SELECT s.Number, 
                                       s.Description1, 
                                       s.Categories, 
                                       p.Picturestring, 
                                       s.Sort,
                                       pr.Amount AS PriceAmount, 
                                       SDPages.Description,
                                       psd.PageNumber, 
                                       psd.SDGroups, 
                                       SDGroups.Min, 
                                       SDGroups.Max, 
                                       v.Value AS VAT
                                FROM Categories c
                                LEFT JOIN Sidedishes AS s ON c.Number = s.Categories
                                LEFT JOIN VAT v ON c.VAT = v.Number
                                INNER JOIN Products_SD AS psd ON s.Number = psd.Sidedisches
                                INNER JOIN SDGroups ON psd.SDGroups = SDGroups.Number
                                INNER JOIN SDPages ON psd.PageNumber = SDPages.Number
                                LEFT OUTER JOIN Pictures AS p ON s.Pictures = p.Number
                                LEFT OUTER JOIN Prices AS pr ON s.Number = pr.Sidedishes AND pr.PricesType = 1
                                WHERE psd.Products = @ProductId
                                ORDER BY s.Sort ASC";

                    command.Parameters.AddWithValue("@ProductId", productId);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            // Kreira sidedish objekat i popunjava podatke iz rezultata upita
                            var sidedish = new SidedishesView
                            {
                                Number = reader.GetInt32(reader.GetOrdinal("Number")),
                                Description1 = reader.IsDBNull(reader.GetOrdinal("Description1")) ? null : reader.GetString(reader.GetOrdinal("Description1")),
                                PriceAmount = reader.IsDBNull(reader.GetOrdinal("PriceAmount")) ? 0m : reader.GetDecimal(reader.GetOrdinal("PriceAmount")),
                                Categories = reader.IsDBNull(reader.GetOrdinal("Categories")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("Categories")),
                                Picturestring = reader.IsDBNull(reader.GetOrdinal("Picturestring")) ? (string)null : reader.GetString(reader.GetOrdinal("Picturestring")),
                                PageNumber = reader.IsDBNull(reader.GetOrdinal("PageNumber")) ? 0 : reader.GetInt32(reader.GetOrdinal("PageNumber")),
                                VAT = reader.IsDBNull(reader.GetOrdinal("VAT")) ? 0m : reader.GetDecimal(reader.GetOrdinal("VAT")),
                                Group = reader.GetInt32(reader.GetOrdinal("SDGroups"))
                            };
                            sidedishesList.Add(sidedish);
                            Console.WriteLine($"Raw side dish: {sidedish.Description1}, PageNumber: {sidedish.PageNumber}, PriceAmount: {sidedish.PriceAmount}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Ispisuje grešku u slučaju izuzetka
                var m = ex.Message;
                Console.WriteLine(m);
            }
            return sidedishesList;
        }

        // Overload metoda – vraća sidedishes za dati proizvod i određenu stranicu (page number)
        public static async Task<List<SidedishesView>> GetSidedishesByProductIdAsync(int productId, int pageNumber)
        {
            var sidedishesList = new List<SidedishesView>();
            var databasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Jetorder.db");
            try
            {
                using (var connection = new SqliteConnection($"Data Source={databasePath}"))
                {
                    await connection.OpenAsync();
                    var command = connection.CreateCommand();
                    // SQL upit sa dodatnim uslovom za broj stranice
                    command.CommandText = @"SELECT s.Number, 
                               s.Description1, 
                               s.Categories, 
                               p.Picturestring, 
                               pr.Amount AS PriceAmount,
                               psd.PageNumber, 
                               v.Value AS VAT
                        FROM Categories c
                        LEFT JOIN Sidedishes s ON c.Number = s.Categories
                        LEFT JOIN VAT v ON c.VAT = v.Number
                        INNER JOIN Products_SD psd ON s.Number = psd.Sidedisches
                        LEFT OUTER JOIN Pictures p ON s.Pictures = p.Number
                        LEFT OUTER JOIN Prices pr ON s.Number = pr.Sidedishes AND pr.PricesType = 1
                        WHERE psd.Products = @ProductId AND psd.PageNumber = @PageNumber";
                    command.Parameters.AddWithValue("@ProductId", productId);
                    command.Parameters.AddWithValue("@PageNumber", pageNumber);
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var sidedish = new SidedishesView
                            {
                                Number = reader.GetInt32(reader.GetOrdinal("Number")),
                                Description1 = reader.IsDBNull(reader.GetOrdinal("Description1")) ? null : reader.GetString(reader.GetOrdinal("Description1")),
                                PriceAmount = reader.IsDBNull(reader.GetOrdinal("PriceAmount")) ? 0m : reader.GetDecimal(reader.GetOrdinal("PriceAmount")),
                                Categories = reader.IsDBNull(reader.GetOrdinal("Categories")) ? (int?)null : reader.GetInt32(reader.GetOrdinal("Categories")),
                                Picturestring = reader.IsDBNull(reader.GetOrdinal("Picturestring")) ? (string)null : reader.GetString(reader.GetOrdinal("Picturestring")),
                                PageNumber = reader.GetInt32(reader.GetOrdinal("PageNumber")),
                                VAT = reader.IsDBNull(reader.GetOrdinal("VAT")) ? 0m : reader.GetDecimal(reader.GetOrdinal("VAT"))
                            };
                            sidedishesList.Add(sidedish);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetSidedishesByProductIdAsync: {ex.Message}");
            }
            return sidedishesList;
        }

        // Metoda koja vraća sve vidljive kategorije zajedno sa slikom, sortirane po vrednosti "sort"
        public static async Task<List<CategoriesView>> GetAllCategoriesAsync()
        {
            var categoriesList = new List<CategoriesView>();
            var databasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Jetorder.db");
            try
            {
                using (var connection = new SqliteConnection($"Data Source={databasePath}"))
                {
                    await connection.OpenAsync();
                    var command = connection.CreateCommand();
                    // SQL upit koji vraća kategorije koje su označene kao vidljive
                    command.CommandText = @"
                    SELECT c.Number, c.Description1, pic.Picturestring
                    FROM Categories c
                    LEFT JOIN Pictures pic ON c.Pictures = pic.Number
                    WHERE c.isvisible = 1  
                    ORDER BY c.sort ASC";

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var category = new CategoriesView
                            {
                                Number = reader.GetInt32(reader.GetOrdinal("Number")),
                                Description1 = reader.IsDBNull(reader.GetOrdinal("Description1")) ? null : reader.GetString(reader.GetOrdinal("Description1")),
                                Picturestring = reader.IsDBNull(reader.GetOrdinal("Picturestring")) ? null : reader.GetString(reader.GetOrdinal("Picturestring"))
                            };
                            categoriesList.Add(category);
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Može se dodati logovanje greške
            }
            return categoriesList;
        }

        // Metoda koja vraća sve parametre iz tabele Parameters
        public async static Task<List<Parameters>> GetParametersAsync()
        {
            return await _database.Table<Parameters>().ToListAsync();
        }

        // Metoda koja vraća sve porudžbine iz tabele Orders
        public static async Task<List<Orders>> GetAllOrdersAsync()
        {
            return await _database.Table<Orders>().ToListAsync();
        }

        // Metoda koja vraća porudžbine zajedno sa stavkama i prvom slikom proizvoda (za prikaz)
        public static async Task<List<OrderWithItemsViewModel>> GetOrdersWithItemsAsync()
        {
            var orders = await _database.Table<Orders>().ToListAsync();

            var orderTasks = orders.Select(async order =>
            {
                var orderItems = await _database.Table<OrderItems>()
                    .Where(oi => oi.OrderID == order.OrderID)
                    .ToListAsync();

                string firstProductImage = null;

                if (orderItems.Any())
                {
                    var firstProductId = orderItems.First().ProductID;
                    var firstProduct = await _database.Table<Products>()
                        .FirstOrDefaultAsync(p => p.Number == firstProductId);

                    if (firstProduct != null && firstProduct.Pictures.HasValue)
                    {
                        var pictureId = firstProduct.Pictures.Value;
                        var picture = await _database.Table<Pictures>()
                            .FirstOrDefaultAsync(pic => pic.Number == pictureId);

                        if (picture != null && !string.IsNullOrEmpty(picture.Picturestring))
                        {
                            firstProductImage = picture.Picturestring;
                        }
                    }
                }

                return new OrderWithItemsViewModel
                {
                    Order = order,
                    OrderItems = orderItems,
                    FirstProductImage = firstProductImage
                };
            });

            var orderWithItemsList = await Task.WhenAll(orderTasks);
            return orderWithItemsList.ToList();
        }

        public static async Task<List<OrderWithItemsViewModel>> GetProcessedOrdersAsync()
        {
            var allOrders = await GetOrdersWithItemsAsync();

            var sortedOrders = allOrders.OrderByDescending(o => o.Order.OrderDate).ToList();

            var processedOrders = sortedOrders
                .Where(o => o.OrderItems?.Any() == true)
                .Select(o =>
                {
                    var filteredItems = o.OrderItems
                        .Where(item => !item.IsCoupon)
                        .GroupBy(item => item.ProductID)
                        .Select(g => g.First())
                        .ToList();

                    if (!filteredItems.Any())
                        return null;

                    o.OrderItems = filteredItems;
                    o.OrderItemsHeight = filteredItems.Count * 40;
                    o.FirstProductImageSource = !string.IsNullOrEmpty(o.FirstProductImage)
                        ? ImageSource.FromStream(() => new MemoryStream(Convert.FromBase64String(o.FirstProductImage)))
                        : ImageSource.FromFile("food.png");
                    return o;
                })
                .Where(o => o != null)
                .Cast<OrderWithItemsViewModel>()
                .ToList();

            return processedOrders;
        }


        // Metoda koja vraća najnoviji zapis iz TimeStampTable (poslednji put ažurirano)
        public static async Task<TimeStampTable> GetTimeStampTable()
        {
            var timeStampTable = new TimeStampTable();
            var databasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Jetorder.db");
            try
            {
                using (var connection = new SqliteConnection($"Data Source={databasePath}"))
                {
                    await connection.OpenAsync();
                    var command = connection.CreateCommand();
                    // SQL upit koji vraća najnoviji datum ažuriranja
                    command.CommandText = @"SELECT DataUpdated FROM TimeStampTable ORDER BY DataUpdated DESC LIMIT 1";
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            timeStampTable.DataUpdated = reader.GetDateTime(reader.GetOrdinal("DataUpdated"));
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Greške se trenutno ne loguju – razmotriti dodavanje logovanja
            }
            return timeStampTable;

            // Alternativno, može se koristiti ORM: return await _database.Table<TimeStampTable>().FirstOrDefaultAsync();
        }

        // Metoda koja vraća sve adrese iz tabele Addresses
        public static async Task<List<Addresses>> GetAddressesAsync()
        {
            return await _database.Table<Addresses>().ToListAsync();
        }

        // Metoda koja vraća dužinu vremenskog intervala (TimeSlotLength) i radno vreme za datu prodavnicu
        public static async Task<(int TimeSlotLength, StoreOpeningHours StoreHours)> GetStoreTimeSlot(string storeId)
        {
            // Dohvata zapis iz StoreOpeningHours za dati storeId
            var storeOpeningHours = await _database.Table<StoreOpeningHours>()
                                                   .Where(soh => soh.StoreID == storeId)
                                                   .FirstOrDefaultAsync();

            // Vraća tuple sa TimeSlotLength i celokupnim zapisom o radnom vremenu
            return (storeOpeningHours?.TimeSlotLength ?? 0, storeOpeningHours);
        }

        // Metoda (nije statička) koja vraća sve zapise radnog vremena iz tabele StoreOpeningHours
        public async Task<List<StoreOpeningHours>> GetAllStoredOpeningHours()
        {
            return await _database.Table<StoreOpeningHours>().ToListAsync();
        }

        // Metoda koja vraća sve prodavnice iz tabele Stores
        public static async Task<List<Stores>> GetAllStoresAsync()
        {
            try
            {
                Debug.WriteLine("[GetAllStoresAsync] Pozivam _database.Table<Stores>().ToListAsync()");

                // Preuzmi sve iz tabele
                var list = await _database.Table<Stores>().ToListAsync();

                if (list == null || list.Count == 0)
                {
                    Debug.WriteLine("[GetAllStoresAsync] Lista je prazna ili null");
                }
                else
                {
                    Debug.WriteLine($"[GetAllStoresAsync] Dobijeno {list.Count} zapisa:");
                    foreach (var store in list)
                    {
                        // Ovde ispiši sve relevantne property-je iz Stores
                        Debug.WriteLine($"[GetAllStoresAsync] ► Id = {store.Id}, Name = {store.Name}, City = {store.City}, Street = {store.Street}, HouseNr = {store.HouseNr}, PLZ = {store.Plz}, Latitude = {store.Latitude}, Longitude = {store.Longitude}");
                    }
                }

                return list;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[GetAllStoresAsync] EXCEPTION: {ex.Message}");
                Debug.WriteLine(ex.StackTrace);
                throw; // prosledi dalje ili vrati novu grešku
            }
        }


        // Metoda koja vraća prodavnicu na osnovu njenog ID-ja
        public static async Task<Stores> GetStoreByID(string storeId)
        {
            try
            {
                return await _database.FindAsync<Stores>(storeId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching store by ID: {ex.Message}");
                return null;
            }
        }

        // Metoda koja vraća sve podatke o PDV-u (VAT) iz tabele VAT
        public static async Task<List<VAT>> GetAllVat()
        {
            return await _database.Table<VAT>().ToListAsync();
        }

        // Metoda koja vraća jedinstvenu listu preporučenih proizvoda na osnovu proizvoda u korpi
        public static async Task<List<int>> GetDistinctRecommendationsForCartAsync(List<int> cartProductIds)
        {
            // Kreira instance repozitorijuma za korisnika i parametre
            UserRepository ur = new UserRepository(DbBootstraper.Connection);
            ParametersRepository pr = new ParametersRepository(DbBootstraper.Connection);
            var user = ur.GetCurrentUser();
            string userId = user?.UserId ?? "default";
            var distinctRecommendations = new HashSet<int>(); // Koristi se HashSet da bi se izbegle duplikacije

            // Prvo, dohvata kategorije proizvoda iz korpe
            var cartProductCategories = await _database.Table<Products>()
                .Where(p => cartProductIds.Contains(p.Number))
                .ToListAsync()
                .ContinueWith(t => t.Result
                    .Select(p => p.Categories)
                    .Distinct()
                    .ToList());

            foreach (var productId in cartProductIds)
            {
                // Dohvata preporuke za svaki proizvod za datog korisnika
                var recommendations = await _database.Table<Recommendation>()
                    .Where(r => r.ProductId == productId && r.UserId == userId)
                    .ToListAsync();

                // Ako nije pronađena nijedna preporuka za korisnika, pokušava sa "default" korisnikom
                if (!recommendations.Any() && userId != "default")
                {
                    recommendations = await _database.Table<Recommendation>()
                        .Where(r => r.ProductId == productId && r.UserId == "default")
                        .ToListAsync();
                }

                foreach (var recommendation in recommendations)
                {
                    // Za svaku preporuku, dohvata preporučene proizvode
                    var recommendedProducts = await _database.Table<RecommendedProduct>()
                        .Where(rp => rp.RecommendationId == recommendation.RecommendationId)
                        .ToListAsync();

                    // Dodaje ID-jeve preporučenih proizvoda u skup
                    foreach (var rp in recommendedProducts)
                    {
                        distinctRecommendations.Add(rp.RecommendedProductId);
                    }
                }
            }

            // Uklanja proizvode koji su već u korpi
            distinctRecommendations.ExceptWith(cartProductIds);

            // Uklanja preporučene proizvode koji pripadaju istim kategorijama kao proizvodi u korpi
            var recommendedProductsToRemove = (await _database.Table<Products>()
                .Where(p => distinctRecommendations.Contains(p.Number) &&
                            cartProductCategories.Contains(p.Categories))
                .ToListAsync())
                .Select(p => p.Number)
                .ToList();

            distinctRecommendations.ExceptWith(recommendedProductsToRemove);

            // Ograničava broj preporuka na 10
            if (distinctRecommendations.Count > 10)
            {
                while (distinctRecommendations.Count > 10)
                {
                    distinctRecommendations.Remove(distinctRecommendations.First());
                }
            }

            return distinctRecommendations.ToList();
        }

        // Metoda koja briše adresu iz tabele Addresses na osnovu RemoteID-ja
        public static async Task<int> DeleteAddressByRemoteIDAsync(string remoteID)
        {
            try
            {
                // SQL upit za brisanje adrese sa zadatim RemoteID-jem
                var query = $"DELETE FROM Addresses WHERE RemoteID = ?";
                return await _database.ExecuteAsync(query, remoteID);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error deleting address with RemoteID {remoteID}: {ex.Message}");
                return 0; // Vraća 0 u slučaju neuspeha
            }
        }

        // Metoda koja vraća porudžbine kreirane u poslednjem satu
        public static async Task<List<Orders>> GetOrdersFromLastHourAsync()
        {
            // Izračunava vreme pre jednog sata
            var oneHourAgo = DateTime.Now.AddHours(-1);
            return await _database.Table<Orders>()
                .Where(order => order.OrderDate >= oneHourAgo)
                .ToListAsync();
        }

        // Metoda koja vraća sve stavke porudžbine na osnovu ID-ja porudžbine
        public static async Task<List<OrderItems>> GetOrderItemsAsync(string orderId)
        {
            return await _database.Table<OrderItems>()
                                  .Where(item => item.OrderID == orderId)
                                  .ToListAsync();
        }
    }
}
