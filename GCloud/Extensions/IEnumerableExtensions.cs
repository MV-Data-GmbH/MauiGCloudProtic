using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Policy;
using System.Web;
using GCloud.Models.Domain;
using LinqKit;

namespace GCloud.Extensions
{
    public static class IEnumerableExtensions
    {
        public static T WhereIsCurrentlyValid<T>(this T coupons, string userId) where T : IEnumerable<Coupon>
        {
            if (TryAsQueryable(coupons, out var queryableList))
            {
                return (T)queryableList.Where(c => c.IsCurrentlyValidForUser(userId));
            }

            return (T) coupons.Where(c => c.IsCurrentlyValidForUser(userId));
        }

        #region DateTimeChecks

        public static T WhereDayTimeIsValid<T>(this T coupon) where T : IEnumerable<Coupon>
        {
            return coupon.WhereVisibilityMatches<DayTimeBoundCouponVisibility, T>(x => x.IsValid());
        }

        public static T WhereDateIsValid<T>(this T coupons) where T : IEnumerable<Coupon>
        {
            return coupons.WhereVisibilityMatches<DateBoundCouponVisibility, T>(x => x.IsValid());
        }

        public static T WhereDateExpired<T>(this T coupons) where T : IEnumerable<Coupon>
        {
            return coupons.WhereVisibilityMatches<DateBoundCouponVisibility, T>(x => x.IsDateTimeExpired(), false);
        }

        public static T WhereDateIsUpcoming<T>(this T coupons) where T : IEnumerable<Coupon>
        {
            return coupons.WhereVisibilityMatches<DateBoundCouponVisibility, T>(x => x.IsDateTimeUpcoming(), false);
        }

        public static T WhereDateIsNowOrUpcoming<T>(this T coupons) where T : IEnumerable<Coupon>
        {
            return coupons.WhereVisibilityMatches<DateBoundCouponVisibility, T>(x => x.IsDateTimeNowOrUpcoming(), false);
        }

        #endregion

        public static T WhereUserHasRedeemsLeft<T>(this T coupons, string userId) where T : IEnumerable<Coupon>
        {
            if (TryAsQueryable(coupons, out var queryableList))
            {
                return (T)queryableList.Where(x => x.Redeems.Count(redeem => redeem.UserId == userId) < (x.MaxRedeems ?? int.MaxValue));
            }

            return (T) coupons.Where(x => x.Redeems.Count(redeem => redeem.UserId == userId) < (x.MaxRedeems ?? int.MaxValue));
        }

        public static T OrderByName<T>(this T coupons) where T : IEnumerable<Coupon>
        {
            if (TryAsQueryable(coupons, out var queryableList))
            {
                return (T)queryableList.OrderBy(x => x.Name);
            }

            return (T) coupons.OrderBy(x => x.Name); ;
        }

        public static T WhereManagerOwnsCoupon<T>(this T coupons, string managerId) where T : IEnumerable<Coupon>
        {
            if (TryAsQueryable(coupons, out var queryableList))
            {
                return (T)queryableList.Where(c => c.AssignedStores.Any(store => store.Company.User.Id == managerId));
            }

            return (T) coupons.Where(c => c.AssignedStores.Any(store => store.Company.User.Id == managerId));
        }

        #region VisibilityMatches

