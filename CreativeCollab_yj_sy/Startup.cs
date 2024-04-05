using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(CreativeCollab_yj_sy.Startup))]
namespace CreativeCollab_yj_sy
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
