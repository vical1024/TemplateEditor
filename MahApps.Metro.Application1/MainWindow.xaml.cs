using MahApps.Metro.Controls;
using System;
using System.Windows;
using System.Windows.Forms;
using System.IO;
using System.Xml;
using System.Collections.Generic;

namespace MahApps.Metro.Application1
{
    public partial class MainWindow : MetroWindow
    {
        public List<TemplateData> templateDataList;

        public MainWindow()
        {
            templateDataList = new List<TemplateData>();

            InitializeComponent();
        }

        private void Button_Directory_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog DirectoryDialog = new FolderBrowserDialog();
            DirectoryDialog.ShowDialog();
            TextBox_Directory.Text = DirectoryDialog.SelectedPath;

            ListBox_FunctionListBox_Refresh();
        }

        private void TextBox_Directory_LostFocus(object sener, RoutedEventArgs e)
        {


            ListBox_FunctionListBox_Refresh();
        }

        private void Button_Delete_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Button_UnselectedComponentDelete_Click(object sender, RoutedEventArgs e)
        {

        }

        private List<XmlNode> GetDeleteNodeList(XmlNodeList NodeList, string TargetComponent)
        {
            List<XmlNode> DeleteNodeList = new List<XmlNode>();

            foreach (XmlNode Node in NodeList)
            {
                if (Node.Name == "ELEMENT-NAME" && Node.InnerText == TargetComponent)
                {
                    DeleteNodeList.Add(Node.ParentNode);
                }
                else if (Node.Name == "OWNER-ELEMENT" && Node.ParentNode.ParentNode.Name.Contains("-CONNECTION") && Node.InnerText == TargetComponent)
                {
                    DeleteNodeList.Add(Node.ParentNode.ParentNode);
                }

                if (Node.HasChildNodes == true)
                {
                    DeleteNodeList.AddRange(GetDeleteNodeList(Node.ChildNodes, TargetComponent));
                }
            }

            return DeleteNodeList;
        }

        private void CheckBox_CheckedUnchecked(object sender, RoutedEventArgs e)
        {
            ListBox_ComponentListBox_Refresh();
        }

        void ListBox_FunctionListBox_Refresh()
        {
            //renewal templateDataList
            templateDataList.RemoveRange(0, templateDataList.Count);
            templateDataList = TemplateSearch(TextBox_Directory.Text);

            //renewal listbox
            ListBox_FileList.ItemsSource = null;
            ListBox_FileList.ItemsSource = templateDataList;

            ListBox_ComponentListBox_Refresh();
        }

        void ListBox_ComponentListBox_Refresh()
        {
            foreach (CheckedFunctionItem item in ListOfFunction)
            {
                if (item.IsChecked == true)
                {
                    XmlDocument TargetXmlDoc = new XmlDocument();

                    TargetXmlDoc.Load(item.FileFullName);
                    XmlElement Root = TargetXmlDoc.DocumentElement;
                }
            }

            ListBox_ComponentList.ItemsSource = null;
            ListBox_ComponentList.ItemsSource = ListOfComponent;

            ListBox_InterfaceListBox_Refresh();
        }

        void ListBox_InterfaceListBox_Refresh()
        {
            ListOfInterface.RemoveRange(0, ListOfInterface.Count);

            foreach (CheckedFunctionItem fitem in ListOfFunction)
            {
                if (fitem.IsChecked == true)
                {
                    XmlDocument TargetXmlDoc = new XmlDocument();

                    TargetXmlDoc.Load(fitem.FileFullName);
                    XmlElement Root = TargetXmlDoc.DocumentElement;

                    foreach (CheckedFunctionItem citem in ListOfComponent)
                    {
                        InterfaceSearch(Root.ChildNodes, citem.Name);
                    }
                    
                }
            }

            DataGrid_Interface.ItemsSource = null;
            DataGrid_Interface.ItemsSource = ListOfInterface;
        }

        private List<TemplateData> TemplateSearch(string path)
        {
            try
            {
                List<TemplateData> templateDataList = new List<TemplateData>();

                foreach (string file in Directory.GetFiles(path, ".xml"))
                {
                    TemplateData templateData = new TemplateData(this, file);
                    templateData.function.Name = file.Replace(TextBox_Directory.Text + @"\", "");
                    templateData.function.IsChecked = true;
                   
                    templateDataList.Add(templateData);
                }

                foreach (string directory in Directory.GetDirectories(path))
                {
                    templateDataList.AddRange(TemplateSearch(directory));
                }

                return templateDataList;
            }
            catch (System.Exception excpt)
            {
                Console.WriteLine(excpt.Message);

                return null;
            }
        }

        

        private void Button_AddComponent_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Button_AddInterface_Click(object sender, RoutedEventArgs e)
        {
            AddNode AddNodeWindow = new AddNode(ListOfComponent);
            AddNodeWindow.ShowDialog();
        }

        private void Button_AddFunction_Click(object sender, RoutedEventArgs e)
        {

        }

        private void InterfaceSearch(XmlNodeList NodeList, string ComponentName)
        {
            foreach (XmlNode Node in NodeList)
            {
                if (Node.Name == "INTER-FUNCTION-SIGNAL-CONNECTION" || Node.Name == "INTRA-FUNCTION-SIGNAL-CONNECTION")
                {
                    string ProviderName = Node["PROVIDER-REF"]["OWNER-ELEMENT"].InnerText;
                    string ReceiverName = Node["REQUESTER-REF"]["OWNER-ELEMENT"].InnerText;

                    if (ProviderName == ComponentName || ReceiverName == ComponentName)
                    {
                        string InterfaceName = Node["PROVIDER-REF"]["TARGET-P-PORT"].InnerText.Substring(2);

                        if (ListOfInterface.Exists(i => ((i.Interface == InterfaceName) && (i.Provider == ProviderName) && (i.Receiver == ReceiverName))) == false)
                        {
                            CheckedInterfaceItem item = new CheckedInterfaceItem();
                            item.Provider  = ProviderName;
                            item.Receiver  = ReceiverName;
                            item.Interface = InterfaceName;
                            item.IsChecked = true;
                            ListOfInterface.Add(item);
                        }
                    }
                }

                if (Node.HasChildNodes == true)
                {
                    InterfaceSearch(Node.ChildNodes, ComponentName);
                }
            }
        }

        private void Button_UnselectedInterfaceDelete_Click(object sender, RoutedEventArgs e)
        {

        }
    }

