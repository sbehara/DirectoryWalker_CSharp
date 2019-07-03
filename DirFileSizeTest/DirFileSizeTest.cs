using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DirFileSize;
namespace DirFileSizeTest
{
    [TestClass]
    public class DirFileSizeTest
    {
        [TestMethod]
        public void TestReadSubDirFilesAndGenerateFileInfoCSVs()
        {
            long retVal = DirFileSize.Program.ReadSubDirFilesAndGenerateFileInfoCSVs();
            //DirFileSize.Program.getDelimitedString(@"\\files\dbstore$\Merchants\100XXX\100000\onlineapp.pdf", "abcde~508733");
            Console.WriteLine(retVal.ToString());
        }
        [TestMethod]
        public void ReadCSVsAndLoadToTable()
        {
            long retVal = DirFileSize.Program.ReadCSVsAndLoadToTable();
            //DirFileSize.Program.getDelimitedString(@"\\files\dbstore$\Merchants\100XXX\100000\onlineapp.pdf", "abcde~508733");
            Console.WriteLine(retVal.ToString());
        }

        [TestMethod]
        public void htmlEncode()
        {
            //string html = "Jxon:<TransactionResponse xmlns=\\http://schemas.datacontract.org/2004/07/CardEquipment\\ xmlns:i=\\http://www.w3.org/2001/XMLSchema-instance\\><AuthCode>173913</AuthCode><AvailableTerminals i:nil=\\true\\/><CardBalance>0</CardBalance><CardPresenceType i:nil=\\true\\/><CardType>Visa</CardType><CashBackAmount>0</CashBackAmount><ClerkID i:nil=\\true\\/><CustomerAccountNO>0128</CustomerAccountNO><EntryMode>MagneticStrip</EntryMode><ErrorMessage i:nil=\\true\\/><GiftCardReferenceNumber i:nil=\\true\\/><HostResponseISOCode>0</HostResponseISOCode><HostValidationCode>3RTQ</HostValidationCode><InvoiceNumber>56</InvoiceNumber><IsApproved>true</IsApproved><IsCVVMatched>false</IsCVVMatched><IsConnectionError>false</IsConnectionError><IsError>false</IsError><IsZipMatched>false</IsZipMatched><MPPGAge>0</MPPGAge><MPPGCHDName i:nil=\\true\\/><MPPGCardId i:nil=\\true\\/><MPPGCount>0</MPPGCount><MPPGExpDateYYMM i:nil=\\true\\/><MPPGPANLast4 i:nil=\\true\\/><MPPGRegisteredBy i:nil=\\true\\/><MPPGScore i:nil=\\true\\/><MPPGStatusCode i:nil=\\true\\/><MPPGStatusMessage i:nil=\\true\\/><OrderNumber i:nil=\\true\\/><OriginalTransactionType i:nil=\\true\\/><ReceiptEncoded i:nil=\\true\\/><ReceiptText i:nil=\\true\\/><ReferenceNumber>56</ReferenceNumber><RequestAmount>0.01</RequestAmount><ResponseAmount>0.01</ResponseAmount><ResponseCode>000</ResponseCode><ResponseMessage>AP</ResponseMessage><SignaturePresent>false</SignaturePresent><SurchargeAmount>0</SurchargeAmount><TaxAmount>0</TaxAmount><TerminalID>8801412098804</TerminalID><TerminalName i:nil=\\true\\/><TerminalType i:nil=\\true\\/><TicketNumber>0</TicketNumber><TipAmount>0</TipAmount><TotalAmount>0.01</TotalAmount><TransactionID>468045815206725</TransactionID><TransactionSequenceNO i:nil=\\true\\/><TransactionStatus>Approved</TransactionStatus><TransactionType i:nil=\\true\\/><VerboseMessage>TransactionStatus: Approved&#xD;\\nResponseAmount: 0.01&#xD;\\nAuthCode: 173913&#xD;\\nTransactionID: 468045815206725&#xD;\\n&#xD;\\n</VerboseMessage><VoucherNumber i:nil=\\true\\/></TransactionResponse>";
            //string encodedHtml = System.Net.WebUtility.HtmlEncode(html);
            string newhtml = "\"Jxon\":\"<TransactionResponse xmlns=\\\"http://schemas.datacontract.org/2004/07/CardEquipment\\\" xmlns:i=\\\"http://www.w3.org/2001/XMLSchema-instance\\\"><AuthCode>173913</AuthCode><AvailableTerminals i:nil=\\\"true\\\"/><CardBalance>0</CardBalance><CardPresenceType i:nil=\\\"true\\\"/><CardType>Visa</CardType><CashBackAmount>0</CashBackAmount><ClerkID i:nil=\\\"true\\\"/><CustomerAccountNO>0128</CustomerAccountNO><EntryMode>MagneticStrip</EntryMode><ErrorMessage i:nil=\\\"true\\\"/><GiftCardReferenceNumber i:nil=\\\"true\\\"/><HostResponseISOCode>0</HostResponseISOCode><HostValidationCode>3RTQ</HostValidationCode><InvoiceNumber>56</InvoiceNumber><IsApproved>true</IsApproved><IsCVVMatched>false</IsCVVMatched><IsConnectionError>false</IsConnectionError><IsError>false</IsError><IsZipMatched>false</IsZipMatched><MPPGAge>0</MPPGAge><MPPGCHDName i:nil=\\\"true\\\"/><MPPGCardId i:nil=\\\"true\\\"/><MPPGCount>0</MPPGCount><MPPGExpDateYYMM i:nil=\\\"true\\\"/><MPPGPANLast4 i:nil=\\\"true\\\"/><MPPGRegisteredBy i:nil=\\\"true\\\"/><MPPGScore i:nil=\\\"true\\\"/><MPPGStatusCode i:nil=\\\"true\\\"/><MPPGStatusMessage i:nil=\\\"true\\\"/><OrderNumber i:nil=\\\"true\\\"/><OriginalTransactionType i:nil=\\\"true\\\"/><ReceiptEncoded i:nil=\\\"true\\\"/><ReceiptText i:nil=\\\"true\\\"/><ReferenceNumber>56</ReferenceNumber><RequestAmount>0.01</RequestAmount><ResponseAmount>0.01</ResponseAmount><ResponseCode>000</ResponseCode><ResponseMessage>AP</ResponseMessage><SignaturePresent>false</SignaturePresent><SurchargeAmount>0</SurchargeAmount><TaxAmount>0</TaxAmount><TerminalID>8801412098804</TerminalID><TerminalName i:nil=\\\"true\\\"/><TerminalType i:nil=\\\"true\\\"/><TicketNumber>0</TicketNumber><TipAmount>0</TipAmount><TotalAmount>0.01</TotalAmount><TransactionID>468045815206725</TransactionID><TransactionSequenceNO i:nil=\\\"true\\\"/><TransactionStatus>Approved</TransactionStatus><TransactionType i:nil=\\\"true\\\"/><VerboseMessage>TransactionStatus: Approved&#xD;\\nResponseAmount: 0.01&#xD;\\nAuthCode: 173913&#xD;\\nTransactionID: 468045815206725&#xD;\\n&#xD;\\n</VerboseMessage><VoucherNumber i:nil=\\\"true\\\"/></TransactionResponse>\"";
            
            string newencodedHtml = System.Net.WebUtility.HtmlEncode(newhtml);

            System.Net.HttpWebRequest req = (System.Net.HttpWebRequest)System.Net.WebRequest.Create("http://www.example.com/");
            var postData = System.Text.Encoding.ASCII.GetBytes(newencodedHtml);
            req.Method = "POST";
            req.ContentLength = postData.Length;
            req.ContentType = "application/x-www-form-urlencoded";

            using (var stream = req.GetRequestStream())
            {
                stream.Write(postData, 0, postData.Length);

            }


            var response = (System.Net.HttpWebResponse)req.GetResponse();
            var responseString = new System.IO.StreamReader(response.GetResponseStream()).ReadToEnd();
        }

        [TestMethod]
        public void GetNetworkIP()
        {
            System.Net.IPHostEntry host;
            string localIP = null;
            try
            {
                host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
                foreach (System.Net.IPAddress ip in host.AddressList)
                {
                    if (ip.AddressFamily.ToString().ToLower() == "internetwork")
                    {
                        localIP = String.Format("https://{0}:5020/TMS/", ip.ToString());
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine("Error");
                //Logging.LogMessage(LogLevels.Error, "Error: {0}{1}", ex.ToString(), Environment.NewLine);
            }
            System.Console.WriteLine(localIP);
            return;
        }
    }
}
