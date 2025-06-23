using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Security.Policy;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using GCloud.Models;
using GridMvc.Columns;
using DisplayNameAttribute = System.ComponentModel.DisplayNameAttribute;

namespace GCloud.Extensions
{
    public static class GridMvcExtensions
    {
        public static IGridColumn<TEntity> TitledByAttribute<TEntity>(this IGridColumn<TEntity> column)
        {
            var property = typeof(TEntity).GetProperties().FirstOrDefault(p => p.Name == column.Name && (Attribute.IsDefined(p, typeof(DisplayNameAttribute)) || Attribute.IsDefined(p, typeof(DisplayAttribute))));

            if (property != null)
            {
                foreach (var customAttribute in property.GetCustomAttributes())
                {
                    switch (customAttribute)
                    {
                        case DisplayNameAttribute d:
                            column.Titled(d.DisplayName);
                            break;
                        case DisplayAttribute d:
                            if (d.Name != null)
                            {
                                column.Titled(d.Name);
                            }
                            break;
                    }
                }
                return column;
            }

            return column;
        }

        public static IGridColumn<TEntity> WithListOptions<TEntity>(this IGridColumn<TEntity> column, HtmlHelper<IEnumerable<TEntity>> htmlHelper, bool unmodifyable = false) where TEntity : IIdentifyable
        {
            
            return column.Sanitized(false).Encoded(false).RenderValueAs(x => htmlHelper.Partial(unmodifyable ? "_UnmodifyableListOptions" : "_ListOptions", x).ToHtmlString());
        }

        public static IGridColumn<TEntity> WithImage<TEntity>(this IGridColumn<TEntity> column, HtmlHelper<IEnumerable<TEntity>> htmlHelper, Func<TEntity, string> imageUrl) where TEntity : IIdentifyable
        {
            return column.Sanitized(false).Encoded(false).RenderValueAs(x => htmlHelper.Raw($@"<div class='media'><div class='media-left'><a href='#'><img class='media-object' src='{imageUrl(x)}' alt='Bild' style='height: 5rem' /></a></div></div>").ToHtmlString());
        }

        public static IGridColumn<TEntity> AddListOptions<TEntity>(this IGridColumnCollection<TEntity> columns, HtmlHelper<IEnumerable<TEntity>> htmlHelper) where TEntity : IIdentifyable
        {
            var column = columns.Add().WithListOptions(htmlHelper);
            return column;
        }

        public static IGridColumn<TEntity> AddListOptionsUnmodifyable<TEntity>(this IGridColumnCollection<TEntity> columns, HtmlHelper<IEnumerable<TEntity>> htmlHelper) where TEntity : IIdentifyable
        {
            var column = columns.Add().WithListOptions(htmlHelper, true);
            return column;
        }
    }
}