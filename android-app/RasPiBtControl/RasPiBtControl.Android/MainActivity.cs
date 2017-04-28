using System;
using System.Linq;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Splat;
using RasPiBtControl.Droid.Services;
using RasPiBtControl.Services;
using Android.Support.V4.Content;
using Android;
using Android.Support.V4.App;

namespace RasPiBtControl.Droid
{
    [Activity(Label = "RasPiBtControl", Icon = "@drawable/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            // Register native implementations for different services
            Locator.CurrentMutable.Register(() => new ProgressDialogService(this), typeof(IProgressDialogService));
            Locator.CurrentMutable.Register(() => new BtDiscovery(this), typeof(IBtDiscovery));
            Locator.CurrentMutable.Register(() => new BtClient(this), typeof(IBtClient));

            base.OnCreate(bundle);

            global::Xamarin.Forms.Forms.Init(this, bundle);
            CheckBluetoothPermissions();
        }

        /// <summary>
        /// MAke sure that all requested permissions are available
        /// </summary>
        private void CheckBluetoothPermissions()
        {
            if (ContextCompat.CheckSelfPermission(this, Manifest.Permission.Bluetooth) == Permission.Granted &&
                ContextCompat.CheckSelfPermission(this, Manifest.Permission.BluetoothAdmin) == Permission.Granted)
            {   
                LoadApplication(new App());
            }
            else
            {
                if (ActivityCompat.ShouldShowRequestPermissionRationale(this, Manifest.Permission.Bluetooth) ||
                    ActivityCompat.ShouldShowRequestPermissionRationale(this, Manifest.Permission.BluetoothAdmin))
                {
                    var messageBuilder = new AlertDialog.Builder(this);
                    messageBuilder.SetMessage("Bluetooth permissions are required to connect to the Raspberry PI")
                        .SetCancelable(true)
                        .SetPositiveButton("OK", (sender, args) =>
                        {
                            this.RequestPermissions(new[] { Manifest.Permission.Bluetooth, Manifest.Permission.BluetoothAdmin }, 0);
                        })
                        .Show();

                    return;
                }

                ActivityCompat.RequestPermissions(this, new[] { Manifest.Permission.Bluetooth, Manifest.Permission.BluetoothAdmin }, 0);
            }
        }
        
        /// <summary>
        /// Handle the reponse from the pemissions request
        /// </summary>
        /// <param name="requestCode"></param>
        /// <param name="permissions"></param>
        /// <param name="grantResults"></param>
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
        {
            if (grantResults.All(p => p == Permission.Granted))
            {
                LoadApplication(new App());
            }
            else
            {
                this.Finish();
            }
        }
    }
}

