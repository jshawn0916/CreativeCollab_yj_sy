using System.Web;
using System.Web.Mvc;

namespace CreativeCollab_yj_sy
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
