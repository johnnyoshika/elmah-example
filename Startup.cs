using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(ElmahTests.Startup))]
namespace ElmahTests
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
