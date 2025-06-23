using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GCloud.Shared
{
    public class BaseUrlContainer
    {
#if DEBUG
        // === DEBUG (ali produkcioni hostovi) ===

        public const string BaseUrlScheme = "https";
     
        public const string BaseUrlHost = "protictest1.willessen.online";
        public const int BaseUrlPort = 443;

        public const string BaseUrlWebScheme = "https";
        
        public const string BaseUrlWebHost = "test1mvdprotic.willessen.online";
        public const int BaseUrlWebPort = 443;


#else
        // === RELEASE ===

        public const string BaseUrlScheme     = "https";
        // stari produkcioni (foodjet): public const string BaseUrlHost = "schnitzelwelt.foodjet.online";
        public const string BaseUrlHost       = "protictest1.willessen.online";
        public const int    BaseUrlPort       = 443;

        public const string BaseUrlWebScheme  = "https";
     
        public const string BaseUrlWebHost    = "test1mvdprotic.willessen.online";
        public const int    BaseUrlWebPort    = 443;
#endif

        public static Uri BaseUri => new UriBuilder(BaseUrlScheme, BaseUrlHost).Uri;
        public static Uri BaseUriWeb => new UriBuilder(BaseUrlWebScheme, BaseUrlWebHost).Uri;
    }
}