        /// <summary>
        /// Filtert Gutscheine nach deren Kriterien. Es werden alle zurückgegeben welche die anforderung erfüllen, oder dieser Anforderung garnicht zugewiesen wurden.
        /// </summary>
        /// <typeparam name="TVisibility">Der Typ der Anforderung</typeparam>
        /// <typeparam name="TEnum">Der Typ der Enumeration</typeparam>
        /// <param name="coupons">Die Couponliste die gefiltert werden soll</param>
        /// <param name="userId">Die Id des Users für den die Einschränkung ausgewertet wird</param>
        /// <param name="includeNonSpecified">Wenn auf True, werden auch Coupons inkludiert, welche die gegebene Bedingung gar nicht aufweisen. zB werden Gutscheine inkludiert, welche keine Datumseinschränkung haben.</param>
        /// <returns>Eine Liste mit gefilterten Coupons</returns>
        private static TEnum WhereVisibilityMatches<TVisibility, TEnum>(this TEnum coupons, string userId, bool includeNonSpecified = true) where TVisibility : AbstractCouponVisibility where TEnum : IEnumerable<Coupon>
        {
            var predicate = PredicateBuilder.New<Coupon>();

            if (TryAsQueryable(coupons, out var queryableList))
            {
                predicate.Or(x => x.Visibilities.OfType<TVisibility>().Any() && x.Visibilities.OfType<TVisibility>().First().IsValid(userId));

                if (includeNonSpecified)
                {
                    predicate.Or(x => !x.Visibilities.OfType<TVisibility>().Any());
                }

                return (TEnum)queryableList.AsExpandable().Where(predicate);
            }

            predicate.Or(x => x.Visibilities.OfType<TVisibility>().Any() && x.Visibilities.OfType<TVisibility>().First().IsValid(userId));

            if (includeNonSpecified)
            {
                predicate.Or(x => !x.Visibilities.OfType<TVisibility>().Any());
            }

            return (TEnum) coupons.Where(predicate);

        }

        /// <summary>
        /// Filtert Gutscheine nach deren Kriterien. Es werden alle zurückgegeben welche die anforderung erfüllen, oder dieser Anforderung garnicht zugewiesen wurden.
        /// </summary>
        /// <typeparam name="TVisibility">Der Typ der Anforderung</typeparam>
        /// <typeparam name="TEnum">Der Typ der Enumeration</typeparam>
        /// <param name="coupons">Die Couponliste die gefiltert werden soll</param>
        /// <param name="expr">Die bedingung die erfüllt werden muss</param>
        /// <param name="includeNonSpecified">Wenn auf True, werden auch Coupons inkludiert, welche die gegebene Bedingung gar nicht aufweisen. zB werden Gutscheine inkludiert, welche keine Datumseinschränkung haben.</param>
        /// <returns>Eine Liste mit gefilterten Coupons</returns>
        private static TEnum WhereVisibilityMatches<TVisibility, TEnum>(this TEnum coupons, Func<TVisibility, bool> expr, bool includeNonSpecified = true) where TVisibility : AbstractCouponVisibility where TEnum : IEnumerable<Coupon>
        {
            var predicate = PredicateBuilder.New<Coupon>();

            if (TryAsQueryable(coupons, out var queryableList))
            {
                predicate.Or(x => x.Visibilities.OfType<TVisibility>().Any() && expr(x.Visibilities.OfType<TVisibility>().First()));

                if (includeNonSpecified)
                {
                    predicate.Or(x => !x.Visibilities.OfType<TVisibility>().Any());
                }

                return (TEnum) queryableList.Where(predicate);
            }

            predicate.Or(x => x.Visibilities.OfType<TVisibility>().Any() && expr(x.Visibilities.OfType<TVisibility>().First()));

            if (includeNonSpecified)
            {
                predicate.Or(x => !x.Visibilities.OfType<TVisibility>().Any());
            }

            return (TEnum) coupons.Where(predicate);
        }

        #endregion

        /// <summary>
        /// Wird verwendet um IQueryable und IEnumerable zu unterstützen, damit man nicht alles kopieren muss
        /// </summary>
        /// <typeparam name="TEnum">Der Typ der Enum</typeparam>
        /// <param name="coupons">Die liste der Enum</param>
        /// <returns>Entweder ein IQueryable Converted List, oder die Ursprungsliste</returns>
        private static bool TryAsQueryable<TEnum>(TEnum coupons, out IQueryable<Coupon> queryableList) where TEnum : IEnumerable<Coupon>
        {
            if (coupons.GetType().GetInterfaces().Any(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(IQueryable<>)))
            {
                queryableList = coupons.AsQueryable();
                return true;
            }

            queryableList = null;

            return false;
        }
    }
}