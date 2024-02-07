using ACWSSK.Model;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using XFDevice.FujitsuPrinter;
using XFUtility.DocumentPrint;

namespace ACWSSK.App_Code
{
	public class DocumentPrintHandler : DocumentPrint
	{
		const string TraceCategory = "ACWSSK.Helper.DocumentPrintHandler";

		private FujitsuPrinter _printerFHandler;
		private string _printerName;
		private bool _isLandscape;

		private bool _isError;
		private int _printedLength = 0;
		private ManualResetEvent _printedEvent = new ManualResetEvent(false);

		private bool _isQueued;

		private ManualResetEvent _requestEvent = new ManualResetEvent(false);
		private Queue<PrintRequest> _requestQueue = new Queue<PrintRequest>();

		#region Printing Properties

		private Dictionary<string, string> _TokenList;
		public Dictionary<string, string> TokenList
		{
			get { return _TokenList; }
			set
			{
				if (_TokenList != value)
					_TokenList = value;
			}
		}

		private Dictionary<string, Bitmap> _ImageList;
		public Dictionary<string, Bitmap> ImageList
		{
			get { return _ImageList; }
			set
			{
				if (_ImageList != value)
					_ImageList = value;
			}
		}

		private List<TableFormat> _TableList;
		public List<TableFormat> TableList
		{
			get { return _TableList; }
			set
			{
				if (_TableList != value)
					_TableList = value;
			}
		}

		#endregion

		public DocumentPrintHandler(string printerName)
		{
			_printerFHandler = new FujitsuPrinter();
			
			_printerName = printerName;

		}

		~DocumentPrintHandler()
		{
			
		}
                
		public void InitializeNeoPrinter()
		{
			_isLandscape = false;

			int _nearEmptyBuffer;
			string buffer = "0";// query.GetSetting("ReceiptPrinterNearEmptyBuffer");// "603 2381 3139";;
			if (!int.TryParse(buffer, out _nearEmptyBuffer))
				_nearEmptyBuffer = 0;

			int _nearEmptyUsage;
			string usage = "0";//query.GetSetting("ReceiptPrinterNearEmptyUsage");// "603 2381 3139";
			if (!int.TryParse(usage, out _nearEmptyUsage))
				_nearEmptyUsage = 0;

			//GeneralVar.ReceiptPrinter_NearEmptyBuffer = _nearEmptyBuffer;
			GeneralVar.ReceiptPrinter_NearEmptyUsage = _nearEmptyUsage;

			InitializeDocuments();
			StartProcessingQueue();
		}

		public void InitializeDocuments()
		{
			PrintDocument printDoc;
			TokenList = new Dictionary<string, string>();
			TableList = new List<TableFormat>();
			ImageList = null;
			PaperSize paperSize = new PaperSize("default", GeneralVar.PaperSize_Width, 300);

			if (!Directory.Exists(GeneralVar.ReceiptTemplateDirectory)) throw new Exception("Receipt Template Directory not exist!");
			if (!Directory.Exists(GeneralVar.SaveReceiptDirectory)) throw new Exception("Save Receipt Directory not exist!");

            string templateFile = GeneralVar.ReceiptTemplateDirectory + @"SalesReceipt_ByEWallet.docx";
			var status = SendAsPrintDocument(templateFile, out printDoc, TokenList, ImageList, TableList, _isLandscape, paperSize, paperSize);

            templateFile = GeneralVar.ReceiptTemplateDirectory + @"SalesReceipt_ByCard.docx";
            status = SendAsPrintDocument(templateFile, out printDoc, TokenList, ImageList, TableList, _isLandscape, paperSize, paperSize);

            templateFile = GeneralVar.ReceiptTemplateDirectory + @"FailedReceipt_ByEWallet.docx";
            status = SendAsPrintDocument(templateFile, out printDoc, TokenList, ImageList, TableList, _isLandscape, paperSize, paperSize);

            templateFile = GeneralVar.ReceiptTemplateDirectory + @"FailedReceipt_ByCard.docx";
            status = SendAsPrintDocument(templateFile, out printDoc, TokenList, ImageList, TableList, _isLandscape, paperSize, paperSize);

            templateFile = GeneralVar.ReceiptTemplateDirectory + @"SalesReceipt_ByApp.docx";
            status = SendAsPrintDocument(templateFile, out printDoc, TokenList, ImageList, TableList, _isLandscape, paperSize, paperSize);

            templateFile = GeneralVar.ReceiptTemplateDirectory + @"FailedReceipt_ByApp.docx";
            status = SendAsPrintDocument(templateFile, out printDoc, TokenList, ImageList, TableList, _isLandscape, paperSize, paperSize);
        }

