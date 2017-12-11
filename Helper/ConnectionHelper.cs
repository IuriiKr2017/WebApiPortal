using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Tooling.Connector;
using System.Configuration;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

namespace B2BWebApi.Helper
{
    public class ConnectionHelper
    {
        public static IOrganizationService GetCrmConnection()
        {
            ServicePointManager.ServerCertificateValidationCallback = delegate (object s, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
            { return true; };

            CrmServiceClient conn = new CrmServiceClient(ConfigurationManager.ConnectionStrings["CRM"].ConnectionString);

            return conn.OrganizationWebProxyClient != null ? (IOrganizationService)conn.OrganizationWebProxyClient : conn.OrganizationServiceProxy;
        }   

    }

}