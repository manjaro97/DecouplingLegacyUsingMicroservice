using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;
using System.Net;
using System.Xml.Linq;
using System.Text;

namespace DecouplingLegacyUsingMicroservice.Features.DataEndpoint;

[ApiController]
public class SoapController : ControllerBase
{
    DataEndpointService dataService;
    public SoapController(DataEndpointService _dataService)
    {
        dataService = _dataService;
    }

    [HttpPost("/sendSoapData")]
    public async Task<IActionResult> SendSoapReq()
    {
        string contentType = "application/xml; charset=utf-8"; //soap+xml

        if (Request.ContentType == null)
        {
            var response = new ContentResult
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Content = @"
                            <soap:Envelope xmlns:soap='http://schemas.xmlsoap.org/soap/envelope/'>
                               <soap:Body>
                                  <soap:Fault>
                                     <faultcode>soap:Client</faultcode>
                                     <faultstring>Bad Request</faultstring>
                                     <detail>
                                        <errorcode>400</errorcode>
                                        <errordescription>Content Type was null</errordescription>
                                     </detail>
                                  </soap:Fault>
                               </soap:Body>
                            </soap:Envelope>",
                ContentType = contentType,
            };
            return response;
        }
        if (Request.ContentType.Equals("application/xml", StringComparison.OrdinalIgnoreCase))
        {
            Data? parsedSoapData = null;
            try
            {
                // Read the SOAP request XML from the request body
                using (var reader = new StreamReader(Request.Body, Encoding.UTF8))
                {
                    var soapXml = await reader.ReadToEndAsync();

                    // Parse the SOAP XML using System.Xml.Linq
                    var xmlDoc = XDocument.Parse(soapXml);

                    // Extract SOAP data by querying the XML
                    var idElement = xmlDoc.Descendants("Id").FirstOrDefault();
                    var titleElement = xmlDoc.Descendants("Title").FirstOrDefault();
                    var descriptionElement = xmlDoc.Descendants("Description").FirstOrDefault();

                    if (idElement != null && titleElement != null && descriptionElement != null)
                    {
                        // Process the SOAP data and create a Data object
                        parsedSoapData = new Data
                        {
                            Id = int.Parse(idElement.Value),
                            Title = titleElement.Value,
                            Description = descriptionElement.Value
                        };
                    }
                }
            }
            catch
            {
                var response = new ContentResult
                {
                    StatusCode = (int)HttpStatusCode.InternalServerError, // Set the desired status code (e.g., 200 OK)
                    Content = @"
                            <soap:Envelope xmlns:soap='http://schemas.xmlsoap.org/soap/envelope/'>
                               <soap:Body>
                                  <soap:Fault>
                                     <faultcode>soap:Client</faultcode>
                                     <faultstring>Internal Server Error</faultstring>
                                     <detail>
                                        <errorcode>500</errorcode>
                                        <errordescription>Data validation crashed</errordescription>
                                     </detail>
                                  </soap:Fault>
                               </soap:Body>
                            </soap:Envelope>",
                    ContentType = contentType,
                };
                return response;
            }

            if (parsedSoapData != null && dataService.SendData(parsedSoapData))
            {
                var response = new ContentResult
                {
                    StatusCode = (int)HttpStatusCode.OK,
                    Content = @"
                            <soap:Envelope xmlns:soap='http://schemas.xmlsoap.org/soap/envelope/'>
                               <soap:Body>
                                  <ns1:Response xmlns:ns1='http://example.com/namespace'>
                                     <ns1:Status>Success</ns1:Status>
                                     <ns1:Message>Data was sent successfully</ns1:Message>
                                  </ns1:Response>
                               </soap:Body>
                            </soap:Envelope>",
                    ContentType = contentType,
                };
                return response;
            }
            else
            {
                var response = new ContentResult
                {
                    StatusCode = (int)HttpStatusCode.BadRequest,
                    Content = @"
                            <soap:Envelope xmlns:soap='http://schemas.xmlsoap.org/soap/envelope/'>
                               <soap:Body>
                                  <soap:Fault>
                                     <faultcode>soap:Client</faultcode>
                                     <faultstring>Bad Request</faultstring>
                                     <detail>
                                        <errorcode>400</errorcode>
                                        <errordescription>Data validation failed</errordescription>
                                     </detail>
                                  </soap:Fault>
                               </soap:Body>
                            </soap:Envelope>",
                    ContentType = contentType,
                };
                return response;
            }
        }
        else
        {
            var response = new ContentResult
            {
                StatusCode = (int)HttpStatusCode.BadRequest,
                Content = @"
                        <soap:Envelope xmlns:soap='http://schemas.xmlsoap.org/soap/envelope/'>
                               <soap:Body>
                                  <soap:Fault>
                                     <faultcode>soap:Client</faultcode>
                                     <faultstring>Bad Request</faultstring>
                                     <detail>
                                        <errorcode>400</errorcode>
                                        <errordescription>Content Type was not Soap</errordescription>
                                     </detail>
                                  </soap:Fault>
                               </soap:Body>
                            </soap:Envelope>",
                ContentType = contentType,
            };
            return response;
        }
    }
    
}
