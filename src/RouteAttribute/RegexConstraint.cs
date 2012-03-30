using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Web.Routing;

namespace StackExchange.Helpers
{
    internal class RegexConstraint : IRouteConstraint, IEquatable<RegexConstraint>
    {
        Regex regex;
        string pattern;

        public RegexConstraint(string pattern, RegexOptions options = RegexOptions.CultureInvariant | RegexOptions.Compiled | RegexOptions.IgnoreCase)
        {
            regex = new Regex(pattern, options);
            this.pattern = pattern;
        }

        public bool Match(System.Web.HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
        {
            object val;
            values.TryGetValue(parameterName, out val);
            string input = Convert.ToString(val, CultureInfo.InvariantCulture);
            var result = regex.IsMatch(input);
            return result;
        }

        public string Pattern
        {
            get
            {
                return pattern;
            }
        }

        public RegexOptions RegexOptions
        {
            get
            {
                return regex.Options;
            }
        }

        private string Key
        {
            get
            {
                return regex.Options.ToString() + " | " + pattern;
            }
        }

        public override int GetHashCode()
        {
            return Key.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var other = obj as RegexConstraint;
            if (other == null) return false;
            return Key == other.Key;
        }

        public bool Equals(RegexConstraint other)
        {
            return this.Equals((object)other);
        }

        public override string ToString()
        {
            return "RegexConstraint (" + Pattern + ")";
        }
    }
}
