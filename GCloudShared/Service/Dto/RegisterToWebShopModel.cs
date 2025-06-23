

namespace GCloudShared.Service.Dto
{
    public class RegisterToWebShopModel
    {
        public string Email { get; set; }

        public bool UsernamesEnabled { get; set; }

        public string Username { get; set; }

        public bool CheckUsernameAvailabilityEnabled { get; set; }


        public string Password { get; set; }


        public string ConfirmPassword { get; set; }

        //form fields & properties
        public bool GenderEnabled { get; set; }

        public string Gender { get; set; }


        public string FirstName { get; set; }

        public string LastName { get; set; }


        public bool DateOfBirthEnabled { get; set; }

        public int? DateOfBirthDay { get; set; }

        public int? DateOfBirthMonth { get; set; }

        public int? DateOfBirthYear { get; set; }

        public bool CompanyEnabled { get; set; }
        public bool CompanyRequired { get; set; }

        //public string Company { get; set; }

        public bool StreetAddressEnabled { get; set; }
        public bool StreetAddressRequired { get; set; }

        public string StreetAddress { get; set; }

        public bool ZipPostalCodeEnabled { get; set; }
        public bool ZipPostalCodeRequired { get; set; }

        public string ZipPostalCode { get; set; }

        public bool CityEnabled { get; set; }
        public bool CityRequired { get; set; }

        public string City { get; set; }

        //public bool PhoneEnabled { get; set; }

        //public bool PhoneRequired { get; set; }

        //public string Phone { get; set; }
    }
}
