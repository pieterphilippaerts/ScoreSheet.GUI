using System;
using System.Collections.Generic;
using System.Text;

namespace TabTService {
    /// <summary>
    /// Er zit (zat?) een bug in de TabT-API van GetMatches waardoor die illegale XML terugstuurde wanneer matchdetails opgevraagd worden.
    /// Specifiek zijn de datetime-strings van de CommentEntryType's die bij de matchdetails zitten fout geformateerd (niet volgens ISO8601).
    /// Dit probleem wordt opgelost door de gegenereerde code van de WSDL aan te passen en te zeggen dat Timestamp een string is (i.p.v. DateTime).
    /// Deze aanpassing staat echter hier, zodat deze fout niet gehergenereerd wordt wanneer er een update van de service reference gebeurt.
    /// In dit geval zal de WSDL code generator terug een Timestamp-veldje (van type DateTime) aanmaken in de CommentEntryType-klasse,
    /// wat een compile error zal opleveren door onderstaande code.
    /// </summary>
    public partial class CommentEntryType {
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Order = 0)]
        public string Timestamp {
            get {
                return this.timestampField;
            }
            set {
                this.timestampField = value;
            }
        }
        private string timestampField;
    }

    /// <summary>
    /// Dit hebben we nodig om zowel VTTL als AFTT te ondersteunen; gewoon de overeenkomstige types
    /// verwijderen uit Reference.cs
    /// </summary>
    public enum EndpointConfiguration {
        TabTAPI_Port,
        AfttAPI_Port
    }
    public partial class TabTAPI_PortTypeClient {
        private static System.ServiceModel.Channels.Binding GetBindingForEndpoint(EndpointConfiguration endpointConfiguration) {
            if ((endpointConfiguration == EndpointConfiguration.TabTAPI_Port) || endpointConfiguration == EndpointConfiguration.AfttAPI_Port) {
                System.ServiceModel.BasicHttpBinding result = new System.ServiceModel.BasicHttpBinding();
                result.MaxBufferSize = int.MaxValue;
                result.ReaderQuotas = System.Xml.XmlDictionaryReaderQuotas.Max;
                result.MaxReceivedMessageSize = int.MaxValue;
                result.AllowCookies = true;
                result.Security.Mode = System.ServiceModel.BasicHttpSecurityMode.Transport;
                return result;
            }
            throw new System.InvalidOperationException(string.Format("Could not find endpoint with name \'{0}\'.", endpointConfiguration));
        }

        private static System.ServiceModel.EndpointAddress GetEndpointAddress(EndpointConfiguration endpointConfiguration) {
            if ((endpointConfiguration == EndpointConfiguration.TabTAPI_Port)) {
                return new System.ServiceModel.EndpointAddress("https://api.vttl.be/index.php?s=vttl");
            }
            if ((endpointConfiguration == EndpointConfiguration.AfttAPI_Port)) {
                return new System.ServiceModel.EndpointAddress("https://resultats.aftt.be/api/tabtapi_main.php?s=aftt");
            }
            throw new System.InvalidOperationException(string.Format("Could not find endpoint with name \'{0}\'.", endpointConfiguration));
        }
    }
}
