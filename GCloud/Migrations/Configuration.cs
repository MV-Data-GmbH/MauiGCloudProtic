using System.Collections.Generic;
using System.IO;
using GCloud.Migrations;
using GCloud.Models.Domain;
using GCloud.Models.Domain.CouponUsageAction;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using WebGrease.Css.Extensions;

namespace GCloud.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<GCloud.Models.Domain.GCloudContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            AutomaticMigrationDataLossAllowed = false;
            MigrationsNamespace = "GCloud.Models.Domain";
        }

        protected override void Seed(GCloud.Models.Domain.GCloudContext context)
        {
            if(!context.Countries.Any())
            {
                var countries = CountryBuilder.GetCountries();
                countries.ForEach(x => context.Countries.Add(x));
                context.SaveChanges();

                var austria = countries.FirstOrDefault(x => x.Name == "Austria");

                //Roles setup
                var adminRole = new IdentityRole("Administrators");
                var managerRole = new IdentityRole("Managers");
                var customerRole = new IdentityRole("Customers");

                var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
                roleManager.Create(adminRole);
                roleManager.Create(managerRole);
                roleManager.Create(customerRole);

                //User setup
                var userStore = new UserStore<User>(context);
                var userManager = new UserManager<User>(userStore);

                var manager = new User()
                {
                    Email = "manager@mvdata.at",
                    IsActive = true,
                    UserName = "manager",
                    FirstName = "Dominik",
                    LastName = "Schiener",
                    EmailConfirmed = true
                };

                userManager.Create(manager, "Kassa1234!");

                var admin = new User()
                {
                    Email = "administrator@mvdata.at",
                    IsActive = true,
                    UserName = "Administrator",
                    FirstName = "Dominik",
                    LastName = "Schiener",
                    EmailConfirmed = true,
                    CreatedUsers = new List<User>()
                {
                    manager
                }
                };

                userManager.Create(admin, "Kassa1234!");

                var customer = new User
                {
                    Email = "customer@mv-data.at",
                    IsActive = true,
                    UserName = "Customer",
                    FirstName = "Dominik",
                    LastName = "Schiener",
                    EmailConfirmed = true,
                    Birthday = DateTime.Now.AddYears(-23),
                };

                userManager.Create(customer, "Kassa1234!");

                userManager.AddToRole(admin.Id, adminRole.Name);
                userManager.AddToRole(manager.Id, managerRole.Name);
                userManager.AddToRole(customer.Id, customerRole.Name);

                var ebillCategory = new EBillCategory()
                {
                    Name = "Schnitzel"
                };

                //CompanySetup
                var company = new Company
                {
                    CommercialRegisterNumber = "1234567890",
                    IsCashbackEnabled = true,
                    IsDeleted = false,
                    Name = "MV-Data Datenverarbeitung Ges.m.b.H",
                    TaxNumber = "ATU46004700",
                    UserId = manager.Id,
                    Stores = new List<Store> {
                    new Store
                    {
                        IsDeleted = false,
                        Name = "Zentrale",
                        ApiToken = "1234567890",
                        City = "Wien",
                        CountryId = austria.Id,
                        CreationDateTime = DateTime.Now,
                        HouseNr = "7",
                        Plz = "1030",
                        Street = "Steingasse",
                        EBillCategory = ebillCategory,
                        InterestedUsers = new List<User> {customer},
                        Coupons = new List<Coupon> {
                            new Coupon
                            {
                                CreatedUser = manager,
                                IsDeleted = false,
                                MaxRedeems = 10,
                                ShortDescription = "Das ist ein Testgutschein",
                                Name = "Testgutschein",
                                Value = 10,
                                CouponType = CouponType.Percent,
                                CouponImages = new List<CouponImage>
                                {
                                    new CouponImage
                                    {
                                        CreationDateTime = DateTime.Now,
                                        CreatorId = manager.Id,
                                        FileName = "test.png",
                                        IsDeleted = false,
                                        OrigFileName = "test.png"
                                    }
                                },
                                Visibilities = new List<AbstractCouponVisibility>
                                {
                                    new DateBoundCouponVisibility
                                    {
                                        ValidFrom = new DateTime(DateTime.Now.Year, DateTime.Now.Month,1),
                                        ValidTo = new DateTime(DateTime.Now.Year,DateTime.Now.Month, DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month))
                                    },
                                    new DayTimeBoundCouponVisibility
                                    {
                                        MondayFrom = TimeSpan.FromHours(8),
                                        MondayTo = TimeSpan.FromHours(18),
                                        TuesdayFrom = TimeSpan.FromHours(8),
                                        TuesdayTo = TimeSpan.FromHours(18),
                                        WednesdayFrom = TimeSpan.FromHours(8),
                                        WednesdayTo = TimeSpan.FromHours(18),
                                        ThursdayFrom = TimeSpan.FromHours(8),
                                        ThursdayTo = TimeSpan.FromHours(18),
                                        FridayFrom = TimeSpan.FromHours(8),
                                        FridayTo = TimeSpan.FromHours(18),
                                        SaturdayFrom = TimeSpan.FromHours(8),
                                        SaturdayTo = TimeSpan.FromHours(18),
                                        SundayFrom = TimeSpan.FromHours(8),
                                        SundayTo = TimeSpan.FromHours(18)
                                    }
                                }, 
                                UsageActions = new List<AbstractUsageAction>{new InvoiceDiscountUsageAction()}
                            },
                            new Coupon
                            {
                                CreatedUser = manager,
                                IsDeleted = false,
                                MaxRedeems = null,
                                ShortDescription = "Joyce's persönlicher Gutschein",
                                Name = "Joycebär",
                                Value = 50,
                                CouponType = CouponType.Value,
                                CouponImages = new List<CouponImage>
                                {
                                    new CouponImage
                                    {
                                        CreationDateTime = DateTime.Now,
                                        CreatorId = manager.Id,
                                        FileName = "bear.png",
                                        IsDeleted = false,
                                        OrigFileName = "bear.png"
                                    }
                                },
                                Visibilities = new List<AbstractCouponVisibility>
                                {
                                    new DateBoundCouponVisibility
                                    {
                                        ValidTo = new DateTime(DateTime.Now.Year,DateTime.Now.AddMonths(1).Month, DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.AddMonths(1).Month))
                                    }
                                },
                                UsageActions = new List<AbstractUsageAction>{new InvoiceDiscountUsageAction()}
                            },
                            new Coupon
                            {
                                CreatedUser = manager,
                                IsDeleted = false,
                                MaxRedeems = 1,
                                ShortDescription = "Geburtstagsgutschein",
                                Name = "Geburtstagsgutschein",
                                Value = 5,
                                CouponType = CouponType.Value,
                                CouponImages = new List<CouponImage>
                                {
                                    new CouponImage
                                    {
                                        CreationDateTime = DateTime.Now,
                                        CreatorId = manager.Id,
                                        FileName = "portal-cake2.jpg",
                                        IsDeleted = false,
                                        OrigFileName = "portal-cake2.jpg"
                                    }
                                },
                                Visibilities = new List<AbstractCouponVisibility>
                                {
                                    new BirthdayCouponVisibility
                                    {
                                        ValidDaysAfter = 3,
                                        ValidDaysBefore = 3,

                                    }
                                },
                                UsageActions = new List<AbstractUsageAction>{new InvoiceDiscountUsageAction()}
                            },
                            new Coupon
                            {
                                CreatedUser = manager,
                                IsDeleted = false,
                                MaxRedeems = null,
                                ShortDescription = "Umsatzgutschein",
                                Name = "Umsatzgutschein",
                                Value = 5,
                                CouponType = CouponType.Percent,
                                CouponImages = new List<CouponImage>
                                {
                                    new CouponImage
                                    {
                                        CreationDateTime = DateTime.Now,
                                        CreatorId = manager.Id,
                                        FileName = "euro.png",
                                        IsDeleted = false,
                                        OrigFileName = "euro.png"
                                    }
                                },
                                Visibilities = new List<AbstractCouponVisibility>
                                {
                                    new MinimumTurnoverCouponVisibility
                                    {
                                        MinimumTurnover = 10,
                                        DateRange = 10
                                    }
                                },
                                UsageActions = new List<AbstractUsageAction>{new InvoiceDiscountUsageAction()}
                            },
                            new Coupon
                            {
                                CreatedUser = manager,
                                IsDeleted = false,
                                MaxRedeems = null,
                                ShortDescription = "Besuchzahlgutschein",
                                Name = "Besuchzahlgutschein",
                                Value = 5,
                                CouponType = CouponType.Value,
                                CouponImages = new List<CouponImage>
                                {
                                    new CouponImage
                                    {
                                        CreationDateTime = DateTime.Now,
                                        CreatorId = manager.Id,
                                        FileName = "visit.png",
                                        IsDeleted = false,
                                        OrigFileName = "visit.png"
                                    }
                                },
                                Visibilities = new List<AbstractCouponVisibility>
                                {
                                    new MinimumVisitCouponVisibility()
                                    {
                                        MinimumVisits = 3,
                                        DateRange = 10
                                    }
                                },
                                UsageActions = new List<AbstractUsageAction>{new InvoiceDiscountUsageAction()}
                            },
                            new Coupon
                            {
                                CreatedUser = manager,
                                IsDeleted = false,
                                MaxRedeems = null,
                                ShortDescription = "2+1Gratis",
                                Name = "Umsatzgutschein",
                                Value = 5,
                                CouponType = CouponType.Percent,
                                CouponImages = new List<CouponImage>
                                {
                                    new CouponImage
                                    {
                                        CreationDateTime = DateTime.Now,
                                        CreatorId = manager.Id,
                                        FileName = "euro.png",
                                        IsDeleted = false,
                                        OrigFileName = "euro.png"
                                    }
                                },
                                Visibilities = new List<AbstractCouponVisibility>
                                {
                                    new MinimumTurnoverCouponVisibility
                                    {
                                        MinimumTurnover = 10,
                                        DateRange = 10
                                    }
                                },
                                UsageActions = new List<AbstractUsageAction>{new InvoiceDiscountUsageAction()}
                            },
                        },
                        TurnoverJournals = new List<TurnoverJournal>
                        {
                            new Cashback()
                            {
                                CreditDateTime = DateTime.Now.AddDays(-1).AddHours(0),
                                TurnoverOld = 0,
                                TurnoverChange = 10,
                                TurnoverNew = 10,
                                UserId = customer.Id,
                                CreditOld = 0,
                                CreditChange = 1,
                                CreditNew = 1
                            },
                            new Cashback()
                            {
                                CreditDateTime = DateTime.Now.AddDays(-1).AddHours(1),
                                TurnoverOld = 10,
                                TurnoverChange = 5,
                                TurnoverNew = 15,
                                UserId = customer.Id,
                                CreditOld = 1,
                                CreditChange = 0.5m,
                                CreditNew = 1.5m
                            },
                            new Cashback()
                            {
                                CreditDateTime = DateTime.Now.AddDays(-1).AddHours(2),
                                TurnoverOld = 15,
                                TurnoverChange = 30,
                                TurnoverNew = 45,
                                UserId = customer.Id,
                                CreditOld =1.5m,
                                CreditChange = 3,
                                CreditNew = 4.5m
                            }
                        }
                    }
                }
                };


                // bills
                var bill = new Bill
                {
                    Amount = 5.01m,
                    Company = "free-store",
                    UserId = admin.Id,
                    ImportedAt = DateTime.Now,
                    InvoiceNumber = "33rer",
                    InvoiceDate = DateTime.Now,
                    
                    Invoice = new Shared.Dto.Invoice
                    {
                        InvoiceDate = DateTime.Now,
                        Comment = "bill",
                        InvoiceNumber = "33rer",
                        TotalGrossAmount = 5.01m
                    }
                };
                context.Bills.Add(bill);

                bill = new Bill
                {
                    Amount = 4.04m,
                    Company = "free-store-2",
                    UserId = manager.Id,
                    ImportedAt = DateTime.Now,
                    InvoiceNumber = "33rer-2222",
                    InvoiceDate = DateTime.Now,

                    Invoice = new Shared.Dto.Invoice
                    {
                        InvoiceDate = DateTime.Now,
                        Comment = "bill-2",
                        InvoiceNumber = "33rer-2222",
                        TotalGrossAmount = 4.04m
                    }
                };
                context.Bills.Add(bill);

                context.Companies.Add(company);
                context.SaveChanges();
            }
        }
    }
}
