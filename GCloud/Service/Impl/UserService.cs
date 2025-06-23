using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Security.Policy;
using System.Web;
using GCloud.Models.Domain;
using GCloud.Repository;
using GCloud.Shared.Exceptions.Anonymous;
using GCloud.Shared.Exceptions.User;
using Microsoft.AspNet.Identity;

namespace GCloud.Service.Impl
{
    public class UserService : AbstractService<User>, IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IAnonymousUserRepository _anonymousUserRepository;

        public UserService(IUserRepository userRepository, IAnonymousUserRepository anonymousUserRepository) : base(userRepository)
        {
            this._userRepository = userRepository;
            _anonymousUserRepository = anonymousUserRepository;
        }

        public User FindbyUsername(string username)
        {
            //koristi se kod logovanje, pronalazenje korisnika u bazi na osnovu UserName-a
            //stara verzija
            //return _userRepository.FindFirstOrDefault(x => x.UserName == username);

            try
            {
                User user = null;

                string connectionString = ConfigurationManager.ConnectionStrings["GCloudContext"].ConnectionString;
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string query = @"
                        SELECT *
                        FROM [dbo].[AspNetUsers]
                        WHERE [UserName] = @Username";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Username", username);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                user = new User
                                {
                                    Id = reader["Id"].ToString(),
                                    IsActive = (bool)reader["IsActive"],
                                    FirstName = reader["FirstName"].ToString(),
                                    LastName = reader["LastName"].ToString(),
                                    Email = reader["Email"].ToString(),
                                    Birthday = reader["Birthday"] != DBNull.Value ? (DateTime?)reader["Birthday"] : null,
                                    CreatedById = reader["CreatedById"].ToString(),
                                    IsDeleted = (bool)reader["IsDeleted"],
                                    EmailConfirmed = (bool)reader["EmailConfirmed"],
                                    PasswordHash = reader["PasswordHash"].ToString(),
                                    SecurityStamp = reader["SecurityStamp"].ToString(),
                                    PhoneNumber = reader["PhoneNumber"].ToString(),
                                    PhoneNumberConfirmed = (bool)reader["PhoneNumberConfirmed"],
                                    TwoFactorEnabled = (bool)reader["TwoFactorEnabled"],
                                    LockoutEndDateUtc = reader["LockoutEndDateUtc"] != DBNull.Value ? (DateTime?)reader["LockoutEndDateUtc"] : null,
                                    LockoutEnabled = (bool)reader["LockoutEnabled"],
                                    AccessFailedCount = (int)reader["AccessFailedCount"],
                                    UserName = reader["UserName"].ToString(),
                                    InvitationCode = reader["InvitationCode"].ToString(),
                                    TotalPoints = reader["TotalPoints"].ToString(),
                                    InvitationCodeSender = reader["InvitationCodeSender"].ToString(),
                                    DataProtection = (bool)reader["DataProtection"],
                                    AGB = (bool)reader["AGB"],
                                    MarketingAgreement = (bool)reader["MarketingAgreement"]
                                };
                            }
                        }
                    }
                }

                return user;
            }
            catch (Exception)
            {
                return null;
            }

        }

        public User FindbyEmail(string email)
        {
            //funkcija se koristi kod brisanja usera
            //stara verzija
            return _userRepository.FindFirstOrDefault(x => x.Email == email);


            //try
            //{
            //    User user = null;

            //    string connectionString = ConfigurationManager.ConnectionStrings["GCloudContext"].ConnectionString;
            //    using (SqlConnection connection = new SqlConnection(connectionString))
            //    {
            //        connection.Open();

            //        string query = "SELECT * FROM AspNetUsers WHERE Email = @Email";

            //        using (SqlCommand command = new SqlCommand(query, connection))
            //        {
            //            command.Parameters.AddWithValue("@Email", email);

            //            using (SqlDataReader reader = command.ExecuteReader())
            //            {
            //                if (reader.Read())
            //                {
            //                    user = new User
            //                    {
            //                        Id = reader["Id"].ToString(),
            //                        IsActive = (bool)reader["IsActive"],
            //                        FirstName = reader["FirstName"].ToString(),
            //                        LastName = reader["LastName"].ToString(),
            //                        Email = reader["Email"].ToString(),
            //                        Birthday = reader["Birthday"] != DBNull.Value ? (DateTime?)reader["Birthday"] : null,
            //                        CreatedById = reader["CreatedById"].ToString(),
            //                        IsDeleted = (bool)reader["IsDeleted"],
            //                        EmailConfirmed = (bool)reader["EmailConfirmed"],
            //                        PasswordHash = reader["PasswordHash"].ToString(),
            //                        SecurityStamp = reader["SecurityStamp"].ToString(),
            //                        PhoneNumber = reader["PhoneNumber"].ToString(),
            //                        PhoneNumberConfirmed = (bool)reader["PhoneNumberConfirmed"],
            //                        TwoFactorEnabled = (bool)reader["TwoFactorEnabled"],
            //                        LockoutEndDateUtc = reader["LockoutEndDateUtc"] != DBNull.Value ? (DateTime?)reader["LockoutEndDateUtc"] : null,
            //                        LockoutEnabled = (bool)reader["LockoutEnabled"],
            //                        AccessFailedCount = (int)reader["AccessFailedCount"],
            //                        UserName = reader["UserName"].ToString(),
            //                        InvitationCode = reader["InvitationCode"].ToString(),
            //                        TotalPoints = reader["TotalPoints"].ToString(),
            //                        InvitationCodeSender = reader["InvitationCodeSender"].ToString(),
            //                        DataProtection = (bool)reader["DataProtection"],
            //                        AGB = (bool)reader["AGB"],
            //                        MarketingAgreement = (bool)reader["MarketingAgreement"]
            //                    };
            //                }
            //            }
            //        }
            //    }

            //    return user;
            //}
            //catch (Exception)
            //{
            //    return null;
            //}
        }

        public void AssignAnonymousUserToUser(Guid anonymousUserId, string userId)
        {
            var realUser = _userRepository.FindById(userId);
            if (realUser == null)
            {
                throw new UserNotFoundException(userId);
            }

            var anonymousUser = _anonymousUserRepository.FindById(anonymousUserId);
            if (anonymousUser == null)
            {
                throw new AnonymousUserNotFoundException(anonymousUserId);
            }

            anonymousUser.UserId = userId;

            _anonymousUserRepository.Update(anonymousUser);
        }
    }
}