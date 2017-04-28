using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using RasPiBtControl.Services;

namespace RasPiBtControl.Droid.Services
{
    /// <summary>
    /// Displays a progress dialog
    /// </summary>
    public class ProgressDialogService : IProgressDialogService
    {
        private Activity _activity;
        private Android.App.ProgressDialog _currentDialog;

        public ProgressDialogService(Activity activity)
        {
            _activity = activity;
        }

        /// <summary>
        /// Shows the progress dialog
        /// </summary>
        /// <param name="message">The message to display</param>
        public void Show(string message)
        {
            if (_currentDialog != null)
            {
                this.Hide();
            }

            _currentDialog = new ProgressDialog(_activity);
            _currentDialog.Indeterminate = true;
            _currentDialog.SetProgressStyle(ProgressDialogStyle.Spinner);
            _currentDialog.SetMessage(message);
            _currentDialog.SetCancelable(false);
            _currentDialog.Show();
        }

        /// <summary>
        /// Hides the progress dialog
        /// </summary>
        public void Hide()
        {
            if (_currentDialog != null)
            {
                _currentDialog.Hide();
                _currentDialog = null;
            }
        }
    }
}