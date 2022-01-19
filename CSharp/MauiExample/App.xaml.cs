using Application = Microsoft.Maui.Controls.Application;

namespace BridgeRock.MauiExample
{
    public partial class App : Application
	{
		public App()
		{
			InitializeComponent();

			MainPage = new MainPage();
		}
	}
}
