using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GCloud.Models.Domain;

namespace GCloud.Service
{
    public interface ITagService : IAbstractService<Tag>
    {
        /// <summary>
        /// Sucht nach einem Tag mit dem gegebenen Namen.
        /// Wurde noch kein Tag mit diesem Namen angelegt, so wird ein neuer Tag mit der Bezeichnung erstellt und in das Ergebnis inkludiert
        /// </summary>
        /// <param name="tagNames">Eine Liste aller Tag namen, nach welchen gesucht werden soll.</param>
        IList<Tag> FindTagsByName(IEnumerable<string> tagNames);

        void AssignTagsToStore(ICollection<Tag> tags, Store store);
        void RemoveTagsFromStore(ICollection<Tag> tags, Store store);
        void RemoveAllTagsFromStore(Store store);

    }
}