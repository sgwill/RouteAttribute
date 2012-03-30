using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Web.Mvc;
using System.Web.Routing;

namespace StackExchange.Helpers
{
    public static class RouteCollectionExtensions
    {
        /// <summary>
        /// Within the StackOverflow.dll assembly, looks for any action methods that have the RouteAttribute defined, 
        /// adding the routes to the parameter 'routes' collection.
        /// </summary>
        public static void MapDecoratedRoutes(this RouteCollection routes, Func<IDictionary<string, long>> getRouteWeights = null)
        {
            MapDecoratedRoutes(routes, Assembly.GetCallingAssembly(), getRouteWeights);
        }

        /// <summary>
        /// Looks for any action methods in 'assemblyToSearch' that have the RouteAttribute defined, 
        /// adding the routes to the parameter 'routes' collection.
        /// </summary>
        /// <param name="assemblyToSearch">An assembly containing Controllers with public methods decorated with the RouteAttribute</param>
        public static void MapDecoratedRoutes(this RouteCollection routes, Assembly assemblyToSearch, Func<IDictionary<string, long>> getRouteWeights = null)
        {
            var decoratedMethods = from t in assemblyToSearch.GetTypes()
                                   where t.IsSubclassOf(typeof(System.Web.Mvc.Controller))
                                   from m in t.GetMethods()
                                   where m.IsDefined(typeof(RouteAttribute), false)
                                   select m;

            var methodsToRegister = new SortedDictionary<RouteAttribute, MethodInfo>(); // sort urls alphabetically via RouteAttribute's IComparable implementation

            var routeWeights = (getRouteWeights == null ? null : getRouteWeights()) ?? new Dictionary<string, long>();

            // first, collect all the methods decorated with our RouteAttribute
            foreach (var method in decoratedMethods)
            {
                var attrs = method.GetCustomAttributes(typeof(RouteAttribute), true).ToList();

                foreach (var attr in attrs)
                {
                    var ra = (RouteAttribute)attr;
                    if (!methodsToRegister.Any(p => p.Key.Url.Equals(ra.Url)))
                    {
                        long weight;
                        string key = (method.DeclaringType.Name + "/" + method.Name).Replace("Controller/", "/");
                        if (routeWeights.TryGetValue(key, out weight)) ra.SetWeight(weight);
                        methodsToRegister.Add(ra, method);
                    }
                    else
                        Debug.WriteLine("MapDecoratedRoutes - found duplicate url -> " + ra.Url);
                }
            }
            Debug.WriteLine(string.Format("MapDecoratedRoutes - found {0} usable methods decorated with RouteAttribute", methodsToRegister.Count));

            // now register the unique urls to the Controller.Method that they were decorated upon
            foreach (var pair in methodsToRegister)
            {
                var attr = pair.Key;
                var method = pair.Value;
                var action = method.Name;

                var controllerType = method.ReflectedType;
                var controllerName = controllerType.Name.Replace("Controller", "");
                var controllerNamespace = controllerType.FullName.Replace("." + controllerType.Name, "");

                Debug.WriteLine(string.Format("MapDecoratedRoutes - mapping url '{0}' to {1}.{2}.{3}", attr.Url, controllerNamespace, controllerName, action));

                var route = new LeftMatchingRoute(attr.Url, new MvcRouteHandler());
                //var route = new Route(attr.Url, new MvcRouteHandler());
                route.Defaults = new RouteValueDictionary(new { controller = controllerName, action = action });

                // optional parameters are specified like: "users/filter/{filter?}"
                if (attr.OptionalParameters != null)
                {
                    foreach (var optional in attr.OptionalParameters)
                        route.Defaults.Add(optional, "");
                }

                // constraints are specified like: @"users/{id:\d+}" or "users/{id:INT}"
                if (attr.Constraints != null)
                {
                    route.Constraints = new RouteValueDictionary();

                    foreach (var constraint in attr.Constraints)
                    {
                        route.Constraints.Add(constraint.Key, constraint.Value);
                    }
                }

                // fully-qualify route to its controller method by adding the namespace; allows multiple assemblies to share controller names/routes
                // e.g. StackOverflow.Controllers.HomeController, StackOverflow.Api.Controllers.HomeController
                route.DataTokens = new RouteValueDictionary(new { namespaces = new[] { controllerNamespace } });

                routes.Add(attr.Name, route);
            }
        }
    }
}
