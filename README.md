RouteAttribute
===

NOTE: this readme is a work in progress.  For real.

The RouteAttribute is a non convention based routing mechanism for asp.net mvc.  The goals are to increase route meta data terseness and locality.  You can read about why [Kevin Montrose](https://github.com/kevin-montrose) likes this style of routing [here](http://kevinmontrose.com/2011/07/25/why-i-love-attribute-based-routing/).

##Core features:##

 - Places the url next to the action code executed when that route is matched
 - Supports custom route weighting
 
Custom route weighting allows you
    
 - Common routing funniness is gone
 
The most obvious example of this occurs when using the default route that gets added when you roll up a new project.  

    routes.MapRoute(
        name: "Default",
        url: "{controller}/{action}/{id}",
        defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
    );
    
For CRUD type applications, you generally have an index route that lists all or some subset of the items, then you click on one or something and it brings you to a details page.  With the default convention based route, all of the following routes point to the list page:

    /
    /Users
    /Users/Index
    /Users/Index/123
    
When an id is specified to go to the index page, it still shows the list, which feels a little off.  With convention based routing, you are creating a ton of valid routes such as the example listed above in your application which may not act like your users expect.  You also run into a canonicalization problems with conventions because all of the above routes expose the exact same data on multiple pages of your site.

##Key limitations:##

 - Currently does not support XSRF prevention
    
    The 
 
 - ASP.NET MVC does not handle a large number of routes very well