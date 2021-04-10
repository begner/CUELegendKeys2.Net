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

        public bool useMocks = false;

        public bool lastLoadErrors = false;

        public delegate void loadJSONDecodeCallback(string jsonData);

        public delegate void loadJSONLoadedCallback();

        public abstract string GetUri();
        public abstract string GetMockFile();
        public abstract void JsonDecode(string jsonData);

        public string dataDragonHost = "http://ddragon.leagueoflegends.com";
        public string clientHost = "https://127.0.0.1:2999";
        public string locale = "de_DE";
        public string currentVersion { get; set; }

        public void LoadData()
        {
            this.LoadJSONData(this.GetUri(), this.GetMockFile(), this.JsonDecode, this.JsonLoaded);
        }

        public void JsonLoaded()
        {
            DataProviderLoadedEventArgs args = new DataProviderLoadedEventArgs();
            this.OnDataLoaded(args);
        }

        public void OnDataLoaded(DataProviderLoadedEventArgs e)
        {
            EventHandler<DataProviderLoadedEventArgs> handler = OnData;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public event EventHandler<DataProviderLoadedEventArgs> OnData;

        private void LoadJSONData(string uri, string mockFile, loadJSONDecodeCallback decode, loadJSONLoadedCallback onReady)
        {
            this.lastLoadErrors = false;
            if (this.useMocks)
            {
                // MOCK
                decode(File.ReadAllText(mockFile));
                onReady();
                return;
            }

            try
            {
                var webRequest = WebRequest.Create(uri) as HttpWebRequest;

                if (webRequest == null)
                {
                    return;
                }
                webRequest.ContentType = "application/json";
                using (var s = webRequest.GetResponse().GetResponseStream())
                {
                    using (var sr = new StreamReader(s))
                    {
                        decode(sr.ReadToEnd());
                        onReady();
                    }
                }
            }
            catch(Exception exception)
            {
                Logger.Instance.Debug(exception.Message);
                this.lastLoadErrors = true;
                onReady();
                // do nothing here
            }
           
            
        }
    }
}
