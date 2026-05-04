using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(SwimmingSchool_Implementation.Startup))]
namespace SwimmingSchool_Implementation
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
