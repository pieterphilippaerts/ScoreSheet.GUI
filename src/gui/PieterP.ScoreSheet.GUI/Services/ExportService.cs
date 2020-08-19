using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Printing;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Printing;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Xps;
using System.Windows.Xps.Packaging;
using PieterP.ScoreSheet.Localization;
using PieterP.ScoreSheet.Model.Database;
using PieterP.ScoreSheet.ViewModels.Interfaces;
using PieterP.ScoreSheet.ViewModels.Score;
using PieterP.Shared;
using PieterP.Shared.Interfaces;
using PieterP.Shared.Services;

namespace PieterP.ScoreSheet.GUI.Services {
    public class ExportService : IExportService {
        public bool ToPdfFile(IEnumerable<object> documents, string file, bool isLandscape = true) {
            try {
                var doc = ToDocument(documents, isLandscape);

                var memoryStream = new MemoryStream();
                var package = Package.Open(memoryStream, FileMode.Create);
                var xpsd = new XpsDocument(package);
                var xw = XpsDocument.CreateXpsDocumentWriter(xpsd);
                xw.Write(doc);
                xpsd.Close();
                package.Close();

                var pdfXpsDoc = PdfSharp.Xps.XpsModel.XpsDocument.Open(memoryStream);
                PdfSharp.Xps.XpsConverter.Convert(pdfXpsDoc, file, 0);
                return true;
            } catch (Exception e) {
                Logger.Log(e);
            }
            return false;
        }

        public bool ToPrinter(IEnumerable<object> documents, bool isLandscape, int? numCopies) {
            try {
                var pd = new PrintDialog();
                pd.PrintTicket.PageOrientation = isLandscape ? PageOrientation.Landscape : PageOrientation.Portrait;
                pd.PrintTicket.CopyCount = numCopies;
                pd.PrintTicket.PageBorderless = PageBorderless.Borderless;
                pd.PrintTicket.PageMediaSize = new PageMediaSize(PageMediaSizeName.ISOA4);
                pd.PageRangeSelection = PageRangeSelection.AllPages;
                pd.UserPageRangeEnabled = true;
                if (pd.ShowDialog() == true) {
                    if (DatabaseManager.Current.Settings.PrintDirect.Value || !File.Exists(DatabaseManager.Current.Settings.AdobePath.Value)) {
                        var doc = ToDocument(documents, isLandscape);
                        pd.PrintDocument(doc.DocumentPaginator, "ScoreSheet");
                    } else { // print via adobe
                        string tempFile = DatabaseManager.Current.GetTempFilename(".pdf");
                        ToPdfFile(documents, tempFile, isLandscape);
                        var adobeProcess = new Process();
                        adobeProcess.StartInfo.CreateNoWindow = true;
                        adobeProcess.StartInfo.FileName = DatabaseManager.Current.Settings.AdobePath.Value;
                        adobeProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                        adobeProcess.StartInfo.Arguments = string.Format("/N /T \"{0}\" \"{1}\"", tempFile, pd.PrintQueue.Name);
                        adobeProcess.Start();
                    }
                }
                return true;
            } catch (Exception e) {
                Logger.Log(e);
            }
            return false;
        }

        public bool ToXpsFile(IEnumerable<object> documents, string file, bool isLandscape = true) {
            try {
                var doc = ToDocument(documents, isLandscape);
                // Create the xps file and write it
                if (File.Exists(file))
                    File.Delete(file);
                var xpsd = new XpsDocument(file, FileAccess.ReadWrite);
                var xw = XpsDocument.CreateXpsDocumentWriter(xpsd);
                xw.Write(doc);
                xpsd.Close();
                return true;
            } catch (Exception e) {
                Logger.Log(e);
            }
            return false;
        }
        private FixedDocument ToDocument(IEnumerable<object> documents, bool isLandscape) {
            // Initialize the xps document structure
            var fixedDoc = new FixedDocument();
            
            foreach (var document in documents) {
                var pageContent = new PageContent();
                var fixedPage = new FixedPage();
                var view = FindTemplate(document);
                view.DataContext = document;
                FixedPage.SetLeft(view, 0);
                FixedPage.SetTop(view, 0);
                // hard coded for A4
                if (isLandscape) {
                    fixedPage.Width = 11.69 * 96; // first number is width of A4 in inch, second number is DPI (always 96 in WPF)
                    fixedPage.Height = 8.27 * 96;
                } else {
                    fixedPage.Width = 8.27 * 96; // first number is width of A4 in inch, second number is DPI (always 96 in WPF)
                    fixedPage.Height = 11.69 * 96;
                }
                fixedPage.Children.Add(view);
                // Create the document and set the datacontext
                view.UpdateLayout();

                fixedPage.Measure(new Size(fixedPage.Width, fixedPage.Height));
                fixedPage.Arrange(new Rect(0, 0, fixedPage.Width, fixedPage.Height));
                fixedPage.UpdateLayout();

                //Create first page of document
                ((System.Windows.Markup.IAddChild)pageContent).AddChild(fixedPage);
                fixedDoc.Pages.Add(pageContent);
            }
            return fixedDoc;
        }
        private ContentControl FindTemplate(object vm) {
            var t = vm.GetType();
            foreach (var value in App.Current.Resources.Values) {
                var dtemp = value as DataTemplate;
                if (dtemp != null && t.Equals(dtemp.DataType)) {
                    var cc = dtemp.LoadContent() as ContentControl;
                    if (cc != null)
                        return cc;
                }
            }
            throw new ArgumentException(Safe.Format(Errors.ExportService_TemplateError, vm.GetType().Name));
        }
    }
}
