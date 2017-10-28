using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Crawler
{
    public class ImgCrawler
    {
        private int imgNameIndex = 0;
        private string dir = "F:\\images\\";
        private string root = "http://www.mzitu.com/";
        private string themePattern = "http://www.mzitu.com/[A-Za-z]{5,}";
        private string individualPattern = @"http://www.mzitu.com/[0-9]{4,}";
        private string selfieUrl = "http://www.mzitu.com/zipai/";
        private readonly object obj = new object();
        public void Start()
        {
           // var task1 = Task.Run(() => StartDownLoad(this.root));
            var task2 = Task.Run(() => StartDownLoadSelfie(this.selfieUrl));
        }

        private void StartDownLoadSelfie(string selfieUrl)
        {
            string commonUrl = "http://www.mzitu.com/zipai/comment-page-{0}/#comments";
            int maxNbr = GetSelfieMaxNbr(selfieUrl);
            for (int i = 0; i < maxNbr; i++)
            {
                string pageUrl = string.Format(commonUrl, i);
                IList<string> list = GetSelfieImgList(pageUrl);
                foreach (string item in list)
                {
                    DownloadImg(imgNameIndex, item, -1);
                    lock(obj)
                    {
                        imgNameIndex++;
                    }
                }
            }

        }

        private IList<string> GetSelfieImgList(string pageUrl)
        {
            //"http://wx2.sinaimg.cn/mw1024/9d52c073gy1fkx6t0g89aj20qu0zsdok.jpg
            string html = GetHtml(pageUrl);
            string pattern = "http://wx(.){20,60}.jpg";
            MatchCollection collection = GetCollection(html, pattern);
            IList<string> list = new List<string>();
            foreach(Match item in collection)
            {
                list.Add(item.Value);
            }
            return list;
        }

        private int GetSelfieMaxNbr(string selfieUrl)
        {
            string html = GetHtml(selfieUrl);
            string pattern = "http://www.mzitu.com/zipai/comment-page-[0-9]{1,3}/#comments";
            MatchCollection collection = GetCollection(html, pattern);
            int maxPage = 0;
            foreach (Match item in collection)
            {
                string nbrPattern = "[0-9]{1,3}";
                Regex reg = new Regex(nbrPattern);
                Match match = reg.Match(item.Value);
                if(match.Success)
                {
                    int page= Convert.ToInt32(match.Value);
                    if(page>maxPage)
                    {
                        maxPage = page;
                    }
                }
            }
            return maxPage;
        }

        private void StartDownLoad(string rootUrl)
        {
            IList<string> themes = GetList(rootUrl, themePattern);
            foreach (string theme in themes)
            {
                int maxPage = GetMaxPage(theme);
                for (int i = 1; i <= maxPage; i++)
                {
                    IList<string> individuals = GetList(theme + "/page/" + i.ToString() + "/", individualPattern);
                    for (int j = 0; j < individuals.Count; j++)
                    {
                        int individualNbr = GetMaxNbr(individuals[j]);
                        int maxIndividualMaxPage = GetIndividualMaxPage(individuals[j], individualNbr);
                        for (int k = 1; k < maxIndividualMaxPage; k++)
                        {
                            string url = "http://www.mzitu.com/" + individualNbr.ToString() + "/" + k.ToString();
                            IList<string> imgList = GetImgList(url);
                            foreach (string img in imgList)
                            {
                                DownloadImg(imgNameIndex, img, individualNbr);
                                lock (obj)
                                {
                                    imgNameIndex++;
                                }
                            }
                        }
                    }
                }
            }
        }

        private MatchCollection GetCollection(string html, string pattern)
        {
            Regex reg = new Regex(pattern);
            MatchCollection collection = reg.Matches(html);
            return collection;
        }

        private IList<string> GetImgList(string url)
        {
            IList<string> list = new List<string>();
            string html = GetHtml(url);
            string pattern = "http://(.){1,35}.jpg";
            MatchCollection collection = GetCollection(html, pattern);
            foreach (Match item in collection)
            {
                list.Add(item.Value);
            }
            return list;
        }

        private int GetIndividualMaxPage(string url, int individualNbr)
        {
            string html = GetHtml(url);
            int maxPage = 0;
            string pattern = "http://www.mzitu.com/" + individualNbr.ToString() + "/[0-9]{1,}";
            MatchCollection collection = GetCollection(html, pattern);
            foreach (Match item in collection)
            {
                int page = GetMaxNbr(item.Value);
                if (page > maxPage)
                {
                    maxPage = page;
                }
            }
            return maxPage;
        }

        private int GetMaxNbr(string url)
        {
            string[] parts = url.Split('/');
            int page;
            int.TryParse(parts[parts.Length - 1], out page);
            return page;
        }

        private IList<string> GetList(string url, string pattern)
        {
            IList<string> list = new List<string>();
            string html = GetHtml(url);
            MatchCollection collection = GetCollection(html, pattern);
            foreach (Match item in collection)
            {
                //avoid duplicate themes
                if (!list.Contains(item.Value))
                {
                    list.Add(item.Value);
                }
            }
            return list;
        }
        private int GetMaxPage(string url)
        {
            string html = GetHtml(url);
            int maxPage = 0;
            string pattern = url + "/page/(\\d){1,3}/";
            MatchCollection collection = GetCollection(html, pattern);
            foreach (Match item in collection)
            {
                string[] parts = item.Value.Split('/');
                int page;
                int.TryParse(parts[parts.Length - 2], out page);
                if (page > maxPage)
                {
                    maxPage = page;
                }
            }
            return maxPage;
        }

        private string GetHtml(string url)
        {
            string html = string.Empty;
            WebClient client = new WebClient();
            client.Encoding = Encoding.UTF8;
            try
            {
                html = client.DownloadString(url);
            }
            catch (Exception ex)
            {
                Console.WriteLine("get html error." + ex.Message);
            }
            return html;
        }

        private void DownloadImg(int index, string url, int refer)
        {
            WebClient client = new WebClient();
            client.Headers.Add(HttpRequestHeader.Accept, "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8");
            client.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip, deflate");
            client.Headers.Add(HttpRequestHeader.AcceptLanguage, "zh-CN,zh;q=0.8");
            client.Headers.Add(HttpRequestHeader.CacheControl, "max-age=0");
            client.Headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0 (Windows NT 6.3; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/61.0.3163.100 Safari/537.36");
            if (refer >= 0)
            {
                client.Headers.Add(HttpRequestHeader.Referer, "http://www.mzitu.com/" + refer);
            }
            client.Headers.Add(HttpRequestHeader.Upgrade, "1");
            client.Encoding = Encoding.UTF8;
            try
            {
                client.DownloadFile(url, dir + index.ToString() + ".jpg");
            }
            catch (Exception ex)
            {
                Console.WriteLine("download img error." + ex.Message);
            }
        }
    }
}
