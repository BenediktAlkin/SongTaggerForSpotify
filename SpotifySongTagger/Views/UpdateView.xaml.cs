using Backend;
using Backend.Entities;
using SpotifySongTagger.ViewModels;
using MaterialDesignThemes.Wpf;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SpotifySongTagger.Views
{
    public partial class UpdateView : UserControl
    {
        public UpdateViewModel ViewModel { get; set; }

        public UpdateView()
        {
            InitializeComponent();
            ViewModel = new UpdateViewModel();
            DataContext = ViewModel;
        }
    }
}