		bool isStartProcessing = false;
		public void StartProcessingQueue()
		{
			if (isStartProcessing)
				return;

			isStartProcessing = true;
			System.Threading.ThreadPool.QueueUserWorkItem(o => ProcessPrinterQueue());
		}

		#region Project Based Functions

        public bool Print_SalesByEWallet(bool isSuccess, DateTime txDate, string refNo, decimal NetAmount, decimal TaxValue, decimal TotalTax, decimal TotalSurCharge, decimal TotalAmount, string ewPaymentType, string ewMerchantCode, string ewTerminalid, string ewRefNo, string ewApprovalCode,
              string ewPaymentId, string ewResponseCode, ref int printedLength)
		{
			try
			{
				int paperWidth = GeneralVar.PaperSize_Width;
				int paperHeight = GeneralVar.PaperSize_Height;

                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceVerbose, "Print_SalesByEWallet: Initializing values", TraceCategory);

				#region TokenList

                TokenList = new Dictionary<string, string>()
                {
                    //{"[@ReceiptTitle]", isSuccess? "Car Wash" : "Car Wash (Failed)" },
                    {"[@CompanyName]", GeneralVar.CurrentComponent.CompanyName },
                    {"[@SSMNo]", string.IsNullOrEmpty(GeneralVar.CurrentComponent.SSMNo)? " " : GeneralVar.CurrentComponent.SSMNo },
                    {"[@Address1]", string.IsNullOrEmpty(GeneralVar.CurrentComponent.Address1)? " " : GeneralVar.CurrentComponent.Address1 },
                    {"[@Address2]", string.IsNullOrEmpty(GeneralVar.CurrentComponent.Address2)? " " : GeneralVar.CurrentComponent.Address2 },
                    {"[@Address3]", string.IsNullOrEmpty(GeneralVar.CurrentComponent.Address3)? " " : GeneralVar.CurrentComponent.Address3 },
                    {"[@Address4]", string.IsNullOrEmpty(GeneralVar.CurrentComponent.Address4)? (string.IsNullOrEmpty(GeneralVar.CurrentComponent.SalesTaxNo)? " " : string.Format("SST Reg. No: {0}", GeneralVar.CurrentComponent.SalesTaxNo)) : GeneralVar.CurrentComponent.Address4 },
                    {"[@GSTNo]",  string.IsNullOrEmpty(GeneralVar.CurrentComponent.Address4)? " " : (string.IsNullOrEmpty(GeneralVar.CurrentComponent.SalesTaxNo)? " " : string.Format("SST Reg. No: {0}", GeneralVar.CurrentComponent.SalesTaxNo)) },

                    //{"[@CompanyGSTRegNo]", GeneralVar.CompanyGSTRegNo },
                    {"[@RefNo]", refNo },
                    {"[@TxDate]", txDate.ToString("dd MMM yyyy hh:mm tt") },
                    {"[@ComponentCode]", GeneralVar.CurrentComponent.ComponentCode },
                    {"[@Amt]", NetAmount.ToString("#.00") },
                    {"[@ST]", TaxValue.ToString("0.00") + "%" },
                    {"[@Tax]", TotalTax.ToString("0.00") },
                    {"[@TA]", TotalAmount.ToString("#.00") },
                    //{"[@TR]", "" },

                    {"[@Email]", string.IsNullOrEmpty(GeneralVar.CurrentComponent.Email)? " " : GeneralVar.CurrentComponent.Email },
                    {"[@Careline]", string.IsNullOrEmpty(GeneralVar.CurrentComponent.Careline)? " " : GeneralVar.CurrentComponent.Careline },

                    {"[@EWPaymentType]", string.IsNullOrEmpty(ewPaymentType) ? "-" : ewPaymentType},
                    {"[@EWMerchantCode]", string.IsNullOrEmpty(ewMerchantCode) ? "-" : ewMerchantCode},
                    {"[@EWTerminalId]", string.IsNullOrEmpty(ewTerminalid) ? "-" : ewTerminalid},
                    {"[@EWRefNo]", string.IsNullOrEmpty(ewRefNo) ? "-" : ewRefNo},
                    {"[@EWApprCode]", string.IsNullOrEmpty(ewApprovalCode) ? "-" : ewApprovalCode},
                    {"[@EWPaymentId]", string.IsNullOrEmpty(ewPaymentId) ? "-" : ewPaymentId},
                    {"[@EWRespCode]", string.IsNullOrEmpty(ewResponseCode) ? "-" : ewResponseCode}
                };

				#endregion

				#region TableList

                //Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceVerbose, "Print_SalesByEWallet: Creating Table List", TraceCategory);
				TableList = new List<TableFormat>();
                
				#endregion

                PaperSize paperSize = new PaperSize("default", paperWidth, paperHeight);
                string templateFile = GeneralVar.ReceiptTemplateDirectory + (isSuccess ? @"SalesReceipt_ByEWallet.docx" : @"FailedReceipt_ByEWallet.docx");

				#region Save Receipt

                //Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceVerbose, "Print_SalesByEWallet: Save Receipt", TraceCategory);

                //SaveAsFile(templateFile, GeneralVar.SaveReceiptDirectory + refNo + (isSuccess?"":"_Failed") + ".docx", WordFileFormat.Docx, TokenList, null, TableList, paperSize);

				#endregion

				#region Print Ticket

				PrintDocument printDoc;
				var status = SendAsPrintDocument(templateFile, out printDoc, TokenList, ImageList, TableList, _isLandscape, paperSize, paperSize);
				if (status != DocumentProcessError.Success)
				{
					GeneralVar.RPTException = new DocumentPrintException(status);
					throw GeneralVar.RPTException;
				}

				EnqueueRequest(new PrintRequest(printDoc, paperHeight));

				#endregion

				_isQueued = true;

				_printedEvent.Reset();
				_printedEvent.WaitOne(3000);

				printedLength = _printedLength;
				_isQueued = false;

				return true;
				//printedLength += paperHeight;
			}
			catch (Exception ex)
			{
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, string.Format("[ERROR] Print_SalesByEWallet: {0}", ex.ToString()), TraceCategory);
				return false;
			}
		}

        public bool Print_SalesByApp(bool isSuccess, DateTime txDate, string refNo, decimal NetAmount, decimal TaxValue, decimal TotalTax, decimal TotalSurCharge, decimal TotalAmount, string errorCode, string id, string userId, string walletId,
              string referenceKey, ref int printedLength)
        {
            try
            {
                int paperWidth = GeneralVar.PaperSize_Width;
                int paperHeight = GeneralVar.PaperSize_Height;

                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceVerbose, "Print_SalesByEWallet: Initializing values", TraceCategory);

                #region TokenList
                if (isSuccess)
                {
                    TokenList = new Dictionary<string, string>()
                {
                    //{"[@ReceiptTitle]", isSuccess? "Car Wash" : "Car Wash (Failed)" },
                    {"[@CompanyName]", GeneralVar.CurrentComponent.CompanyName },
                    {"[@SSMNo]", string.IsNullOrEmpty(GeneralVar.CurrentComponent.SSMNo)? " " : GeneralVar.CurrentComponent.SSMNo },
                    {"[@Address1]", string.IsNullOrEmpty(GeneralVar.CurrentComponent.Address1)? " " : GeneralVar.CurrentComponent.Address1 },
                    {"[@Address2]", string.IsNullOrEmpty(GeneralVar.CurrentComponent.Address2)? " " : GeneralVar.CurrentComponent.Address2 },
                    {"[@Address3]", string.IsNullOrEmpty(GeneralVar.CurrentComponent.Address3)? " " : GeneralVar.CurrentComponent.Address3 },
                    {"[@Address4]", string.IsNullOrEmpty(GeneralVar.CurrentComponent.Address4)? (string.IsNullOrEmpty(GeneralVar.CurrentComponent.SalesTaxNo)? " " : string.Format("SST Reg. No: {0}", GeneralVar.CurrentComponent.SalesTaxNo)) : GeneralVar.CurrentComponent.Address4 },
                    {"[@GSTNo]",  string.IsNullOrEmpty(GeneralVar.CurrentComponent.Address4)? " " : (string.IsNullOrEmpty(GeneralVar.CurrentComponent.SalesTaxNo)? " " : string.Format("SST Reg. No: {0}", GeneralVar.CurrentComponent.SalesTaxNo)) },

                    //{"[@CompanyGSTRegNo]", GeneralVar.CompanyGSTRegNo },
                    {"[@RefNo]", refNo },
                    {"[@TxDate]", txDate.ToString("dd MMM yyyy hh:mm tt") },
                    {"[@ComponentCode]", GeneralVar.CurrentComponent.ComponentCode },
                    {"[@Amt]", NetAmount.ToString("#.00") },
                    {"[@ST]", TaxValue.ToString("0.00") + "%" },
                    {"[@Tax]", TotalTax.ToString("0.00") },
                    {"[@TA]", TotalAmount.ToString("#.00") },
                    //{"[@TR]", "" },

                    {"[@Email]", string.IsNullOrEmpty(GeneralVar.CurrentComponent.Email)? " " : GeneralVar.CurrentComponent.Email },
                    {"[@Careline]", string.IsNullOrEmpty(GeneralVar.CurrentComponent.Careline)? " " : GeneralVar.CurrentComponent.Careline },

                    {"[@Id]", string.IsNullOrEmpty(id) ? "-" : id},
                    {"[@UserId]", string.IsNullOrEmpty(userId) ? "-" : userId},
                    {"[@WalletId]", string.IsNullOrEmpty(walletId) ? "-" : walletId},
                    {"[@ReferenceKey]", string.IsNullOrEmpty(referenceKey) ? "-" : referenceKey}
                };
                }
                else
                {
                    TokenList = new Dictionary<string, string>()
                {
                    {"[@CompanyName]", GeneralVar.CurrentComponent.CompanyName },
                    {"[@SSMNo]", string.IsNullOrEmpty(GeneralVar.CurrentComponent.SSMNo)? " " : GeneralVar.CurrentComponent.SSMNo },
                    {"[@Address1]", string.IsNullOrEmpty(GeneralVar.CurrentComponent.Address1)? " " : GeneralVar.CurrentComponent.Address1 },
                    {"[@Address2]", string.IsNullOrEmpty(GeneralVar.CurrentComponent.Address2)? " " : GeneralVar.CurrentComponent.Address2 },
                    {"[@Address3]", string.IsNullOrEmpty(GeneralVar.CurrentComponent.Address3)? " " : GeneralVar.CurrentComponent.Address3 },
                    {"[@Address4]", string.IsNullOrEmpty(GeneralVar.CurrentComponent.Address4)? (string.IsNullOrEmpty(GeneralVar.CurrentComponent.SalesTaxNo)? " " : string.Format("SST Reg. No: {0}", GeneralVar.CurrentComponent.SalesTaxNo)) : GeneralVar.CurrentComponent.Address4 },
                    {"[@GSTNo]",  string.IsNullOrEmpty(GeneralVar.CurrentComponent.Address4)? " " : (string.IsNullOrEmpty(GeneralVar.CurrentComponent.SalesTaxNo)? " " : string.Format("SST Reg. No: {0}", GeneralVar.CurrentComponent.SalesTaxNo)) },

                    {"[@RefNo]", refNo },
                    {"[@TxDate]", txDate.ToString("dd MMM yyyy hh:mm tt") },
                    {"[@ComponentCode]", GeneralVar.CurrentComponent.ComponentCode },
                    {"[@Amt]", NetAmount.ToString("#.00") },
                    {"[@ST]", TaxValue.ToString("0.00") + "%" },
                    {"[@Tax]", TotalTax.ToString("0.00") },
                    {"[@TA]", TotalAmount.ToString("#.00") },
                    //{"[@TR]", "" },

                    {"[@Email]", string.IsNullOrEmpty(GeneralVar.CurrentComponent.Email)? " " : GeneralVar.CurrentComponent.Email },
                    {"[@Careline]", string.IsNullOrEmpty(GeneralVar.CurrentComponent.Careline)? " " : GeneralVar.CurrentComponent.Careline },

                    {"[@Code]", string.IsNullOrEmpty(errorCode) ? "-" : errorCode}
                };
                }
               

                #endregion

                #region TableList

                //Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceVerbose, "Print_SalesByEWallet: Creating Table List", TraceCategory);
                TableList = new List<TableFormat>();

                #endregion

                PaperSize paperSize = new PaperSize("default", paperWidth, paperHeight);
                string templateFile = GeneralVar.ReceiptTemplateDirectory + (isSuccess ? @"SalesReceipt_ByApp.docx" : @"FailedReceipt_ByApp.docx");

                #region Save Receipt

                //Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceVerbose, "Print_SalesByEWallet: Save Receipt", TraceCategory);

                //SaveAsFile(templateFile, GeneralVar.SaveReceiptDirectory + refNo + (isSuccess?"":"_Failed") + ".docx", WordFileFormat.Docx, TokenList, null, TableList, paperSize);

                #endregion

                #region Print Ticket

                PrintDocument printDoc;
                var status = SendAsPrintDocument(templateFile, out printDoc, TokenList, ImageList, TableList, _isLandscape, paperSize, paperSize);
                if (status != DocumentProcessError.Success)
                {
                    GeneralVar.RPTException = new DocumentPrintException(status);
                    throw GeneralVar.RPTException;
                }

                EnqueueRequest(new PrintRequest(printDoc, paperHeight));

                #endregion

                _isQueued = true;

                _printedEvent.Reset();
                _printedEvent.WaitOne(3000);

                printedLength = _printedLength;
                _isQueued = false;

                return true;
                //printedLength += paperHeight;
            }
            catch (Exception ex)
            {
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, string.Format("[ERROR] Print_SalesByEWallet: {0}", ex.ToString()), TraceCategory);
                return false;
            }
        }

        public bool Print_SalesByCard(bool isSuccess, DateTime txDate, string refNo, decimal NetAmount, decimal TaxValue, decimal TotalTax, decimal TotalSurCharge, decimal TotalAmount,
            string ccCardNo, string ccCardType, string ccTerminalid, string ccRefNo, string ccApprovalCode,
               string ccHashPAN, string ccResponseCode, ref int printedLength)
        {
            try
            {
                int paperWidth = GeneralVar.PaperSize_Width;
                int paperHeight = GeneralVar.PaperSize_Height;

                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceVerbose, "Print_SalesByCard: Initializing values", TraceCategory);

                #region TokenList

                TokenList = new Dictionary<string, string>()
                {
                    //{"[@ReceiptTitle]", isSuccess? "Car Wash" : "Car Wash (Failed)" },
                    {"[@CompanyName]", GeneralVar.CurrentComponent.CompanyName },
                    {"[@SSMNo]", string.IsNullOrEmpty(GeneralVar.CurrentComponent.SSMNo)? " " : GeneralVar.CurrentComponent.SSMNo },
                    {"[@Address1]", string.IsNullOrEmpty(GeneralVar.CurrentComponent.Address1)? " " : GeneralVar.CurrentComponent.Address1 },
                    {"[@Address2]", string.IsNullOrEmpty(GeneralVar.CurrentComponent.Address2)? " " : GeneralVar.CurrentComponent.Address2 },
                    {"[@Address3]", string.IsNullOrEmpty(GeneralVar.CurrentComponent.Address3)? " " : GeneralVar.CurrentComponent.Address3 },
                    {"[@Address4]", string.IsNullOrEmpty(GeneralVar.CurrentComponent.Address4)? (string.IsNullOrEmpty(GeneralVar.CurrentComponent.SalesTaxNo)? " " : string.Format("SST Reg. No: {0}", GeneralVar.CurrentComponent.SalesTaxNo)) : GeneralVar.CurrentComponent.Address4 },
                    {"[@GSTNo]",  string.IsNullOrEmpty(GeneralVar.CurrentComponent.Address4)? " " : (string.IsNullOrEmpty(GeneralVar.CurrentComponent.SalesTaxNo)? " " : string.Format("SST Reg. No: {0}", GeneralVar.CurrentComponent.SalesTaxNo)) },

                    //{"[@CompanyGSTRegNo]", GeneralVar.CompanyGSTRegNo },
                    {"[@RefNo]", refNo },
                    {"[@TxDate]", txDate.ToString("dd MMM yyyy hh:mm tt") },
                    {"[@ComponentCode]", GeneralVar.CurrentComponent.ComponentCode },
                    {"[@Amt]", NetAmount.ToString("#.00") },
                    {"[@ST]", TaxValue.ToString("0.00") + "%" },
                    {"[@Tax]", TotalTax.ToString("0.00") },
                    {"[@TA]", TotalAmount.ToString("#.00") },
                    //{"[@TR]", "" },

                    {"[@Email]", string.IsNullOrEmpty(GeneralVar.CurrentComponent.Email)? " " : GeneralVar.CurrentComponent.Email },
                    {"[@Careline]", string.IsNullOrEmpty(GeneralVar.CurrentComponent.Careline)? " " : GeneralVar.CurrentComponent.Careline },

                    {"[@CCCardNo]", string.IsNullOrEmpty(ccCardNo) ? "-" : ccCardNo},
                    {"[@CCCardType]", string.IsNullOrEmpty(ccCardType) ? "-" : ccCardType},
                    {"[@CCTerminalId]", string.IsNullOrEmpty(ccTerminalid) ? "-" : ccTerminalid},
                    {"[@CCRefNo]", string.IsNullOrEmpty(ccRefNo) ? "-" : ccRefNo},
                    {"[@CCApprCode]", string.IsNullOrEmpty(ccApprovalCode) ? "-" : ccApprovalCode},
                    {"[@CCHashPAN]", string.IsNullOrEmpty(ccHashPAN) ? "-" : ccHashPAN},
                    {"[@CCRespCode]", string.IsNullOrEmpty(ccResponseCode) ? "-" : ccResponseCode},
                };

                #endregion

                #region TableList

                //Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceVerbose, "Print_SalesByCard: Creating Table List", TraceCategory);
                TableList = new List<TableFormat>();

                #endregion

                PaperSize paperSize = new PaperSize("default", paperWidth, paperHeight);
                string templateFile = GeneralVar.ReceiptTemplateDirectory + (isSuccess ? @"SalesReceipt_ByCard.docx" : @"FailedReceipt_ByCard.docx");

                #region Save Receipt

                //Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceVerbose, "Print_SalesByCard: Save Receipt", TraceCategory);

                //SaveAsFile(templateFile, GeneralVar.SaveReceiptDirectory + refNo + (isSuccess ? "" : "_Failed") + ".docx", WordFileFormat.Docx, TokenList, null, TableList, paperSize);

                #endregion

                #region Print Ticket

                PrintDocument printDoc;
                var status = SendAsPrintDocument(templateFile, out printDoc, TokenList, ImageList, TableList, _isLandscape, paperSize, paperSize);
                if (status != DocumentProcessError.Success)
                {
                    GeneralVar.RPTException = new DocumentPrintException(status);
                    throw GeneralVar.RPTException;
                }

                EnqueueRequest(new PrintRequest(printDoc, paperHeight));

                #endregion

                _isQueued = true;

                _printedEvent.Reset();
                _printedEvent.WaitOne(3000);

                printedLength = _printedLength;
                _isQueued = false;

                return true;
                //printedLength += paperHeight;
            }
            catch (Exception ex)
            {
                Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceError, string.Format("[ERROR] Print_SalesByCard: {0}", ex.ToString()), TraceCategory);
                return false;
            }
        }

		#endregion

		public bool GetPrinterStatus(out XFDevice.FujitsuPrinter.FujitsuPrinter status)
		{
			bool result = false;

			result = _printerFHandler.GetStatus();
			//_printerFHandler.LastPrinterStatus[ePrinterStatus.CoverOpen] = false;
			//_printerFHandler.LastPrinterStatus[ePrinterStatus.PaperEnd] = false;
			//_printerFHandler.LastPrinterStatus[ePrinterStatus.PaperNearEnd] = false;

			status = _printerFHandler;

			return result;

		}

		private void ProcessPrinterQueue()
		{
			try
			{

				while (true)
				{
					if (_requestQueue.Count > 0)
					{
						PrintRequest request = _requestQueue.Dequeue();
						if (_isError)
							continue;

						request.Document.PrinterSettings.PrinterName = _printerName;
						request.Document.Print();
						_printedLength += request.PaperHeight;

						System.Threading.Thread.Sleep(100);
					}
					else
					{
						if (!_isQueued)
						{
							System.Threading.Thread.Sleep(50);
							continue;
						}

                        XFDevice.FujitsuPrinter.FujitsuPrinter status;
                        var result = GetPrinterStatus(out status);
                        if (result)
                        {
                            if (status.LastPrinterStatus[ePrinterStatus.PaperEnd])
                            {
                                GeneralVar.IsNearPaperEnd = false;
                                GeneralVar.RPTException = new Exception("RPT03");
                                System.Diagnostics.Trace.WriteLine("[ERROR] PrintDocument: PaperStatus.PaperEnd");

                                int categoryId = GeneralVar.AlarmHandler.GetAlarmCategoryId("RPT");
                                string txType = "E";
                                string description = "Printer status error : PaperEnd.";

                                GeneralVar.AlarmHandler.InsertAlarm(categoryId, txType, description);

                                _printedEvent.Set();

                                _requestEvent.Reset();
                                _requestEvent.WaitOne();
                            }
                            else if (status.LastPrinterStatus[ePrinterStatus.PaperNearEnd]) 
                            {
                                int categoryId = GeneralVar.AlarmHandler.GetAlarmCategoryId("RPT");
                                string txType = "W";
                                string description = "Printer status : PaperNearEnd.";
                                GeneralVar.IsNearPaperEnd = true;
                                GeneralVar.AlarmHandler.InsertAlarm(categoryId, txType, description);

                                GeneralVar.RPTException = null;
                                System.Threading.Thread.Sleep(100);
                            }
                            else
                            {
                                GeneralVar.IsNearPaperEnd = false;
                                GeneralVar.RPTException = null;
                                System.Threading.Thread.Sleep(100);
                            }
                        }
                        else
                            System.Threading.Thread.Sleep(100);
							
					}
				}
			}
			catch (Exception ex) 
			{
				Trace.WriteLineIf(GeneralVar.SwcTraceLevel.TraceInfo, string.Format("ProcessPrinterQueue error: {0}", ex.Message), TraceCategory);
			}
		}

		private void EnqueueRequest(PrintRequest request)
		{
			_requestQueue.Enqueue(request);
			_requestEvent.Set();
		}
	}

	public class DocumentPrintException : Exception
	{
		private DocumentProcessError _error;
		public DocumentProcessError Error
		{
			get { return _error; }
			set { _error = value; }
		}

		public DocumentPrintException(DocumentProcessError error)
		{
			Error = error;
		}
	}

	public class PrintRequest
	{
		public PrintDocument Document;
		public int PaperHeight;

		public PrintRequest(PrintDocument document, int paperHeight)
		{
			Document = document;
			PaperHeight = paperHeight;
		}
	}
}
