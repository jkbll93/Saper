using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.Graphics;
using Windows.UI.Notifications;
using Windows.Data.Xml.Dom;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=391641

namespace S2
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        Button[,] btn;
        int[,] btn_prop;
        int[,] saved_btn_prop;
        int[,] canClick;

        bool bombTime = false;
        bool gameOver;
        bool firstPlay = true;

        int flagValue = 10;

        int flags;
        int mines;
        int boardX, boardY;

        int[] dx8 = { 1, 0, -1, 0, 1, -1, -1, 1 };
        int[] dy8 = { 0, 1, 0, -1, 1, -1, 1, -1 };
        
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {

        }

        private void bombOrNothingBtn_Click(object sender, RoutedEventArgs e)
        {
            if (bombTime == false)
            {
                setFlagImage();
                bombTime = true;
            }
            else
            {
                setBombImage();
                bombTime = false;
            }
        }

        private void setFlagImage()
        {
            ImageBrush myBrush = new ImageBrush();
            Image image = new Image();
            image.Source = new BitmapImage(new Uri(@"ms-appx:///Images/flaga.png"));
            myBrush.ImageSource = image.Source;
            bombOrNothingBtn.Background = myBrush;
        }

        private void setBombImage()
        {
            ImageBrush myBrush = new ImageBrush();
            Image image = new Image();
            image.Source = new BitmapImage(new Uri(@"ms-appx:///Images/bomb.png"));
            myBrush.ImageSource = image.Source;
            bombOrNothingBtn.Background = myBrush;
        }

        private void smileBtn_Click(object sender, RoutedEventArgs e)
        {
            if (firstPlay)
            {
                StartGame();
                firstPlay = false;
            }
            else if (!firstPlay)
            {
                ResetGame();
                StartGame();
            }
            
        }

        void ResetGame()
        {
            for (int i = 0; i < boardX; i++)
                for (int j = 0; j < boardY; j++)
                {
                    canClick[i, j] = 1;
                    setClickImage(i, j);
                    btn_prop[i, j] = 0;
                    saved_btn_prop[i, j] = 0;
                }
        }

        void StartGame()
        {

            boardX = boardY = 16;
            mines = 20;
            flagLeft.Text = "Flag: " +  mines.ToString();
            gameOver = false;

            flags = mines;

            //boardX = boardY = 9;
            //mines = 10;
            setBombImage();
            if ( firstPlay)
            {
                GenerateButtons(boardX, boardY);
            }
            
            GenerateMap(boardX, boardY, mines);
            SetMapNumbers(boardX, boardY);
        }

        void GenerateButtons(int x, int y)
        {
            Grid board = new Grid();
            board.Width = defaultLayout.Width;
            board.Height = defaultLayout.Height;
            Thickness thick = new Thickness(5, 5, 5, 5);
            board.Margin = thick;
            board.HorizontalAlignment = HorizontalAlignment.Center;
            board.VerticalAlignment = VerticalAlignment.Center;
            //board.Background = new SolidColorBrush(Colors.LightSteelBlue);

            GridLength gL;

            gL = new GridLength(25);
            btn = new Button[16, 16];
            btn_prop = new int[16, 16];
            saved_btn_prop = new int[16, 16];
            canClick = new int[16, 16];
           
            ColumnDefinition[] gridCol = new ColumnDefinition[y];
            RowDefinition[] gridRow = new RowDefinition[x];

            for ( int j = 0; j < y; j++)
            {
                gridCol[j] = new ColumnDefinition();
                gridCol[j].Width = gL;
                board.ColumnDefinitions.Add(gridCol[j]);
            }

            for (int i = 0; i < x; i++)
            {
                gridRow[i] = new RowDefinition();
                gridRow[i].Height = gL;
                board.RowDefinitions.Add(gridRow[i]);
            }

            Thickness t = new Thickness(2, 2, 2, 2);
            int index = 0;
            for (int i = 0; i < x; i++)
                for (int j = 0; j < y; j++)
                {
                    btn[i, j] = new Button();

                    btn[i, j].MinHeight = 60;
                    btn[i, j].Height = 60;
                    btn[i, j].MaxHeight = 60;
                    
                    btn[i, j].MinWidth = 30;
                    btn[i, j].Width = 30;
                    btn[i, j].MaxWidth = 30;

                    btn[i, j].TabIndex = i + j;

                    btn[i, j].Name = i + "x" + j + "y" + index;
                    index++;

                    btn[i, j].Margin = t;

                    setClickImage(i, j);

                    btn[i, j].Click += new RoutedEventHandler(OneClick);

                    if (firstPlay)
                    { 
                    Grid.SetRow(btn[i, j], i);
                    Grid.SetColumn(btn[i, j], j);
                    board.Children.Add(btn[i, j]);
                    }
                }

            StackPanel sp = new StackPanel();
            sp.Children.Add(board);
            defaultLayout.Children.Add(sp);
            Grid.SetRow(sp, 2);
            
        }

        void setClickImage(int x, int y)
        {
            ImageBrush myBrush = new ImageBrush();
            Image image = new Image();
            image.Source = new BitmapImage(new Uri(@"ms-appx:///Images/click.png"));
            myBrush.ImageSource = image.Source;
            btn[x,y].Background = myBrush;
        }

        private void OneClick(object sender, RoutedEventArgs e)
        {
            string name = ((Button)sender).Name;
            int ix = name.IndexOf("x");
            int x = int.Parse(name.Substring(0, ix));
            name = name.Substring(ix + 1);
            int iy = name.IndexOf("y");
            int y = int.Parse(name.Substring(0, iy));
            int index = int.Parse(name.Substring(iy + 1));

            if (bombTime == false)
            {
                if (btn_prop[x, y] != flagValue)
                {
                    if (btn_prop[x, y] != -1 && !gameOver)
                    {
                        Check_ClickWin();
                    }

                    canClick[x, y] = 0;

                    if ( btn_prop[x,y] == 0)
                    {
                        EmptySpace(x, y);
                    }
                    
                    setButtonImage(x, y);                    
                }
            }
            else
            {
                ImageBrush b = new ImageBrush();
                Image i = new Image();

                if (btn_prop[x, y] != flagValue && flags > 0 && canClick[x,y] == 1)
                {
                    i.Source = new BitmapImage(new Uri(@"ms-appx:///Images/flaga.png"));
                    b.ImageSource = i.Source;
                    btn[x, y].Background = b;
                    btn_prop[x, y] = flagValue;
                    flags--;
                    Check_FlagWin();
                }
                else
                if (btn_prop[x, y] == flagValue)
                {
                    btn_prop[x, y] = saved_btn_prop[x, y];
                    i.Source = new BitmapImage(new Uri(@"ms-appx:///Images/click.png"));
                    b.ImageSource = i.Source;
                    btn[x, y].Background = b;
                    flags++;
                }

                flagLeft.Text = "Flag: " + flags;

            }
        }

        void setButtonImage(int x, int y)
        {
            if (gameOver && btn_prop[x, y] == flagValue)
                btn_prop[x, y] = saved_btn_prop[x, y];

            if (gameOver) { }
                //timer.Stop();

                switch (btn_prop[x, y])
                {
                    case 0:
                        ImageBrush b0 = new ImageBrush();
                        Image i0 = new Image();
                        i0.Source = new BitmapImage(new Uri(@"ms-appx:///Images/puste.png"));
                        b0.ImageSource = i0.Source;
                        btn[x, y].Background = b0;
                        //EmptySpace(x, y);
                        break;
                    case 1:
                        ImageBrush b1 = new ImageBrush();
                        Image i1 = new Image();
                        i1.Source = new BitmapImage(new Uri(@"ms-appx:///Images/jeden.png"));
                        b1.ImageSource = i1.Source;
                        btn[x, y].Background = b1;
                        break;
                    case 2:
                        ImageBrush b2 = new ImageBrush();
                        Image i2 = new Image();
                        i2.Source = new BitmapImage(new Uri(@"ms-appx:///Images/dwa.png"));
                        b2.ImageSource = i2.Source;
                        btn[x, y].Background = b2;
                        //btn[x, y].Content = "2";
                        break;
                    case 3:
                        ImageBrush b3 = new ImageBrush();
                        Image i3 = new Image();
                        i3.Source = new BitmapImage(new Uri(@"ms-appx:///Images/trzy.png"));
                        b3.ImageSource = i3.Source;
                        btn[x, y].Background = b3;
                        //btn[x, y].Content = "3";
                        break;
                    case 4:
                        ImageBrush b4 = new ImageBrush();
                        Image i4 = new Image();
                        i4.Source = new BitmapImage(new Uri(@"ms-appx:///Images/cztery.png"));
                        b4.ImageSource = i4.Source;
                        btn[x, y].Background = b4;
                        break;
                    case 5:
                        ImageBrush b5 = new ImageBrush();
                        Image i5 = new Image();
                         b5.ImageSource = i5.Source;
                        btn[x, y].Background = b5;
                        break;
                    case 6:
                        ImageBrush b6 = new ImageBrush();
                        Image i6 = new Image();
                        i6.Source = new BitmapImage(new Uri(@"ms-appx:///Images/szesc.png"));
                        b6.ImageSource = i6.Source;
                        btn[x, y].Background = b6;
                        break;
                    case 7:
                        ImageBrush b7 = new ImageBrush();
                        Image i7 = new Image();
                        i7.Source = new BitmapImage(new Uri(@"ms-appx:///Images/siedem.png"));
                        b7.ImageSource = i7.Source;
                        btn[x, y].Background = b7;
                        break;
                    case 8:
                        ImageBrush b8 = new ImageBrush();
                        Image i8 = new Image();
                        i8.Source = new BitmapImage(new Uri(@"ms-appx:///Images/osiem.png"));
                        b8.ImageSource = i8.Source;
                        btn[x, y].Background = b8;
                        break;
                    case -1:
                        ImageBrush b10 = new ImageBrush();
                        Image i10 = new Image();
                        i10.Source = new BitmapImage(new Uri(@"ms-appx:///Images/bomb.png"));
                        b10.ImageSource = i10.Source;
                        btn[x, y].Background = b10;
                        if (!gameOver)
                        {
                            GameOver();
                        }
                        break;
                }
        }

        void EmptySpace(int x, int y)
        {
            List<Point> pL = new List<Point>();
            if (btn_prop[x, y] == 0)
            {
                pL.Clear();
                for (int i = 0; i < 8; i++)
                {
                    int cx = x + dx8[i];
                    int cy = y + dy8[i];
                    if (isPointOnMap(cx, cy) == 1)
                        if (canClick[cx,cy] == 1 && btn_prop[cx, cy] != -1 && !gameOver)
                        {
                            canClick[cx, cy] = 0;
                            if (btn_prop[cx, cy] == 0)
                            {
                                Point p = new Point(cx, cy);
                                pL.Add(p);
                            }
                            setButtonImage(cx, cy);
                        }
                }

                foreach(Point p in pL)
                {
                    EmptySpace((int)p.X, (int)p.Y);
                }
            }
        }

        int isPointOnMap(int x, int y)
        {
            if (x < 0 || x >= boardX|| y < 0 || y >= boardY)
                return 0;
            return 1;
        }

        void GenerateMap(int x, int y, int mines)
        {
            Random rand = new Random();
            List<int> coordx = new List<int>();
            List<int> coordy = new List<int>();

            while (mines > 0)
            {
                coordx.Clear();
                coordy.Clear();

                for (int i = 0; i < x; i++)
                    for (int j = 0; j < y; j++)
                        if (btn_prop[i, j] != -1)
                        {
                            coordx.Add(i);
                            coordy.Add(j);
                            btn_prop[i, j] = 0;
                            canClick[i, j] = 1;
                        }

                int randNum = rand.Next(0, coordx.Count);
                btn_prop[coordx[randNum], coordy[randNum]] = -1;
                saved_btn_prop[coordx[randNum], coordy[randNum]] = -1;
                mines--;
            }
        }

        void SetMapNumbers(int x, int y)
        {           
            int cx, cy;
            //pierwszy wiersz
            for (int j = 0; j < boardY; j++)
            {
                // 0,0
                if (j == 0)
                {
                    if (btn_prop[0, j] != -1)
                    {
                        int[] dxC = { 0, 1, 1 };
                        int[] dyC = { 1, 0, 1 };

                        for (int i = 0; i < 3; i++)
                        {
                            cx = 0 + dxC[i];
                            cy = j + dyC[i];
                            if (btn_prop[cx, cy] == -1)
                            {
                                btn_prop[0, j]++;
                                saved_btn_prop[0, j]++;
                            }
                        }
                    }
                } // 0,boardY-1
                else if (j == boardY - 1)
                {
                    if (btn_prop[0, j] != -1)
                    {
                        int[] dxC = {  0, 1, 1 };
                        int[] dyC = { -1, 0,-1 };

                        for (int i = 0; i < 3; i++)
                        {
                            cx = 0 + dxC[i];
                            cy = j + dyC[i];
                            if (btn_prop[cx, cy] == -1)
                            {
                                btn_prop[0, j]++;
                                saved_btn_prop[0, j]++;
                            }
                        }
                    }
                }
                else
                {
                    int[] dx = { 0, 0, 1,  1, 1 };
                    int[] dy = { -1, 1, 1,-1, 0 };
                    if (btn_prop[0, j] != -1)
                        for (int i = 0; i < 5; i++)
                        {
                            cx = 0 + dx[i];
                            cy = j + dy[i];
                            if (btn_prop[cx, cy] == -1)
                            {
                                btn_prop[0, j]++;
                                saved_btn_prop[0, j]++;
                            }
                        }
                }
            }

            //srodek
            for (int i = 1; i < x - 1; i++)
                for (int j = 1; j < y - 1; j++)
                    if (btn_prop[i, j] != -1)
                    {
                        for (int k = 0; k < 8; k++)
                        {
                            cx = i + dx8[k];
                            cy = j + dy8[k];

                            if (btn_prop[cx, cy] == -1)
                            {
                                btn_prop[i, j]++;
                                saved_btn_prop[i,j]++;
                            }
                        }
                    }

            //ostatni wiersz
            for (int j = 0; j < boardY - 1; j++)
            {
                // boardX -1 ,0
                if (j == 0)
                {
                    if (btn_prop[boardX -1 , j] != -1)
                    {
                        int[] dxC = {-1, -1, 0 }; 
                        int[] dyC = { 0,  1, 1 };

                        for (int i = 0; i < 3; i++)
                        {
                            cx = boardX -1 + dxC[i];
                            cy = j + dyC[i];
                            if (btn_prop[cx, cy] == -1)
                            {
                                btn_prop[boardX - 1, j]++;
                                saved_btn_prop[boardX - 1, j]++;
                            }
                        }
                    }
                } // boardX -1 ,boardY-1
                else if (j == boardY - 1)
                {
                    if (btn_prop[boardX - 1, j] != -1)
                    {
                        int[] dxC = { -1, -1, 0 };
                        int[] dyC = {  0, -1,-1 };

                        for (int i = 0; i < 3; i++)
                        {
                            cx = boardX - 1 + dxC[i];
                            cy = j + dyC[i];
                            if (btn_prop[cx, cy] == -1)
                            {
                                btn_prop[boardX - 1, j]++;
                                saved_btn_prop[boardX - 1, j]++;
                            }
                        }
                    }
                }
                else
                {
                    int[] dx = {  0, -1, -1, -1, 0 };
                    int[] dy = { -1, -1,  0,  1, 1 };
                    if (btn_prop[boardX - 1, j] != -1)
                        for (int i = 0; i < 5; i++)
                        {
                            cx = boardX - 1 + dx[i];
                            cy = j + dy[i];
                            if (btn_prop[cx, cy] == -1)
                            {
                                btn_prop[boardX - 1, j]++;
                                saved_btn_prop[boardX - 1, j]++;
                            }
                        }
                }
            }

            //pierwsza kolumna
            for (int j = 1; j < boardX - 1; j++)
            {
                int[] dx = { -1, -1, 0, 1, 1 };
                int[] dy = {  0,  1, 1, 1, 0 };
                if (btn_prop[j, 0] != -1)
                    for (int i = 0; i < 5; i++)
                    {
                        cx = j  + dx[i];
                        cy = 0 + dy[i];
                        if (btn_prop[cx, cy] == -1)
                        {
                            btn_prop[j, 0]++;
                            saved_btn_prop[j, 0]++;
                        }
                    }
            }

            //ostatnia kolumna
            for (int j = 1; j < boardX - 1; j++)
            {
                int[] dx = { -1, -1,  0,  1, 1 };
                int[] dy = {  0, -1, -1, -1, 0 };
                if (btn_prop[j, boardY-1] != -1)
                    for (int i = 0; i < 5; i++)
                    {
                        cx = j + dx[i];
                        cy = boardY - 1 + dy[i];
                        if (btn_prop[cx, cy] == -1)
                        {
                            btn_prop[j, boardY -1 ]++;
                            saved_btn_prop[j, boardY -1 ]++;
                        }
                    }
            }
        }

        void Discover_Map()
        {
            for (int i = 0; i < boardX; i++)
                for (int j = 0; j < boardY; j++)
                    if (canClick[i, j] == 1)
                    {
                        canClick[i, j] = 0;
                        setButtonImage(i, j);
                    }
        }

        void GameOver()
        {
            gameOver = true;
            ShowToastNotification("Przegrana");
            Discover_Map();
        }

        void Check_FlagWin()
        {
            bool win = true;

            for (int i = 0; i < boardX; i++)
                for (int j = 0; j < boardY; j++)
                    if (btn_prop[i, j] == -1)
                        win = false;

            if (win)
            {
                WinGame();
            }
        }

        void Check_ClickWin()
        {
            bool win = true;
            for (int i = 0; i < boardX; i++)
                for (int j = 0; j < boardY; j++)
                    if (canClick[i, j] == 1 && saved_btn_prop[i, j] != -1)
                    {
                        int k = i, l = j;
                        win = false;
                    }

            if (win)
            {
                WinGame();
            }
        }

        void WinGame()
        {
            gameOver = true;
            ShowToastNotification("Wygrana!");
            Discover_Map();
        }

        private void ShowToastNotification(String message)
        {
            ToastTemplateType toastTemplate = ToastTemplateType.ToastImageAndText01;
            XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(toastTemplate);

            // Set Text
            XmlNodeList toastTextElements = toastXml.GetElementsByTagName("text");
            toastTextElements[0].AppendChild(toastXml.CreateTextNode(message));

            // toast duration
            IXmlNode toastNode = toastXml.SelectSingleNode("/toast");
            ((XmlElement)toastNode).SetAttribute("duration", "short");

            // toast navigation
            var toastNavigationUriString = "#/MainPage.xaml?param1=12345";
            var toastElement = ((XmlElement)toastXml.SelectSingleNode("/toast"));
            toastElement.SetAttribute("launch", toastNavigationUriString);

            // Create the toast notification based on the XML content you've specified.
            ToastNotification toast = new ToastNotification(toastXml);

            // Send your toast notification.
            ToastNotificationManager.CreateToastNotifier().Show(toast);
        }

        public MainPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Enabled;
        }
        

    }
}
