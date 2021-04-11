using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CueLegendKey2
{
    public class DataProviderLoadedEventArgs : EventArgs
    {
    }

    public abstract class DataProvider
    {
        public DataProvider()
        {
            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
        }

        public bool useMocks { get; set; } = false;
        protected bool lastLoadErrors = false;
        public bool loadHasErrors()
        {
            return lastLoadErrors;
        }

        protected string requestContentType = "text/plain";
        protected bool autoReportDoneAfterDecode = true;


        public abstract string GetUri();
        public abstract string GetMockFile();

        protected string dataDragonHost = "http://ddragon.leagueoflegends.com";
        protected string clientHost = "https://127.0.0.1:2999";
        public string locale { get; set; } = "de_DE";
        public string currentVersion { get; set; }

        protected virtual FileCache GetCache()
        {
            return null;
        }

        public virtual void LoadData()
        {
            this.LoadWebData(this.GetUri(), this.GetMockFile());
        }

        protected void DataLoaded()
        {
            DataProviderLoadedEventArgs args = new DataProviderLoadedEventArgs();
            this.OnDataLoaded(args);
        }

        protected void OnDataLoaded(DataProviderLoadedEventArgs e)
        {
            EventHandler<DataProviderLoadedEventArgs> handler = OnData;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public event EventHandler<DataProviderLoadedEventArgs> OnData;

        protected string StreamToString(Stream stream)
        {
            StreamReader reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        protected virtual void DecodeData(Stream stream)
        {
        }
        
        protected void LoadWebData(string uri, string mockFile)
        {
            this.lastLoadErrors = false;
            
            // MOCK
            if (this.useMocks)
            {
                // MOCK
                this.DecodeData(new MemoryStream(File.ReadAllBytes(mockFile)));
                if (autoReportDoneAfterDecode)
                {
                    this.DataLoaded();
                }
                return;
            }

            // CACHE
            
            FileCache fileCache = this.GetCache();
            if (fileCache != null)
            {
                
                if (fileCache.Exists())
                {
                    this.DecodeData(fileCache.Read());
                    if (autoReportDoneAfterDecode)
                    {
                        this.DataLoaded();
                    }
                    return;
                }
                
            }    
            

            // LOAD
            try
            {
                var webRequest = WebRequest.Create(uri) as HttpWebRequest;

                if (webRequest == null)
                {
                    return;
                }
                webRequest.ContentType = this.requestContentType;
                using (var responseStream = webRequest.GetResponse().GetResponseStream())
                {

                    MemoryStream workStream = new MemoryStream();
                    responseStream.CopyTo(workStream);


                    workStream.Seek(0, SeekOrigin.Begin);

                    this.DecodeData(workStream);
                    if (autoReportDoneAfterDecode)
                    {
                        this.DataLoaded();
                    }

                    if (fileCache != null)
                    {
                        workStream.Seek(0, SeekOrigin.Begin);
                        fileCache.Write(workStream);
                    }
                  
                }
            }
            catch(Exception exception)
            {
                Logger.Instance.Debug(exception.Message);
                this.lastLoadErrors = true;
                this.DataLoaded();
            }
        }

    }
}
