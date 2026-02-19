using LibraryMS.Win.Helper;

namespace LibraryMS
{
    internal static class Program
    {

        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            // Build services from appsettings.json
            //  var services = AppBootstrapper.Build();
            var services = AppBootstrapper.BuildAsync().GetAwaiter().GetResult();
            while (true)
            {
                using var login = new frmLogin(services.Location, services.Auth);
                if (login.ShowDialog() != DialogResult.OK)
                    break; // user cancelled login -> exit app

                using var main = new LibraryMS.Win.frmMainWindow(services.Menu, services.Registration, services.Approval, services.GroupMenus);
                // show main as dialog so we can return here on logout
                var result = main.ShowDialog();

                if (result != DialogResult.Retry)
                    break; // normal close -> exit app (or change behavior if you want)
                // Retry means logout -> loop again to show login
            }
        }
    }
}