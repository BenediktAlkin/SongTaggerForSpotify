using Backend;
using System;
using System.Collections.Generic;
using System.Text;

namespace Frontend.Wpf.ViewModels
{
    public class HomeViewModel : BaseViewModel
    {
        private bool rememberMe = true;
        public bool RememberMe
        {
            get => rememberMe;
            set => SetProperty(ref rememberMe, value, nameof(RememberMe));
        }
    }
}
