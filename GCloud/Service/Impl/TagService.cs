using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GCloud.Models.Domain;
using GCloud.Repository;

namespace GCloud.Service.Impl
{
    public class TagService : AbstractService<Tag>, ITagService
    {
        private readonly ITagRepository _tagRepository;
        private readonly IStoreRepository _storeRepository;

        public TagService(ITagRepository repository, IStoreRepository storeRepository) : base(repository)
        {
            _tagRepository = repository;
            _storeRepository = storeRepository;
        }

        
        public IList<Tag> FindTagsByName(IEnumerable<string> tagNames)
        {
            var result = new List<Tag>();

            if (tagNames != null)
            {
                foreach (var tagName in tagNames.Where(t => !string.IsNullOrWhiteSpace(t)))
                {
                    var dbTag = _tagRepository.FindByName(tagName);
                    if (dbTag == null)
                    {
                        dbTag = new Tag
                        {
                            Name = tagName
                        };
                        _tagRepository.Add(dbTag);
                    }
                    result.Add(dbTag);
                }
            }

            return result;
        }

        public void AssignTagsToStore(ICollection<Tag> tags, Store store)
        {
            foreach (var tag in tags)
            {
                store.Tags.Add(tag);
            }
            _storeRepository.Update(store);
        }

        public void RemoveTagsFromStore(ICollection<Tag> tags, Store store)
        {
            foreach (var tag in tags)
            {
                var storeTag = store.Tags.FirstOrDefault(sTag => sTag.Name == tag.Name);
                if (storeTag != null)
                {
                    store.Tags.Remove(storeTag);
                }
            }
            _storeRepository.Update(store);
        }

        public void RemoveAllTagsFromStore(Store store)
        {
            store.Tags.Clear();
            _storeRepository.Update(store);
        }
    }
}