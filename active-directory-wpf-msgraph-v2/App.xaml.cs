using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Identity.Client;
using Microsoft.Identity.Client.Broker;
using Microsoft.Identity.Client.Extensions.Msal;

namespace active_directory_wpf_msgraph_v2
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>

    // To change from Microsoft public cloud to a national cloud, use another value of AzureCloudInstance
    public partial class App : Application
    {
        // Below are the clientId (Application Id) of your app registration and the tenant information.
        // You have to replace these values in App.config:
        // - the content of ClientID with the Application Id for your app registration
        // - The content of Tenant by the information about the accounts allowed to sign-in in your application:
        //   - For Work or School account in your org, use your tenant ID, or domain
        //   - for any Work or School accounts, use organizations
        //   - for any Work or School accounts, or Microsoft personal account, use common
        //   - for Microsoft Personal account, use consumers
        private static string ClientId = ConfigurationManager.AppSettings["ClientId"];

        // Note: Tenant is important for the quickstart.
        private static string Tenant = ConfigurationManager.AppSettings["Tenant"];
        private static string Instance = "https://login.microsoftonline.com/";
        private static IPublicClientApplication _clientApp;
        private static bool _useAlternateAuth = false;

        public static IPublicClientApplication PublicClientApp { get { return _clientApp; } }
        public static bool UseAlternateAuth
        {
            get { return _useAlternateAuth; }
            set
            {
                if (_useAlternateAuth != value)
                {
                    _useAlternateAuth = value;
                    active_directory_wpf_msgraph_v2.Properties.Settings.Default.UseAlternateAuth = value;
                    active_directory_wpf_msgraph_v2.Properties.Settings.Default.Save();
                    CreateApplication();
                }
            }
        }

        static App()
        {
            // Load the saved authentication preference
            _useAlternateAuth = active_directory_wpf_msgraph_v2.Properties.Settings.Default.UseAlternateAuth;
            CreateApplication();
        }

        public static void CreateApplication()
        {
            if (_useAlternateAuth)
            {
                // Alternate authentication approach using WithTenantId and WithAuthority
                var builder = PublicClientApplicationBuilder.Create(ClientId)
                    .WithTenantId(Tenant)
                    .WithAuthority(AzureCloudInstance.AzurePublic, AadAuthorityAudience.AzureAdMyOrg);

                var brokerOptions = new BrokerOptions(BrokerOptions.OperatingSystems.Windows);
                builder = builder.WithBroker(brokerOptions)
                    .WithDefaultRedirectUri();

                _clientApp = builder.Build();
            }
            else
            {
                // Original authentication approach
                BrokerOptions brokerOptions = new BrokerOptions(BrokerOptions.OperatingSystems.Windows);

                _clientApp = PublicClientApplicationBuilder.Create(ClientId)
                    .WithAuthority($"{Instance}{Tenant}")
                    .WithDefaultRedirectUri()
                    .WithBroker(brokerOptions)
                    .Build();
			}

			MsalCacheHelper cacheHelper = CreateCacheHelperAsync().GetAwaiter().GetResult();

			// Let the cache helper handle MSAL's cache, otherwise the user will be prompted to sign-in every time.
			cacheHelper.RegisterCache(_clientApp.UserTokenCache);
		}

        private static async Task<MsalCacheHelper> CreateCacheHelperAsync()
        {
            // Since this is a WPF application, only Windows storage is configured
            var storageProperties = new StorageCreationPropertiesBuilder(
                              System.Reflection.Assembly.GetExecutingAssembly().GetName().Name + ".msalcache.bin",
                              MsalCacheHelper.UserRootDirectory)
                                .Build();

            MsalCacheHelper cacheHelper = await MsalCacheHelper.CreateAsync(
                        storageProperties,
                        new TraceSource("MSAL.CacheTrace"))
                     .ConfigureAwait(false);

            return cacheHelper;
        }
    }
}
