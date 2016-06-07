using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Microsoft.Owin.FileSystems;
using Microsoft.Owin.StaticFiles;
using Owin;

[assembly: OwinStartup(typeof(CLRH100Demo1WebUI.Startup))]

namespace CLRH100Demo1WebUI
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=316888
            app.MapSignalR();

            //var fileSystem = new PhysicalFileSystem(AppDomain.CurrentDomain.BaseDirectory);
            //app.UseStaticFiles(new StaticFileOptions
            //{
            //    FileSystem = fileSystem,
            //    ServeUnknownFileTypes = false
            //});
        }
    }
}
