using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using GCloud.Shared.Dto.Domain;

namespace GCloudPhone
{
    public class Categories
    {
        [PrimaryKey]
        public int Number { get; set; }
        public string Description1 { get; set; }
        public string Description2 { get; set; }
        public string Description3 { get; set; }
        public int? VAT { get; set; }
        public int? Groups { get; set; }
        public int? Pictures { get; set; }
        public string Reference { get; set; }
        public int Sort { get; set; }
        public int IsVisible { get; set; }
    }

    public class Groups
    {
        [PrimaryKey]
        public int Number { get; set; }
        public string Description1 { get; set; }
        public string Description2 { get; set; }
        public string Description3 { get; set; }
    }

    public class Pictures
    {
        [PrimaryKey]
        public int Number { get; set; }
        //public int Idc { get; set; }
        public string Picturestring { get; set; }
        public string Description { get; set; }
    }

    public class Prices
    {
        [PrimaryKey, AutoIncrement]
        public int Idc { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public int? Products { get; set; }
        public int? Sidedishes { get; set; }
        public int? PricesType { get; set; }
    }

    public class Prices_Type
    {
        [PrimaryKey]
        public int Number { get; set; }
        public string Description { get; set; }
    }

    public class Products
    {
        [PrimaryKey]
        public int Number { get; set; }
        public string Description1 { get; set; }
        public string Description2 { get; set; }
        public string Description3 { get; set; }
        public int? Prices { get; set; }
        public int? Categories { get; set; }
        public int? Pictures { get; set; }
        //public int? PicturesBanner { get; set; }
        public int Sort { get; set; }
    }

    public class Products_SD
    {
        [PrimaryKey, AutoIncrement]
        public int Idc { get; set; }
        public int? Products { get; set; }
        public int? Sidedisches { get; set; }
        public int? SDGroups { get; set; }
        public int? PageNumber { get; set; }
    }

    public class SDGroups
    {
        [PrimaryKey]
        public int Number { get; set; }
        public int Idc { get; set; }
        public int? Min { get; set; }
        public int? Max { get; set; }
    }

    public class SDPages
    {
        [PrimaryKey]
        public int Number { get; set; }
        public string Description { get; set; }
    }

    public class Sidedishes
    {
        [PrimaryKey]
        public int Number { get; set; }
        public string Description1 { get; set; }
        public string Description2 { get; set; }
        public string Description3 { get; set; }
        public int? Prices { get; set; }
        public int? Categories { get; set; }
        public int? Pictures { get; set; }
        public int Sort { get; set; }
    }

    public class VAT
    {
        [PrimaryKey]
        public int Number { get; set; }
        public string Description1 { get; set; }
        public string Description2 { get; set; }
        public decimal? Value { get; set; }
        public string Reference { get; set; }
    }

    public class OrderItems
    {
        [PrimaryKey, AutoIncrement]
        public int Idc { get; set; }
        public string OrderID { get; set; }
        public int ProductID { get; set; }
        public string ProductDescription1 { get; set; }
        public string ProductDescription2 { get; set; }
        public decimal? Amount { get; set; }
        public int? Quantity { get; set; }
        public decimal? VAT { get; set; }
        public string Reference { get; set; }
        public string ItemNote { get; set; }

        // Dodata svojstva za kupon
        public bool IsCoupon { get; set; }
        public decimal CouponValue { get; set; }
    }

    public class Orders
    {
        [PrimaryKey]
        public string OrderID { get; set; }
        public string UserID { get; set; }
        public string StoreID { get; set; }
        public DateTime OrderDate { get; set; }
        public DateTime DeliveryDate { get; set; }
        public string DeliveryTime { get; set; }
        public string DeliveryAddress { get; set; }
        public string DeliveryCity { get; set; }
        public string DeliveryZip { get; set; }
        public string DeliveryCountry { get; set; }
        public string DeliveryPhone { get; set; }
        public string DeliveryEmail { get; set; }
        public string DeliveryContact { get; set; }
        public string DeliveryNotes { get; set; }
        public string PaymentMethod { get; set; }
        public string PaymentStatus { get; set; }
        public string OrderStatus { get; set; }
        public decimal? TotalAmount { get; set; }
        public decimal? TotalVAT { get; set; }
        public string OrderType { get; set; }
        public string Reference { get; set; }
        public decimal? Tip { get; set; }
    }

