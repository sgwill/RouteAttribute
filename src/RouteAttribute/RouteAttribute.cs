using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using System.Web.Routing;

namespace StackExchange.Helpers
{
    /// <summary>
    /// Allows MVC routing urls to be declared on the action they map to.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class RouteAttribute : ActionMethodSelectorAttribute, IComparable<RouteAttribute>
    {
        public RouteAttribute(string url)
            : this(url, "", null, RoutePriority.Default)
        {
        }

        public RouteAttribute(string url, HttpVerbs verbs)
            : this(url, "", verbs, RoutePriority.Default)
        {
        }

        public RouteAttribute(string url, RoutePriority priority)
            : this(url, "", null, priority)
        {
        }

        public RouteAttribute(string url, HttpVerbs verbs, RoutePriority priority)
            : this(url, "", verbs, priority)
        {
        }

        internal RouteAttribute(string url, string name, HttpVerbs? verbs, RoutePriority priority)
        {
            Url = url.ToLower();
            Name = name;
            AcceptVerbs = verbs;
            Priority = priority;
        }

        private long _weight;
        internal void SetWeight(long value)
        {
            _weight = value;
        }

        /// <summary>
        /// The explicit verbs that the route will allow.  If null, all verbs are valid.
        /// </summary>
        public HttpVerbs? AcceptVerbs { get; set; }

        /// <summary>
        /// Optional name to allow this route to be referred to later.
        /// </summary>
        public string Name { get; set; }

        private string _url;
        /// <summary>
        /// The request url that will map to the decorated action method.
        /// Specifying optional parameters: "/users/{id}/{name?}" where 'name' may be omitted.
        /// Specifying constraints on parameters: "/users/{id:(\d+)}" where 'id' matches a regex for at least one number
        /// Constraints can also be predefined: "/users/{id:INT}" where 'id' will be constrained to the predefined INT regex <see cref="PredefinedConstraints"/>.
        /// </summary>
        public string Url
        {
            get { return _url; }
            set { _url = ParseUrlForConstraints(value); /* side-effects include setting this.OptionalParameters and this.Constraints */ }
        }

        /// <summary>
        /// Determines when this route is registered in the <see cref="System.Web.Routing.RouteCollection"/>.  The higher the priority, the sooner
        /// this route is added to the collection, making it match before other registered routes for a given url.
        /// </summary>
        public RoutePriority Priority { get; set; }

        /// <summary>
        /// Gets any optional parameters contained by this Url. Optional parameters are specified with a ?, e.g. "users/{id}/{name?}".
        /// </summary>
        public string[] OptionalParameters { get; private set; }

        /// <summary>
        /// Based on /users/{id:(\d+)(;\d+)*}
        /// </summary>
        public Dictionary<string, IRouteConstraint> Constraints { get; private set; }

        /// <summary>
        /// Contains keys that can be used in routes for well-known constraints, e.g. "users/{id:INT}" - this route would ensure the 'id' parameter
        /// would only accept at least one number to match.
        /// </summary>
        public static readonly IDictionary<string, string> PredefinedConstraints = new Dictionary<string, string> 
        { 
            { "INT",            @"-?\d+" },
            { "INTS_DELIMITED", @"-?\d+(;-?\d+)*" },
            { "GUID",           @"\b[A-Fa-f0-9]{8}(?:-[A-Fa-f0-9]{4}){3}-[A-Za-z0-9]{12}\b" }
        };

        public override bool IsValidForRequest(ControllerContext cc, MethodInfo mi)
        {
            bool result = true;

            if (AcceptVerbs.HasValue)
                result = new AcceptVerbsAttribute(AcceptVerbs.Value).IsValidForRequest(cc, mi);

            return result;
        }

        public override string ToString()
        {
            return (AcceptVerbs.HasValue ? AcceptVerbs.Value.ToString().ToUpper() + " " : "") + Url;
        }

        public int CompareTo(RouteAttribute other)
        {
            // note these are reversed, to order descending
            var result = ((int)other.Priority).CompareTo((int)this.Priority);
            if (result == 0) result = other._weight.CompareTo(this._weight);

            if (result == 0) // sort like priorities in asc alphabetical order
                result = this.Url.CompareTo(other.Url);

            return result;
        }

        private static HashSet<RegexConstraint> cachedConstraints = new HashSet<RegexConstraint>();

        private static Regex parseUrlForConstraintsRegex = new Regex(@"{(?<param>\w+)(?<metadata>(?<optional>\?)?(?::(?<constraint>[^}]*))?)}", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private string ParseUrlForConstraints(string url)
        {
            // example url with both optional specifier and a constraint: "posts/{id:INT}/edit-submit/{revisionguid?:GUID}"
            // note that a constraint regex cannot use { } for quantifiers
            var regex = parseUrlForConstraintsRegex;
            var matches = regex.Matches(url);

            if (matches.Count == 0) return url; // vanilla route without any parameters, e.g. "home", "users/login"

            var result = regex.Replace(url, @"{${param}}");
            var optionals = new List<string>();
            var constraints = new Dictionary<string, IRouteConstraint>();

            foreach (Match m in matches)
            {
                var metadata = m.Groups["metadata"].Value; // all the extra info after the parameter name
                if (!String.IsNullOrEmpty(metadata)) // we have optional specifier and/or constraints
                {
                    var param = m.Groups["param"].Value; // the name, e.g. 'id' in "/users/{id}"
                    var isOptional = m.Groups["optional"].Success;

                    if (isOptional)
                        optionals.Add(param);

                    var constraint = m.Groups["constraint"].Value;
                    if (!String.IsNullOrEmpty(constraint))
                    {
                        string predefined = null;
                        RegexConstraint actual = null;
                        if (PredefinedConstraints.TryGetValue(constraint.ToUpper(), out predefined))
                        {
                            constraint = predefined;
                        }

                        if (isOptional)
                        {
                            constraint = "(" + constraint + ")?";
                        }

                        actual = new RegexConstraint("^(" + constraint + ")$");

                        // reuse constraints where possible
                        if (cachedConstraints.Contains(actual))
                        {
                            actual = cachedConstraints.First(c => c.Equals(actual));
                        }
                        else
                        {
                            cachedConstraints.Add(actual);
                        }

                        constraints.Add(param, actual);
                    }
                }
            }

            if (optionals.Count > 0) this.OptionalParameters = optionals.ToArray();
            if (constraints.Count > 0) this.Constraints = constraints;

            return result;
        }
    }
}
