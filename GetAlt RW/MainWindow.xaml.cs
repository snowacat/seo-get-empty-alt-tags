using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Threading;

using HtmlAgilityPack;


namespace GetAlt_RW
{
    public partial class MainWindow : Window
    {
        String FILE_LOCATION = @"C:\git-folder\image-list.txt";
        String MAIN_NODE = "https://weezlabs.com";
        Int32 COUNT_EMPTY_TAGS = 0;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void OpenFile_Click(Object sender, RoutedEventArgs e)
        {

            Microsoft.Win32.OpenFileDialog fileDialog = new Microsoft.Win32.OpenFileDialog();
            fileDialog.Title = "File Site Map";
            fileDialog.Filter = "All files (*.*)|*.*|All files (*.*)|*.*";

            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<Boolean> result = fileDialog.ShowDialog();

            // Get the selected file name
            if (result == true)
            {
                String fileContent = File.ReadAllText(fileDialog.FileName);
                FindTags(fileContent);
            }
        }

        public void FindTags(String fileContent)
        {
            // Pase URL's froms Site Map
            List<String> urlsList = parseUrls(fileContent);

            // Parse html pages
            ParseHtml(urlsList);

            OutputTextBlock.AppendText("Finished!\n Your file: " + FILE_LOCATION + "\nCount empty alt attributes = " + COUNT_EMPTY_TAGS);
        }

        public void CreateFile(String fileLocation)
        {
            try
            {
                // Check if file already exists. If yes, delete it. 
                if (File.Exists(fileLocation))
                {
                    File.Delete(fileLocation);
                }

                // Create a new file 
                using (StreamWriter sw = File.CreateText(fileLocation))
                {
                    sw.WriteLine("New file created: {0}", DateTime.Now.ToString());
                }
            }
            catch (Exception Ex)
            {
                OutputTextBlock.AppendText(Ex.ToString() + "\n");
            }
        }

        public void ParseHtml(List<String> urls)
        {
            // Create new empty file
            CreateFile(FILE_LOCATION);

            foreach (var url in urls)
            {
                WriteData("\n\n============================================================================================================================================");
                WriteData("Page URL: " + url);
                WriteData("============================================================================================================================================\n\n");

                var client = new WebClient();
                var htmlText = client.DownloadString(url);

                var htmlDoc = new HtmlAgilityPack.HtmlDocument
                {
                    OptionFixNestedTags = true,
                    OptionAutoCloseOnEnd = true
                };

                htmlDoc.LoadHtml(htmlText);

                if (htmlDoc.DocumentNode != null)
                {
                    Int32 index = 0;
                    foreach (HtmlNode node in htmlDoc.DocumentNode.SelectNodes("//img"))
                    {
                        var altText = node.GetAttributeValue("alt", null);
                        if (String.IsNullOrEmpty(altText) || altText == " ")
                        {
                            var src = node.GetAttributeValue("src", null);

                            if (!src.Contains("weezlabs.com") && !src.Contains("googleads"))
                            {
                                src = MAIN_NODE + src; 
                            }

                            // Write data to file
                            WriteData("#" + index + ". Image: " + src);
                            WriteData("Alt: " + altText);
                            WriteData("Image tag: " + node.OuterHtml);
                            WriteData("");

                            index++;
                            COUNT_EMPTY_TAGS++;
                        }
                    }
                }
            }
        }

        public void WriteData(String message)
        {
            using (StreamWriter sw = File.AppendText(FILE_LOCATION))
            {
                sw.WriteLine(message);
            }
        }

        public List<String> parseUrls(String fileContent)
        {
            List<String> listUrl = new List<String>();
            StringBuilder output = new StringBuilder();

            using (XmlReader reader = XmlReader.Create(new StringReader(fileContent)))
            {
                XmlWriterSettings ws = new XmlWriterSettings();
                ws.Indent = true;
                using (XmlWriter writer = XmlWriter.Create(output, ws))
                {
                    // Parse the file and display each of the nodes.
                    while (reader.Read())
                    {
                        switch (reader.NodeType)
                        {
                            case XmlNodeType.Element:
                                writer.WriteStartElement(reader.Name);
                                break;
                            case XmlNodeType.Text:
                                writer.WriteString(reader.Value);
                                listUrl.Add(reader.Value);
                                break;
                            case XmlNodeType.XmlDeclaration:
                            case XmlNodeType.ProcessingInstruction:
                                writer.WriteProcessingInstruction(reader.Name, reader.Value);
                                break;
                            case XmlNodeType.Comment:
                                writer.WriteComment(reader.Value);
                                break;
                            case XmlNodeType.EndElement:
                                writer.WriteFullEndElement();
                                break;
                        }
                    }
                }
            }

            return listUrl;
        }
    }
}
