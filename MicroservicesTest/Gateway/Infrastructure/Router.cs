using Gateway.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Gateway.Infrastructure {
    public class Router {
        public List<Route> Routes { get; set; }
        public Destination AuthenticationService { get; set; }


        public Router(string routeConfigFilePath) {
            dynamic router = JsonLoader.LoadFromFile<dynamic>(routeConfigFilePath);

            Routes = JsonLoader.Deserialize<List<Route>>(Convert.ToString(router.routes));
            AuthenticationService = JsonLoader.Deserialize<Destination>(Convert.ToString(router.authenticationService));

        }
        public Router(List<Route> routes, Destination authService = null) {
            Routes = routes;
            if (authService != null) {
                AuthenticationService = authService;
            }
        }
        public async Task<HttpResponseMessage> RouteRequest(HttpRequest request) {
            string path = request.Path.ToString();
            string basePath = '/' + path.Split('/')[1];

            Destination destination;
            try {
                destination = Routes.First(r => r.Endpoint.Equals(basePath)).Destination;
            }
            catch {
                return ConstructErrorMessage("The path could not be found.");
            }

            if (destination.RequiresAuthentication) {
                string token = request.Headers["token"];
                request.Query.Append(new KeyValuePair<string, StringValues>("token", new StringValues(token)));
                HttpResponseMessage authResponse = await AuthenticationService.SendRequest(request);
                if (!authResponse.IsSuccessStatusCode) return ConstructErrorMessage("Authentication failed.");
            }

            return await destination.SendRequest(request);
        }
        public async Task<string> ProcessRequest(HttpRequest request) {
            string path = request.Path.ToString();
            string basePath = '/' + path.Split('/')[1];

            Destination destination;
            try {
                destination = Routes.First(r => r.Endpoint.Equals(basePath)).Destination;
                if (destination.RequiresAuthentication) {
                    string token = request.Headers["token"];
                    request.Query.Append(new KeyValuePair<string, StringValues>("token", new StringValues(token)));
                    HttpResponseMessage authResponse = await AuthenticationService.SendRequest(request);
                    if (!authResponse.IsSuccessStatusCode) return await ConstructErrorMessage("Authentication failed.").Content.ReadAsStringAsync();
                }
                var res = await destination.SendRequest(request);
                return await res.Content.ReadAsStringAsync();
            }
            catch(Exception ex) {
                Debug.Write(ex.Message);
                return await ConstructErrorMessage("The path could not be found.").Content.ReadAsStringAsync();
            }
        }

        private HttpResponseMessage ConstructErrorMessage(string error) {
            HttpResponseMessage errorMessage = new HttpResponseMessage {
                StatusCode = HttpStatusCode.NotFound,
                Content = new StringContent(error)
            };
            return errorMessage;
        }
    }
}
