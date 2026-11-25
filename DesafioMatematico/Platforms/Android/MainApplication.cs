using Microsoft.Maui;
using Microsoft.Maui.Hosting;
using Android.App;
using Android.Runtime;
namespace DesafioMatematico;
[Application]
public class MainApplication : MauiApplication
{
    public MainApplication(nint handle, JniHandleOwnership ownership) : base(handle, ownership) { }
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}