using ComponentsGenerator.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ComponentsGenerator.Views
{
    /// <summary>
    /// Interaction logic for BrowseWindow.xaml
    /// </summary>
    public partial class BrowseView : Window
    {
        private BrowseViewModel context;
        public BrowseView()
        {
            InitializeComponent();
            context = new BrowseViewModel();
            DataContext = context;                  
        }
    }
}
