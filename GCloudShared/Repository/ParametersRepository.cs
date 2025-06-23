using GCloudShared.Domain;
using GCloudShared.Shared;
using Newtonsoft.Json;
using SQLite;

namespace GCloudShared.Repository
{
    public class ParametersRepository : AbstractRepository<Parameters>
    {


        public ParametersRepository(SQLiteConnection connection) : base(connection)
        {

        }
        public Parameters FindByParameter(string parametersName)
        {
            try
            {
                var result = _connection.Table<Parameters>().Where(x => x.ParameterName == parametersName).FirstOrDefault();
                return result;
            }
            catch (Exception)
            {
                return null;
            }


        }
        public void InsertFastOrder(string barcodeData)
        {
            Parameters pa = new Parameters();
            var data=_connection.Table<Parameters>().Where(x=>x.ParameterName== "FastOrder").ToList();
            if(data!=null )
            {
                this.Delete("FastOrder");
            }
            pa.ParameterName = "FastOrder";
            pa.ParameterValue = barcodeData;
            pa.TypeParameter = TypeParameter.FastOrder;
            this.Insert(pa);
            
           // pr.Insert(pa);
        }
        public List<Parameters> Get()
        {
            try
            {
                var result = _connection.Table<Parameters>().ToList();
                return result;
            }
            catch (Exception)
            {
                return null;
            }


        }
        public void Delete(string parameterName)
        {
            var result = _connection.Table<Parameters>().Where(x => x.ParameterName == parameterName).FirstOrDefault();
            if (result != null)
            {
                _connection.Delete(result);
            }

        }
        #region Resources
        private string InsertParameters(List<Parameters> parameters)
        {
            try
            {
                if (_connection.Table<Parameters>().ToList().Count() != 0)
                {
                    for (int i = 0; i < _connection.Table<Parameters>().ToList().Count(); i++)
                    {
                        var forDelete = _connection.Table<Parameters>().Where(x => x.TypeParameter == TypeParameter.Resource).ToList();
                        foreach (var parameter in forDelete)
                        {
                            _connection.Delete(parameter);
                        }
                    }

                }
                var result = _connection.InsertAll(parameters);
                return "Ok";


            }
            catch (Exception)
            {
                return null;
            }


        }
        public static void FillResources()
        {
            ParametersRepository pr = new ParametersRepository(DbBootstraper.Connection);
            List<Parameters> parameters = new List<Parameters>();
            Resources resource = new Resources();
            foreach (var prop in resource.GetType().GetProperties())
            {
                Parameters p = new Parameters();
                p.ParameterName = prop.Name;
                p.ParameterValue = prop.GetValue(resource, null).ToString();
                p.TypeParameter = TypeParameter.Resource;
                parameters.Add(p);
            }


            pr.InsertParameters(parameters);
        }

        #endregion

        #region Auth
        public static void SetAuthTokenToParameterTable(string authToken, DateTime expirationDate)
        {
            ParametersRepository pr = new ParametersRepository(DbBootstraper.Connection);
            AuthToken auth = new AuthToken
            {
                Token = authToken,
                Expiration = expirationDate
            };

            // Delete the existing AuthToken parameter if it exists
            var param = pr.FindByParameter("AuthToken");
            if (param != null)
            {
                pr.Delete("AuthToken");
            }


            var value = JsonConvert.SerializeObject(auth);
            Parameters p = new Parameters
            {
                ParameterValue = value,
                ParameterName = "AuthToken",
                TypeParameter = TypeParameter.Token
            };



            pr.Insert(p);
        }

        public static string GetAuthTokenFromParameterTable()
        {
            ParametersRepository pr = new ParametersRepository(DbBootstraper.Connection);
            var param = pr.FindByParameter("AuthToken");
            if (param != null)
            {
                var token = JsonConvert.DeserializeObject<AuthToken>(param.ParameterValue);

                return token.Token;
            }

            return null;
        }

