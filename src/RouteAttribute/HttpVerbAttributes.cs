using System.Web.Mvc;

namespace StackExchange.Helpers
{
    public class GetAttribute : RouteAttribute
    {
        public GetAttribute(string url)
            : base(url, "", HttpVerbs.Get, RoutePriority.Default)
        {
        }

        public GetAttribute(string url, RoutePriority priority)
            : base(url, "", HttpVerbs.Get, priority)
        {
        }
    }

    public class PostAttribute : RouteAttribute
    {
        public PostAttribute(string url)
            : base(url, "", HttpVerbs.Post, RoutePriority.Default)
        {
        }

        public PostAttribute(string url, RoutePriority priority)
            : base(url, "", HttpVerbs.Post, priority)
        {
        }
    }

    public class PutAttribute : RouteAttribute
    {
        public PutAttribute(string url)
            : base(url, "", HttpVerbs.Put, RoutePriority.Default)
        {
        }

        public PutAttribute(string url, RoutePriority priority)
            : base(url, "", HttpVerbs.Put, priority)
        {
        }
    }

    public class DeleteAttribute : RouteAttribute
    {
        public DeleteAttribute(string url)
            : base(url, "", HttpVerbs.Delete, RoutePriority.Default)
        {
        }

        public DeleteAttribute(string url, RoutePriority priority)
            : base(url, "", HttpVerbs.Delete, priority)
        {
        }
    }

    /// <remarks>
    /// For the sake of completeness?
    /// </remarks>
    public class HeadAttribute : RouteAttribute 
    {
        public HeadAttribute(string url)
            : base(url, "", HttpVerbs.Head, RoutePriority.Default)
        {
        }

        public HeadAttribute(string url, RoutePriority priority)
            : base(url, "", HttpVerbs.Head, priority)
        {
        }
    }
}
