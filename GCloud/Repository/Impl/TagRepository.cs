using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using GCloud.Models.Domain;

namespace GCloud.Repository.Impl
{
    public class TagRepository : AbstractRepository<Tag>, ITagRepository
    {
        public TagRepository(DbContext context) : base(context)
        {
        }

        public Tag FindByName(string tagName)
        {
            return FindBy(tag => tag.Name == tagName).FirstOrDefault();
        }
    }
}