using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GCloud.Models.Domain;
using GCloud.Repository;

namespace GCloud.Service
{
    public interface IUserService : IAbstractService<User>
    {
        User FindbyUsername(string username);
        User FindbyEmail(string email);

        /// <summary>
        /// Weist einen anonymen Benutzer einem "echten" Benutzer zu.
        /// </summary>
        /// <param name="anonymousUserId">Die Id des anonymen Benutzers</param>
        /// <param name="userId">Die Id des "echten" Benutzers</param>
        void AssignAnonymousUserToUser(Guid anonymousUserId, string userId);
    }
}