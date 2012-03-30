using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StackExchange.Helpers
{
    /// <summary>
    /// Contains values that control when routes are added to the main <see cref="System.Web.Routing.RouteCollection"/>.  A route with Low priority will be registered after routes with Default and High priorities.
    /// </summary>
    /// <remarks>Routes with identical RoutePriority and RouteImportance are registered in alphabetical order.  RoutePriority allows for different strata of routes.</remarks>
    public enum RoutePriority
    {
        Lowest = 0,
        Low = 1,
        Default = 2,
        High = 3
    }
}
