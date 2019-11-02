//--------------------------------------------------------------------------------------------------
// Copyright 2018 DataArt, Inc.
// -------------------------------------------------------------------------------------------------
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this work except in compliance with the License.
// You may obtain a copy of the License in the LICENSE file, or at:
//
//  http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//--------------------------------------------------------------------------------------------------
#if NETSTANDARD2_0
using System;
using System.Fabric;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace DataArt.Atlas.Azure.Hosting.Fabric
{
    internal class Certificate
    {
        public static X509Certificate2 FindBindedCertificate(ServiceFabricApplication fabricApplication, string applicationTypeName, string serviceManifestName)
        {
            using (var client = new FabricClient())
            {
                var manifest =
                    client.ApplicationManager.GetApplicationManifestAsync(
                            applicationTypeName,
                            fabricApplication.GetApplicationVersionFunction()).ConfigureAwait(false).GetAwaiter()
                        .GetResult();

                var doc = XDocument.Parse(manifest);
                var ns = doc.Root?.GetDefaultNamespace();

                var certRefs = from manifestImport in doc.Root?.Descendants(ns + "ServiceManifestImport")
                               where manifestImport.Element(ns + "ServiceManifestRef")?.Attribute("ServiceManifestName")?.Value ==
                                     serviceManifestName
                               select manifestImport.Element(ns + "Policies")?.Element(ns + "EndpointBindingPolicy")?
                                   .Attribute("CertificateRef")?.Value;

                var certificates = certRefs.ToList();

                if (!certificates.Any())
                {
                    throw new InvalidProgramException("Unable to find https CertificateRef");
                }

                var certRef = certificates[0];

                var eCertificates = from certificate in doc.Root?.Descendants(ns + "Certificates")
                                    let endpointCertificate = certificate.Element(ns + "EndpointCertificate")
                                    where endpointCertificate?.Attribute("Name")?.Value == certRef
                                    select endpointCertificate;

                var endpointCertificates = eCertificates.ToList();

                if (!endpointCertificates.Any())
                {
                    throw new InvalidProgramException("Unable to find https EndpointCertificate");
                }

                var thumbprint = endpointCertificates[0].Attribute("X509FindValue")?.Value;

                if (thumbprint == null)
                {
                    throw new InvalidProgramException("Https EndpointCertificate X509FindValue not found");
                }

                var regexItem = new Regex("^[a-zA-Z0-9 ]+$");

                if (!regexItem.IsMatch(thumbprint))
                {
                    thumbprint = fabricApplication.Application?.ApplicationParameters[thumbprint.Split('[', ']')[1]].Value;
                }

                return GetCertificateFromStore(
                    thumbprint,
                    endpointCertificates[0].Attribute("X509StoreName")?.Value);
            }
        }

#pragma warning disable SA1313 // Parameter names must begin with lower-case letter // ReSharper disable InconsistentNaming
        private static X509Certificate2 GetCertificateFromStore(string X509FindValue, string X509StoreName = null)
#pragma warning restore SA1313 // Parameter names must begin with lower-case letter
        {
            if (string.IsNullOrEmpty(X509StoreName) || !Enum.TryParse(X509StoreName, out StoreName myStatus))
            {
                myStatus = StoreName.My;
            }

            var store = new X509Store(myStatus, StoreLocation.LocalMachine);

            try
            {
                store.Open(OpenFlags.ReadOnly);
                var certCollection = store.Certificates;
                var currentCerts = certCollection.Find(X509FindType.FindByThumbprint, X509FindValue, false);
                return currentCerts.Count == 0 ? null : currentCerts[0];
            }
            finally
            {
                store.Close();
            }
        }
    }
}
#endif