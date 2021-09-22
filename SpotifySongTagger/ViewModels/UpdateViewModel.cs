using Backend;
using System;
using System.Collections.Generic;
using System.Text;
using Util;

namespace SpotifySongTagger.ViewModels
{
    public class UpdateViewModel : BaseViewModel
    {
        public UpdateManager UpdateManager => UpdateManager.Instance;
    }
}
