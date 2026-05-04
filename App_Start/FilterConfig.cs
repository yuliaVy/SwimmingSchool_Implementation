using System.Web;
using System.Web.Mvc;

namespace SwimmingSchool_Implementation
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