    public class OrderWithItems
    {
        public Orders Order { get; set; }
        public List<OrderItems> Items { get; set; }
    }

    public class Parameters
    {
        [PrimaryKey]
        public string Parameter { get; set; }
        public string Value { get; set; }
    }

    public class TimeStampTable
    {
        public DateTime DataUpdated { get; set; }
    }

    public class StoreOpeningHours
    {
        [PrimaryKey]
        public string StoreID { get; set; }
        public int DayOfWeek { get; set; }
        public string OpenFrom { get; set; }
        public string OpenTo { get; set; }
        public int TimeSlotLength { get; set; }
        public int OrdersInTimeSlot { get; set; }
    }

    public class Stores
    {
        [PrimaryKey]
        public string Id { get; set; } // Store GUID as TEXT
        public string Name { get; set; }
        public string City { get; set; }
        public string Street { get; set; }
        public string HouseNr { get; set; }
        public string Plz { get; set; }
        public DateTime CreationDateTime { get; set; }
        public string ApiToken { get; set; }
        public string CompanyId { get; set; } // Company GUID as TEXT
        public string CountryId { get; set; } // Country GUID as TEXT
        public bool IsDeleted { get; set; }
        public float Latitude { get; set; }
        public float Longitude { get; set; }
        public string EBillCategory_Id { get; set; } // EBillCategory GUID as TEXT
        public int PointsPerEuro { get; set; }
        public decimal EuroPerPoints { get; set; }
        public string SpecialProduct_Id { get; set; } // SpecialProduct GUID as TEXT
        public string StoreWebSite { get; set; }
        public string CashRegisterName { get; set; }
        public string IPAddress { get; set; }
        public string ShortId { get; set; }
        public int StoreGroup {  get; set; }
    }

    public class Recommendation
    {
        [PrimaryKey, AutoIncrement]
        public int RecommendationId { get; set; } // Auto-incrementing primary key
        public string UserId { get; set; } // To store UserId
        public int ProductId { get; set; } // To store ProductId
    }
    public class RecommendedProduct
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; } // Auto-incrementing primary key for SQLite
        public int RecommendationId { get; set; } // Foreign Key to Recommendations table
        public int RecommendedProductId { get; set; } // One RecommendedProductId
    }

    public class Addresses
    {
        public string UserID { get; set; }
        public string Name { get; set; }
        public string MiddleName { get; set; }
        public string SurName { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string AddressLine3 { get; set; }
        public string City { get; set; }
        public string Zip { get; set; }
        public string Country { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string ContactPerson { get; set; }
        public string Notes { get; set; }
        public string AddressType { get; set; }
        public int IsDefault { get; set; } = 0;
        public string RemoteID { get; set; }
    }

    public class Coupons
    {
        [PrimaryKey]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string ShortDescription { get; set; }
        public int? MaxRedeems { get; set; }
        public int? RedeemsLeft { get; set; }
        public DateTime? ValidFrom { get; set; }
        public DateTime? ValidTo { get; set; }
        public decimal Value { get; set; }
        public int CouponType { get; set; }
        public int CouponScope { get; set; }
        public int? ArticleNumber { get; set; }
        public bool IsValid { get; set; }
        public string IconBase64 { get; set; }
        public string CouponPoints { get; set; }
        public string TextColor { get; set; }
        public string PointsText { get; set; }
        public string BorderColor { get; set; }
        public double PictureWidth { get; set; }
    }

    public class StaticPicture
    {
        [PrimaryKey]
        public int Id { get; set; }
        public string PictureString { get; set; }
    }
}
