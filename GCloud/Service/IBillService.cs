using System;
using System.Collections.Generic;
using System.Linq;
using GCloud.Models.Domain;
using GCloud.Shared.Dto;

namespace GCloud.Service
{
    public interface IBillService : IAbstractService<Bill>
    {
        /// <summary>
        /// Fügt ein neues anonymes Gerät hinzu und verknüpft dieses gleich mit einem Benutzer
        /// </summary>
        /// <param name="firebaseToken">Der FirebaseToken des Gerätes</param>
        /// <param name="anonymousUserId">Die User-ID des Benutzers. Wenn <code>null</code> wird ein neuer Benutzer für das Gerät angelegt</param>
        /// <returns></returns>
        AnonymousUser AddAnonymousUserPhone(string firebaseToken, Guid? anonymousUserId);

        /// <summary>
        /// Returns all bills, including the bills from the anonymous linked accounts
        /// </summary>
        /// <param name="userId">The userId of the account performing the request</param>
        /// <returns>A list of all bills created with this and its associated anonymous users</returns>
        List<Bill> FindAllForUser(string userId, List<Guid> alreadyGot = null);

        /// <summary>
        /// Liefert alle rechnungen von einem anonymen Benutzer zurück
        /// </summary>
        /// <param name="anonymousUserId">Die Id des anonymen Benutzers</param>
        /// <param name="alreadyGot">Eine Liste von Rechnungen, die ignoriert werden</param>
        /// <returns>Eine Liste aller Rechnungen für den gegebenen Benutzer</returns>
        List<Bill> FindAllForAnonymousUser(Guid anonymousUserId, List<Guid> alreadyGot = null);

        /// <summary>
        /// Sucht alle mit diesem User und dessen anonymen User verknüpften Rechnungen nach der übergebenen ID ab
        /// </summary>
        /// <param name="userId">Die Id des Users, der überprüft werden soll</param>
        /// <param name="billId">Die Id der Rechnung, welche überprüft werden soll.</param>
        /// <returns>Die Rechnung, welche dem User, oder einem seiner anonymen User zugewiesen wurde.</returns>
        Bill FindByIdForUser(string userId, Guid billId);

        /// <summary>
        /// Liefert eine Rechnung welche dem anonymen Benutzer zugewiesen ist
        /// </summary>
        /// <param name="anonymousUserId">Die Id des anonymen Benutzers</param>
        /// <param name="billId">Die Id der Rechnung</param>
        /// <returns>Die Rechnung</returns>
        Bill FindByIdForAnonymous(Guid anonymousUserId, Guid billId);

        String BillsCsv(List<Invoice> billList);

        void DeleteAnonymousUser(Guid anonymousUserId);
    }
}