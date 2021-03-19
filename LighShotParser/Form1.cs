using CsQuery;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LighShotImgLoader
{
    public partial class Form1 : Form
    {
        string imgFolder = @"C:\img\";
        string urlStart = "https://prnt.sc/";
        char[] symbols = {'1','2','3','4','5','6','7','8','9','0','a','b','c','d','e','f','g','h','i',
            'k','l','m','n','o','p','q','r','s','t','u','v','w','x','y','z'};
        int length = 35;
        int counter = 0;
        bool isDownl = false;
        Random rnd;
        delegate void countChanged();
        delegate void isDownlChanged(string s);

        public Form1()
        {
            InitializeComponent();

            length = symbols.Length;
            rnd = new Random();
            // default path
            string desktop = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            string folderName = "lightshot_imgs/";
            imgFolder = Path.Combine(desktop, folderName);

            if(!Directory.Exists(imgFolder))
                Directory.CreateDirectory(imgFolder);

        }

        // shows one img
        private void btnNextOne_Click(object sender, EventArgs e)
        {
            string curURL = generateNewURL();
            string imgURL = parseOneURL(curURL);
            loadIMG(imgURL);
     
        }

        // loads one img to form
        private void loadIMG(string imgURL)
        {
            string imgName = imgURL.Split('/').Last();
            string imgPath = imgFolder + imgName;
            lblFileName.Text = imgName;
            using (WebClient client = new WebClient())
            {
                if (!File.Exists(imgPath)) ;
                    client.DownloadFile(new Uri(imgURL), imgPath );
            }
            pictureBox1.Image = Image.FromFile(imgPath);
        }

        // getting img URL from html page
        private string parseOneURL(string url)
        {
            string pictureURL;
            CQ htmlPage = CQ.CreateFromUrl(url);

            pictureURL =htmlPage["body"].Find("img").First().Elements.First().GetAttribute("src");
           
            if (pictureURL.Length > 0)
            {
                if(pictureURL.StartsWith("//"))
                {
                    pictureURL = "https:" + pictureURL;
                }
                return pictureURL;
            }
            else return "https://im0-tub-ru.yandex.net/i?id=c8606f4d310b3b4ee6bfa8f3cac52c1a&n=13";

        }

        // generates new URLs
        private string generateNewURL()
        {
            StringBuilder urlEnd = new StringBuilder();
            for(int i=0;i<6;i++)
            {
                char rndChar = symbols[rnd.Next(0, length)];
                urlEnd.Append(rndChar);
            }
            string newURL = urlStart + urlEnd;
            return newURL;
        }

        // changes folder
        private void btnFolder_Click(object sender, EventArgs e)
        {
            DialogResult res = folderBrowserDialog1.ShowDialog();
            if(res == DialogResult.OK)
                imgFolder =  folderBrowserDialog1.SelectedPath+ @"\";
        }

        // clears the panel
        private void btnClear_Click(object sender, EventArgs e)
        {
            pictureBox1.Image = null;
        }

        // async downloading
        private async void btnStartDownl_Click(object sender, EventArgs e)
        {
            lblCount.Text = "0";
            counter = 0;
            isDownl = true;
            await Task.Run( () => downloadImages());
        }

        // downloading multiple images
        private void downloadImages()
        {
            Invoke(new isDownlChanged(isLoadingChanged), "YES");
            using (WebClient client = new WebClient())
            {
                while (isDownl)
                {
                    string curURL = generateNewURL();
                    string imgURL = parseOneURL(curURL);
                    string imgName = imgURL.Split('/').Last();
                    string imgPath = imgFolder + imgName;
                    {
                        if (!File.Exists(imgPath))
                        {
                            client.DownloadFile(new Uri(imgURL), imgPath);
                            counter++;
                            this.Invoke( new countChanged(counterChanged));
                        }
                    }
                }
            }
            Invoke(new isDownlChanged(isLoadingChanged),"NO");
            return;
        }

        // changing lblTextColor in the main thread
        private void isLoadingChanged(string s)
        {
            if(s == "YES")
            {
                lblLoading.ForeColor = Color.DarkRed;
            }
            else
            {
                lblLoading.ForeColor = Color.Black;
            }
            lblLoading.Text = s;
        }

        // changing lblCountText in the main thread
        private void counterChanged()
        {
            lblCount.Text = counter.ToString();
        }

        // stops downloading
        private void btnStopDownl_Click(object sender, EventArgs e)
        {
            isDownl = false;
        }
    }
}