    #region data class

    public class TemplateData
    {
        #region member

        public MainWindow parentMainWindow;
        public string fileName { get; set; }
        public FunctionData function { get; set; }

        #endregion

        #region constructor
        public TemplateData(MainWindow parentMainWindow, string fileName)
        {
            this.parentMainWindow = parentMainWindow;

            function = new FunctionData(this, fileName);

            this.fileName = fileName;
        }
        #endregion
    }

    public class FunctionData
    {
        #region member

        public TemplateData parentTemplateData { get; set; }

        public string Name { get; set; }
        public bool IsChecked { get; set; }

        public List<ElementData> elementList { get; set; }

        #endregion

        #region constructor 

        public FunctionData(TemplateData parentTemplateData, string fileName)
        {
            this.parentTemplateData = parentTemplateData;

            elementList = new List<ElementData>();

            XmlDocument TargetXmlDoc = new XmlDocument();

            TargetXmlDoc.Load(fileName);
            XmlElement Root = TargetXmlDoc.DocumentElement;
            ElementSearch(Root.ChildNodes);
        }

        #endregion

        #region method

        private void ElementSearch(XmlNodeList nodeList)
        {
            foreach (XmlNode Node in nodeList)
            {
                if (Node.Name == "ELEMENT-NAME" || (Node.Name == "OWNER-ELEMENT" && Node.ParentNode.ParentNode.Name.Contains("-CONNECTION")))
                {
                    if (elementList.Exists(i => i.name == Node.InnerText) == false)
                    {
                        ElementData item = new ElementData(this, Node.InnerText, nodeList);
                        item.isChecked = true;

                        elementList.Add(item);
                    }
                }

                if (Node.HasChildNodes == true)
                {
                    ElementSearch(Node.ChildNodes);
                }
            }
        }

        #endregion
    }

    public class ElementData
    {
        public FunctionData parentFunctionData;

        public string name { get; set; }
        public bool isChecked { get; set; }

        public List<InterfaceData> interfacesList { get; set; }

        public ElementData(FunctionData parentFunctionData, string name, XmlNodeList nodeList)
        {
            this.parentFunctionData = parentFunctionData;

            interfacesList = new List<InterfaceData>();

            this.name = name;

            InterfaceSearch(nodeList);
        }

        private void InterfaceSearch(XmlNodeList nodeList)
        {
            foreach (XmlNode Node in nodeList)
            {
                if (Node.Name == "INTER-FUNCTION-SIGNAL-CONNECTION" || Node.Name == "INTRA-FUNCTION-SIGNAL-CONNECTION")
                {
                    string ProviderName = Node["PROVIDER-REF"]["OWNER-ELEMENT"].InnerText;
                    string ReceiverName = Node["REQUESTER-REF"]["OWNER-ELEMENT"].InnerText;

                    if (ProviderName == this.name || ReceiverName == this.name)
                    {
                        string InterfaceName = Node["PROVIDER-REF"]["TARGET-P-PORT"].InnerText.Substring(2);

                        if (interfacesList.Exists(i => ((i.name == InterfaceName) && (i.provider.name == ProviderName) && (i.receiver.name == ReceiverName))) == false)
                        {
                            InterfaceData item = new InterfaceData(this);

                            foreach (TemplateData template in this.parentFunctionData.parentTemplateData.parentMainWindow.templateDataList)
                            {
                                foreach (ElementData element in template.function.elementList)
                                {

                                }
                            }
                        }
                    }
                }

                if (Node.HasChildNodes == true)
                {
                    InterfaceSearch(Node.ChildNodes);
                }
            }
        }
    }

    public class InterfaceData
    {
        public ElementData parentElementData;

        public string name { get; set; }
        public string isChecked { get; set; }

        public ElementData provider { get; set; }
        public ElementData receiver { get; set; }

        public InterfaceData(ElementData parentElementData)
        {
            this.parentElementData = parentElementData;
        }
    }
    #endregion

}