        public static DateTime? GetAuthTokenExpirationFromParameterTable()
        {
            ParametersRepository pr = new ParametersRepository(DbBootstraper.Connection);
            var param = pr.FindByParameter("AuthToken");
            if (param != null)
            {
                AuthToken authToken = JsonConvert.DeserializeObject<AuthToken>(param.ParameterValue);
                return authToken.Expiration;
            }
            return null;
        }



        public static void DeletedAuthToken()
        {
            ParametersRepository parametersRepository = new ParametersRepository(DbBootstraper.Connection);
            parametersRepository.Delete("AuthToken");
        }

        public static void SetDeviceIdToParameterTable(string DeviceId)
        {
            ParametersRepository pr = new ParametersRepository(DbBootstraper.Connection);
            var param = pr.FindByParameter("DeviceId");
            if (param != null)
            {

                pr.Delete("DeviceId");

            }
            Parameters p = new Parameters();
            p.ParameterValue = DeviceId;
            p.ParameterName = "DeviceId";
            p.TypeParameter = TypeParameter.DeviceId;
            pr.Insert(p);


        }
        public static string GetDeviceIdFromParameterTable()
        {
            ParametersRepository pr = new ParametersRepository(DbBootstraper.Connection);
            var param = pr.FindByParameter("DeviceId");
            if (param != null)
            {
                var DeviceId = param.ParameterValue;

                return DeviceId;
            }

            return null;
        }


        public static void DeletedDeviceId()
        {
            ParametersRepository parametersRepository = new ParametersRepository(DbBootstraper.Connection);
            parametersRepository.Delete("DeviceId");
        }
        #endregion

        #region Web
        public static void SetWebAuthTokenToParameterTable(string authToken)
        {
            ParametersRepository pr = new ParametersRepository(DbBootstraper.Connection);
            AuthToken auth = new AuthToken();
            var param = pr.FindByParameter("WebAuthToken");
            if (param != null)
            {

                pr.Delete("WebAuthToken");

            }


            auth.Token = authToken;

            var value = JsonConvert.SerializeObject(auth);
            Parameters p = new Parameters();
            p.ParameterValue = value;
            p.ParameterName = "WebAuthToken";
            p.TypeParameter = TypeParameter.Token;
            pr.Insert(p);




        }
        public static string GetWebAuthTokenFromParameterTable()
        {
            ParametersRepository pr = new ParametersRepository(DbBootstraper.Connection);
            var param = pr.FindByParameter("WebAuthToken");
            if (param != null)
            {
                var token = JsonConvert.DeserializeObject<AuthToken>(param.ParameterValue);

                return token.Token;
            }

            return null;
        }
        public static void DeletedWebAuthToken()
        {
            ParametersRepository parametersRepository = new ParametersRepository(DbBootstraper.Connection);
            parametersRepository.Delete("WebAuthToken");
        }
        public static void SetWebDeviceIdToParameterTable(string DeviceId)
        {
            ParametersRepository pr = new ParametersRepository(DbBootstraper.Connection);
            var param = pr.FindByParameter("WebDeviceId");
            if (param != null)
            {

                pr.Delete("WebDeviceId");

            }
            Parameters p = new Parameters();
            p.ParameterValue = DeviceId;
            p.ParameterName = "WebDeviceId";
            p.TypeParameter = TypeParameter.DeviceId;
            pr.Insert(p);


        }
        public static string GetWebDeviceIdFromParameterTable()
        {
            ParametersRepository pr = new ParametersRepository(DbBootstraper.Connection);
            var param = pr.FindByParameter("WebDeviceId");
            if (param != null)
            {
                var DeviceId = param.ParameterValue;

                return DeviceId;
            }

            return null;
        }
        public static void DeletedWebDeviceId()
        {
            ParametersRepository parametersRepository = new ParametersRepository(DbBootstraper.Connection);
            parametersRepository.Delete("WebDeviceId");
        }
        #endregion
    }
}
