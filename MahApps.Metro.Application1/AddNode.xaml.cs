using System.Collections.Generic;
using System.Linq;
using MahApps.Metro.Controls;

namespace MahApps.Metro.Application1
{
    /// <summary>
    /// Interaction logic for AddNode.xaml
    /// </summary>
    public partial class AddNode : MetroWindow
    {
        public List<CheckedFunctionItem> ListOfComponent;

        public AddNode()
        {
            InitializeComponent();
        }

        public AddNode(List<CheckedFunctionItem> pListOfComponent)
        {
            InitializeComponent();

            ListOfComponent = pListOfComponent;

            List<string> CompNameList = ListOfComponent.Select(item => item.Name).ToList();
            comboBox.ItemsSource = CompNameList;
            comboBox_Copy.ItemsSource = CompNameList;
        }
    }
}
